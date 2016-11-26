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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
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
		
		private volatile string filename = null;
		private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);
		
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
		private static bool CheckDirAccessRights(string dir, FileSystemRights accessRights, ICollection<Exception> debug)
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
			if(!(initData is ConfigFileId))
				throw new ArgumentException("initData is not an internal ConfigFileId type", "initData");
			var helper=(ConfigFileId)initData;
			
			var debug=new List<Exception>();
			var currentFilename=helper.actualFilename;
			bool writeAllowed=true;
			byte[] rawConfigData=null;
			
			if(string.IsNullOrEmpty(currentFilename) || string.IsNullOrWhiteSpace(currentFilename))
			{
				debug.Add(new Exception("Main config filename is empty, will not attempt to read or write it"));
				writeAllowed=false;
			}
			
			//only proceed to read primary config if config filename is not empty
			if(writeAllowed)
			{
				//try to read primary file contents
				rawConfigData=ReadCfgFile(currentFilename, ref writeAllowed, debug);
				//try to read temporary file contents
				if(rawConfigData==null)
				{
					currentFilename+=".new";
					rawConfigData=ReadCfgFile(currentFilename, ref writeAllowed, debug);
					//try to move this file to default
					if(rawConfigData!=null && writeAllowed)
					{
						//helper.actualFilename should not exist, because we already failed to read it!
						try { File.Move(currentFilename,helper.actualFilename); }
						catch(Exception ex) { writeAllowed=false; debug.Add(ex); };
					}
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
			
			if(writeAllowed)
				filename=helper.actualFilename;
			
			//return init response
			return new StorageBackendInitResponse( rawConfigData, writeAllowed, new InitDebug(debug.ToArray(), currentFilename) );
		}
		
		public void Commit(byte[] data)
		{
			if(data == null)
				throw new ArgumentNullException("data","Cannot write null data!");
			writeLock.Wait();
			try
			{
				var target=filename;
				if(target==null)
					throw new Exception("Target filename is not defined, because config write is not allowed!");
				if(target.Length == 0)
					throw new Exception("Target filename has zero length!");
				//write to new location
				var newTarget=target+".new";
				using(var stream = new FileStream(newTarget, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
				{
					stream.Write(data, 0, data.Length);
					//flush to disk, to ensure data safety during next steps
					stream.Flush(true);
				}
				//because metadata operations on journalled filesystems are atomic, and file data already flushed to disk
				//following steps should ensure that we will get at least one file with undamaged data in case of power failure
				//1. delete old file
				File.Delete(target);
				//2. move new file in place of old one
				File.Move(newTarget,target);
			}
			finally { writeLock.Release(); }
		}
		
		public async Task CommitAsync(byte[] data)
		{
			if(data == null)
				throw new ArgumentNullException("data","Cannot write null data!");
			await writeLock.WaitAsync();
			try
			{
				var target=filename;
				if(target==null)
					throw new Exception("Target filename is not defined, because config write is not allowed!");
				if(target.Length == 0)
					throw new Exception("Target filename has zero length!");
				//write to new location
				var newTarget=target+".new";
				//there is no async flush to disk, so, we will use WriteThrough mode, also set buffer size to 64KiB
				using(var stream = new FileStream(newTarget, FileMode.Create, FileAccess.Write, FileShare.Read, 65536, FileOptions.WriteThrough))
				{
					await stream.WriteAsync(data, 0, data.Length);
					await stream.FlushAsync();
				}
				//because metadata operations on journalled filesystems are atomic, and file data already flushed to disk
				//following steps should ensure that we will get at least one file with undamaged data in case of power failure
				//1. delete old file
				await Task.Run(()=>File.Delete(target));
				//2. move new file in place of old one
				await Task.Run(()=>File.Move(newTarget,target));
			}
			finally { writeLock.Release(); }
		}
		
		public void Dispose()
		{
			filename = null;
			//additional protection for semaphore usage during dispose (not 100% reliable)
			writeLock.Wait();
			writeLock.Release();
			//dispose writelock
			writeLock.Dispose();
		}
		
		public void Delete()
		{
			writeLock.Wait();
			try
			{
				var target = filename;
				if(target == null)
					throw new Exception("Target filename is not defined, maybe config write is not allowed, or file already deleted");
				if(target.Length == 0)
					throw new Exception("Target filename has zero length!");
				var newTarget = target + ".new";
				//delete new file, if exist
				File.Delete(newTarget);
				//delete old file, if exist
				File.Delete(target);
				filename = null;
			}
			finally { writeLock.Release(); }
		}
	}
}
