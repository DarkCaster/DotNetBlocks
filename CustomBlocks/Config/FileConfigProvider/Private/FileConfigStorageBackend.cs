// FileConfigStorageBackend.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2016 DarkCaster <dark.caster@outlook.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;
using System.Security.AccessControl;

using DarkCaster.Config.Private;

namespace DarkCaster.Config.Files.Private
{
	/// <summary>
	/// Storage backend for for FileConfigProvider.
	/// </summary>
	public class FileConfigStorageBackend : IConfigStorageBackend
	{
		[Serializable]
		public struct InitDebug
		{
			public Exception[] Ex;
			public string Filename;
			public InitDebug(Exception[] exList, string filename)
			{
				this.Ex = exList;
				this.Filename = filename;
			}
		}
		
		private volatile string filename=null;
		
		private static readonly Dictionary<string, bool> cachedPerms = new Dictionary<string, bool>();
		private static readonly object cacheLocker = new object();
		
		private static byte[] ReadCfgFile(string target, ref bool writeAllowed, ICollection<Exception> debug)
		{
			byte[] result=null;
			try { result=File.ReadAllBytes(target); }
			//catch cases, when file read error means that write is unavailable
			catch(DirectoryNotFoundException) { } //this is ok
			catch(FileNotFoundException) { } //this is ok
			catch(Exception ex) { writeAllowed=false; debug.Add(ex); } //any other exception is an error
			return result;
		}
		
		//TODO: check and create separate method for mono\linux
		//Based on http://stackoverflow.com/a/16032192
		public static bool CheckDirAccessRights(string dir, FileSystemRights accessRights, ICollection<Exception> debug)
		{
			bool isInRoleWithAccess = false;
			try 
			{
				var rules = Directory.GetAccessControl(dir).GetAccessRules(true, true, typeof(NTAccount));
				var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
				foreach(AuthorizationRule rule in rules)
				{
					var fsRule = rule as FileSystemAccessRule;
					if(fsRule == null)
						continue;
					if((fsRule.FileSystemRights & accessRights) > 0)
					{
						var ntAccount = rule.IdentityReference as NTAccount;
						if(ntAccount == null)
							continue;
						if(principal.IsInRole(ntAccount.Value))
						{
							if (fsRule.AccessControlType == AccessControlType.Deny)
								return false;
							isInRoleWithAccess = true;
						}
					}
				}
			}
			catch(Exception ex) { debug.Add(ex); return false; }
			return isInRoleWithAccess;
		}
		
		public StorageBackendInitResponse Init(object initData)
		{
			if(!(initData is FileHelper))
				throw new ArgumentException("initData is not an internal FileHelper type", "initData");
			var helper=(FileHelper)initData;
			if(helper.error)
				throw (helper.exception ?? new Exception("File helper"));
			
			var debug=new List<Exception>();
			bool writeAllowed=true;
			var currentFilename=helper.actualFilename;
			
			//try to read primary file contents
			byte[] rawConfigData=ReadCfgFile(currentFilename, ref writeAllowed, debug);
			//try to read temporary file contents
			if(rawConfigData==null)
			{
				currentFilename+=".new";
				rawConfigData=ReadCfgFile(currentFilename, ref writeAllowed, debug);
				//try to move this file over default
				if(rawConfigData!=null && writeAllowed)
				{
					try { File.Copy(currentFilename,helper.actualFilename,true); }
					catch(Exception ex) { writeAllowed=false; debug.Add(ex); };
				}
				//try to delete temporary file
				if(rawConfigData!=null && writeAllowed)
				{
					try { File.Delete(currentFilename); }
					catch(Exception ex) { writeAllowed=false; debug.Add(ex); };
				}
			}
			
			string baseDir=null;
			bool dirExist=false;
			
			if(writeAllowed)
			{
				try { baseDir=Path.GetDirectoryName(helper.actualFilename); }
				catch(Exception ex) { debug.Add(ex); writeAllowed=false; }
			}
			
			//check primary location file's base dir exist
			if(writeAllowed && !string.IsNullOrEmpty(baseDir))
			{
				//shoud not throw any exceptions according to docs
				try { dirExist=Directory.Exists(baseDir); }
				catch(Exception ex) { debug.Add(ex); writeAllowed=false; }
			}
			
			//if dir exist, check file read\write\create\delete perms
			if (writeAllowed && dirExist)
			{
				//try to get directory perms from cache
				lock (cacheLocker)
				{
					if (cachedPerms.ContainsKey(currentFilename))
						writeAllowed &= cachedPerms[currentFilename];
					else
					{
						writeAllowed &= (CheckDirAccessRights(baseDir,FileSystemRights.CreateFiles, debug) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Delete, debug) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Read, debug) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Write, debug));
						cachedPerms.Add(baseDir, writeAllowed);
					}
				}
			}
			//if dir not exist, try to create
			else if(writeAllowed && !dirExist)
			{
				try { Directory.CreateDirectory(baseDir); }
				catch(Exception ex) { debug.Add(ex); writeAllowed=false; }
				if(writeAllowed)
					lock (cacheLocker)
					{
						writeAllowed &= (CheckDirAccessRights(baseDir,FileSystemRights.CreateFiles, debug) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Delete, debug) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Read, debug) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Write, debug));
						cachedPerms[baseDir] = writeAllowed;
					}
			}
						
			//if primary file cannot be read, try read backup location[s], ignore any errors
			if(rawConfigData == null)
			foreach(var backupFilename in helper.backupFilenames)
			{
				currentFilename=backupFilename;
				try { rawConfigData=File.ReadAllBytes(currentFilename); }
				catch(DirectoryNotFoundException) { currentFilename=null; continue; } //do not to log this exception as error
				catch(FileNotFoundException) { currentFilename=null; continue; } //do not to log this exception as error
				//log all other exceptions, while reading backup config file
				catch(Exception ex) { debug.Add(ex); continue; }
				break;
			}
			
			var debugData=new InitDebug(debug.ToArray(), currentFilename);
			if(writeAllowed)
				filename=helper.actualFilename;
			
			//return init response
			return new StorageBackendInitResponse( rawConfigData, writeAllowed, debugData );
		}
		
		public void Commit(byte[] data)
		{
			throw new NotImplementedException("TODO");
		}
		
		public async Task CommitAsync(byte[] data)
		{
			throw new NotImplementedException("TODO");
		}
		
		public void Dispose()
		{
			throw new NotImplementedException("TODO");
		}
		
		public void Delete()
		{
			throw new NotImplementedException("TODO");
			//also delete cached file perms record
		}
	}
}
