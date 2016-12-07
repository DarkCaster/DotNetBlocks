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
	public class FileConfigStorageBackend : IConfigStorageBackend, IDisposable
	{
		private readonly string filename;
		private readonly bool writeAllowed;
		private volatile bool deleteOnExit;
		private byte[] cachedData;
		
		private readonly ReaderWriterLockSlim readLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);
		
		private static readonly Dictionary<string, bool> cachedPerms = new Dictionary<string, bool>();
		private static readonly object cacheLocker = new object();
		
		private static byte[] ReadCfgFile(string target, ref bool writeAllowed)
		{
			byte[] result=null;
			try { result=File.ReadAllBytes(target); }
			catch(DirectoryNotFoundException) { } //this is ok
			catch(FileNotFoundException) { } //this is ok
			//any other exception is an error and write is unavailable
			catch(Exception) { writeAllowed=false; }
			return result;
		}
		
		//TODO: check and create separate method for mono\linux
		//Based on http://stackoverflow.com/a/16032192
		private static bool CheckDirAccessRights(string dir, FileSystemRights accessRights)
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
			catch(Exception) { return false; }
			return isInRoleWithAccess;
		}
		
		private FileConfigStorageBackend() {}
		
		[Obsolete("This constructor is not for direct use. Use only for test purposes.")]
		public FileConfigStorageBackend(string testFileName) : this (new ConfigFileId(testFileName)) {}
		
		internal FileConfigStorageBackend(ConfigFileId id)
		{
			filename = id.actualFilename;
			writeAllowed = true;
			deleteOnExit = false;
			cachedData = null;
			
			if(string.IsNullOrEmpty(filename) || string.IsNullOrWhiteSpace(filename))
				writeAllowed=false;
			
			//only proceed to read primary config if config filename is not empty
			if(writeAllowed)
			{
				//try to read primary file contents
				cachedData=ReadCfgFile(filename, ref writeAllowed);
				//try to read temporary file contents
				if(cachedData==null)
				{
					var newFilename=filename+".new";
					cachedData=ReadCfgFile(newFilename, ref writeAllowed);
					//try to move this file to default
					if(cachedData!=null && writeAllowed)
					{
						//id.actualFilename should not exist, because we already failed to read it!
						try { File.Move(newFilename,filename); }
						catch(Exception) { writeAllowed=false; }
					}
				}
			}
			
			string baseDir=null;
			bool dirExist=false;
			
			if(writeAllowed)
			{
				try { baseDir=Path.GetDirectoryName(filename); }
				catch(Exception) { writeAllowed=false; }
			}
			
			//check primary location file's base dir exist
			if(writeAllowed && !string.IsNullOrEmpty(baseDir))
			{
				//shoud not throw any exceptions according to docs
				try { dirExist=Directory.Exists(baseDir); }
				catch(Exception) { writeAllowed=false; }
			}
			
			//if dir exist, check file read\write\create\delete perms
			if (writeAllowed && dirExist)
			{
				//try to get directory perms from cache
				lock (cacheLocker)
				{
					if (cachedPerms.ContainsKey(baseDir))
						writeAllowed &= cachedPerms[baseDir];
					else
					{
						writeAllowed &= (CheckDirAccessRights(baseDir,FileSystemRights.CreateFiles) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Delete) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Read) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Write));
						cachedPerms.Add(baseDir, writeAllowed);
					}
				}
			}
			//if dir not exist, try to create
			else if(writeAllowed && !dirExist)
			{
				try { Directory.CreateDirectory(baseDir); }
				catch(Exception) { writeAllowed=false; }
				if(writeAllowed)
					lock (cacheLocker)
					{
						writeAllowed &= (CheckDirAccessRights(baseDir,FileSystemRights.CreateFiles) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Delete) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Read) &&
						                 CheckDirAccessRights(baseDir,FileSystemRights.Write));
						cachedPerms[baseDir] = writeAllowed;
					}
			}
						
			//if primary file still cannot be read, try read backup location[s], ignore any errors
			if(cachedData == null)
			foreach(var backupFilename in id.backupFilenames)
			{
				bool tmpWriteAllowed=true;
				cachedData=ReadCfgFile(backupFilename, ref tmpWriteAllowed);
				if(cachedData!=null)
					break;
			}
		}
		
		public bool IsWriteAllowed { get { return writeAllowed; } }
		
		public byte[] Fetch()
		{
			readLock.EnterReadLock();
			try
			{
				if(cachedData!=null)
				{
					var result=new byte[cachedData.Length];
					Buffer.BlockCopy(cachedData,0,result,0,cachedData.Length);
					return result;
				}
				return null;
			}
			finally{ readLock.ExitReadLock(); }
		}
		
		public void Commit(byte[] data)
		{
			if(data == null)
				throw new ArgumentNullException("data","Cannot write null data!");
			writeLock.Wait();
			try
			{
				if(!writeAllowed)
					throw new Exception("Write is not allowed!");
				//write to new location
				var newTarget=filename+".new";
				using(var stream = new FileStream(newTarget, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
				{
					stream.Write(data, 0, data.Length);
					//flush to disk, to ensure data safety during next steps
					stream.Flush(true);
				}
				//because metadata operations on journalled filesystems are atomic, and file data already flushed to disk
				//following steps should ensure that we will get at least one file with undamaged data in case of power failure
				//1. delete old file
				File.Delete(filename);
				//2. move new file in place of old one
				File.Move(newTarget,filename);
				//populate data cache
				readLock.EnterWriteLock();
				cachedData=data;
				readLock.ExitWriteLock();
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
				if(!writeAllowed)
					throw new Exception("Write is not allowed!");
				//write to new location
				var newTarget=filename+".new";
				//there is no async flush to disk, so, we will use WriteThrough mode, also set buffer size to 64KiB
				using(var stream = new FileStream(newTarget, FileMode.Create, FileAccess.Write, FileShare.Read, 65536, FileOptions.WriteThrough))
				{
					await stream.WriteAsync(data, 0, data.Length);
					await stream.FlushAsync();
				}
				//because metadata operations on journalled filesystems are atomic, and file data already flushed to disk
				//following steps should ensure that we will get at least one file with undamaged data in case of power failure
				//1. delete old file
				await Task.Run(()=>File.Delete(filename)); //TODO: recheck.
				//2. move new file in place of old one
				await Task.Run(()=>File.Move(newTarget,filename));
				//populate data cache
				readLock.EnterWriteLock();
				cachedData=data;
				readLock.ExitWriteLock();
			}
			finally { writeLock.Release(); }
		}
		
		public void Dispose()
		{
			if(deleteOnExit)
				Delete();
			writeLock.Dispose();
			readLock.Dispose();
		}
		
		public void MarkForDelete()
		{
			deleteOnExit=true;
		}
		
		private void Delete()
		{
			writeLock.Wait();
			try
			{
				if(!writeAllowed)
					return;
				var newTarget = filename + ".new";
				//delete new file, if exist
				try { File.Delete(newTarget); }
				catch(Exception) {}
				//delete old file, if exist
				try { File.Delete(filename); }
				catch(Exception) {}
			}
			finally { writeLock.Release(); }
		}
	}
}
