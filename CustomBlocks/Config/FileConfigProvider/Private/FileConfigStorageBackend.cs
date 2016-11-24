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
using DarkCaster.Config.Private;

using System.Security.Principal;
using System.Security.AccessControl;
using System.Collections.Generic;
using System.Security;

namespace DarkCaster.Config.Files.Private
{
	/// <summary>
	/// Storage backend for for FileConfigProvider.
	/// </summary>
	public class FileConfigStorageBackend : IConfigStorageBackend
	{
		private static readonly Dictionary<string, bool> cachedPerms = new Dictionary<string, bool>();
		private static readonly object cacheLocker = new object();
				
		public StorageBackendInitResponse Init(object initData)
		{
			if(!(initData is FileHelper))
				throw new ArgumentException("initData is not an internal FileHelper type", "initData");
			var helper=(FileHelper)initData;
			if(helper.error)
				throw (helper.exception ?? new Exception("File helper"));
			
			bool writeAllowed=true;
			byte[] rawConfigData=null;
			var currentFilename=helper.actualFilename;
			
			//try to read primary file contents
			try { rawConfigData=File.ReadAllBytes(currentFilename); }
			//catch cases, when file read error means that write is unavailable
			catch(DirectoryNotFoundException) { } //do not to catch this by IOException (parent)
			catch(FileNotFoundException) { } //do not to catch this by IOException (parent)
			catch(SecurityException) { writeAllowed=false; }
			catch(NotSupportedException) { writeAllowed=false; }
			catch(UnauthorizedAccessException) { writeAllowed=false; }
			catch(IOException) { writeAllowed=false; }
			catch(ArgumentNullException) { writeAllowed=false; }
			catch(ArgumentException) { writeAllowed=false; }
			
			//if file exist - check file write perms
			if(rawConfigData!=null)
			{
				//TODO
				//set writeAllowed=false if file is readonly
			}
			
			string baseDir=null;
			bool dirExist=false;
			try { baseDir=Path.GetDirectoryName(helper.actualFilename); }
			catch { writeAllowed=false; }
			
			//check primary location file's base dir exist
			if(writeAllowed && !string.IsNullOrEmpty(baseDir))
				dirExist=Directory.Exists(baseDir);
			
			//if dir exist - check file read\write\create\delete perms
			if(writeAllowed && dirExist)
			{
				//TODO
				//set writeAllowed=false if directory is readonly
			}
			
			//if dir not exist - try to create
			if(writeAllowed && !dirExist)
			{
				//TODO: recursive create directory
				//set writeAllowed=false on failure
			}
						
			//if primary file cannot be read, try read backup location[s], ignore any errors
			if(rawConfigData == null)
			foreach(var backupFilename in helper.backupFilenames)
			{
				currentFilename=backupFilename;
				try { rawConfigData=File.ReadAllBytes(currentFilename); }
				catch { currentFilename=null; continue; }
				break;
			}
			
			//construct init responce
			return new StorageBackendInitResponse( rawConfigData, writeAllowed, currentFilename );
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
		
		/*private bool CheckDirPerms(string path)
		{
			return false;
		}
		
		private bool CheckFilePerms(string path)
		{
			return false;
		}
		
		
		public bool Prepare(string path)
		{
			if(error)
				return false;
			try
			{
				//filename is directory
				if(Directory.Exists(path))
					return false;
				//check base directory
				var basePath=Path.GetDirectoryName(path);
				if(!Directory.Exists(basePath))
					return false;
				
				//TODO: test and fix on mono\linux ?
				var acl=Directory.GetAccessControl(basePath);
				if(acl==null)
					return false;
				var rules=acl.GetAccessRules(true, true, typeof(SecurityIdentifier));
            	var curUser = WindowsIdentity.GetCurrent();
            	foreach(FileSystemAccessRule rule in rules)
            	{
                	if (curUser.Groups.Contains(rule.IdentityReference) &&
            		   (FileSystemRights.CreateFiles & rule.FileSystemRights) == FileSystemRights.CreateFiles &&
            		   rule.AccessControlType == AccessControlType.Allow)
                    		return true;
                }
                
				if(File.Exists(path))
					return true;
				//check, that we have 
			}
			catch
			{
				return false;
			}
		}*/
	}
}
