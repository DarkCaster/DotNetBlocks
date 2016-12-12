// FileConfigBackendFactory.cs
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
using System.Collections.Generic;
using DarkCaster.Config.Private;

namespace DarkCaster.Config.Files.Private
{
	/// <summary>
	/// Factory for FileConfigStorageBackend
	/// </summary>
	public class FileConfigBackendFactory : IConfigBackendFactory
	{
		private readonly ConfigFileId fileId;
		
		private FileConfigBackendFactory() {}
		
		public FileConfigBackendFactory(string dirName, string id)
			: this(new ConfigFileId(dirName,id)) {}
		
		public FileConfigBackendFactory(string filename)
			: this(new ConfigFileId(filename)) {}
		
		internal FileConfigBackendFactory(ConfigFileId fileId) 
		{
			this.fileId = fileId;
		}
		
		private static readonly object metaLock=new object();
		private static readonly Dictionary<ConfigFileId, BackendInfo> backends = new Dictionary<ConfigFileId, BackendInfo>(31);
		private static readonly Dictionary<ConfigFileId, int> refCounters= new Dictionary<ConfigFileId, int>(31);
		
		private class BackendInfo
		{
			public readonly object locker = new object();
			public FileConfigBackend backend = null;
		}
		
		public string GetId()
		{
			return fileId.actualFilename;
		}
		
		public IConfigBackend Create()
		{
			BackendInfo info = null;
			lock(metaLock)
			{
				if(!refCounters.ContainsKey(fileId))
				{
					backends.Add(fileId, new BackendInfo());
					refCounters.Add(fileId, 0);
				}
				refCounters[fileId] = refCounters[fileId] + 1;
				info = backends[fileId];
			}
			
			//use different lockers for backends that serve different fileId
			//to allow concurrency when performing init for several different backends at same time
			lock(info.locker)
			{
				if(info.backend==null)
					info.backend=new FileConfigBackend(fileId);
				return info.backend;
			}
		}
		
		public void Destroy(IConfigBackend target)
		{
			if(target == null)
				throw new ArgumentNullException("target");
			if(!(target is FileConfigBackend))
				throw new ArgumentException("target is not a FileConfigBackend","target");
			var testTarget=(FileConfigBackend)target;
			lock(metaLock)
			{
				if(!refCounters.ContainsKey(fileId))
					throw new Exception("FileConfigBackendFactory.Destroy method triggered before FileConfigBackendFactory.Create");
				var locker=backends[fileId].locker;
				lock(locker)
				{
					var backend = backends[fileId].backend;
					if(backend != testTarget)
						throw new ArgumentException("Trying to destroy backend produced by different factory","target");
					if(refCounters[fileId] <= 1)
					{
						refCounters.Remove(fileId);
						backends.Remove(fileId);
						backend.Dispose();
					}
					else
						refCounters[fileId] = refCounters[fileId] - 1;
				}
			}
		}
	}
}
