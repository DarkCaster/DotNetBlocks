// FileConfigProvider.cs
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
using System.Threading;
using System.Threading.Tasks;
using DarkCaster.Events;
using DarkCaster.Serialization;

using DarkCaster.Config.Files.Private;
using DarkCaster.Config.Private;

namespace DarkCaster.Config.Files
{
	/// <summary>
	/// Config provider, that store config data as files at user's home directory.
	/// Data serialization is provided by ISerializationHelper class,
	/// instance of particular serializer should be provided as parameter at constructor.
	/// </summary>
	public sealed class FileConfigProvider<CFG> : IConfigProviderController<CFG>, IConfigProvider<CFG> where CFG: class, new()
	{
		private readonly ReaderWriterLockSlim opLock=new ReaderWriterLockSlim();
		private readonly ISerializationHelper<CFG> serializer;
		private readonly ConfigFileId fileId;
		private readonly bool bkIsExt;
		private IConfigStorageBackend backend;
		private ConfigProviderState state = ConfigProviderState.Offline;
		private volatile bool writeEnabled;
		
		private FileConfigProvider() {} 
		
		/// <summary>
		/// This constructor is not recommended for direct use. For now only used for testing purposes.
		/// </summary>
		[Obsolete("This constructor is not recommended for direct use. For now only used for testing purposes.")]
		public FileConfigProvider(ISerializationHelper<CFG> serializer, IConfigStorageBackend backend)
		{
			this.serializer=serializer;
			this.fileId=null;
			this.bkIsExt=true;
			this.backend=backend;
			this.writeEnabled=backend.IsWriteAllowed;
			this.state=ConfigProviderState.Init;
		}
		
		/// <summary>
		/// Create new FileConfigProvider instance.
		/// </summary>
		/// <param name="serializer">Serializer that will encode and decode config data into classes</param>
		/// <param name="dirName">Directory name for config files storage. Directory location is platform dependend.</param>
		/// <param name="id"></param>
		public FileConfigProvider(ISerializationHelper<CFG> serializer, string dirName, string id) : this(serializer, new ConfigFileId(dirName,id)) {}
		
		public FileConfigProvider(ISerializationHelper<CFG> serializer, string filename) : this(serializer, new ConfigFileId(filename)) {}
		
		internal FileConfigProvider(ISerializationHelper<CFG> serializer, ConfigFileId fileId) 
		{			
			this.serializer=serializer;
			this.fileId=fileId;
			this.bkIsExt=false;
			this.backend=null;
			this.writeEnabled=false;
			this.state=ConfigProviderState.Init;
		}
		
		public void Init()
		{
			opLock.EnterWriteLock();
			try
			{
				if( state == ConfigProviderState.Online )
					return;
				if( state == ConfigProviderState.Offline )
					throw new FileConfigProviderInitException(fileId.actualFilename, state, "This FileConfigProvider is offline, create new object", null);
				if(bkIsExt)
					state = ConfigProviderState.Online;
				else
					backend=FileConfigStorageBackendManager.GetBackend(fileId);
				//TODO: rise event
			}
			finally { opLock.ExitWriteLock(); }
		}
		
		public async Task InitAsync()
		{
			await Task.Run((Action)Init);
		}
		
		public void Shutdown()
		{
			throw new NotImplementedException("TODO:");
		}
		
		public IConfigProvider<CFG> GetProvider()
		{
			return (IConfigProvider<CFG>)this;
		}
		
		public void Dispose()
		{
			throw new NotImplementedException("TODO:");
		}
		
		public bool IsWriteEnabled
		{
			get
			{
				throw new NotImplementedException("TODO:");
			}
		}
		
		public void WriteConfig(CFG config)
		{
			throw new NotImplementedException("TODO:");
		}
		
		public async Task WriteConfigAsync(CFG config)
		{
			throw new NotImplementedException("TODO:");
		}
		
		public IReadOnlyConfigProvider<CFG> GetReadOnlyProvider()
		{
			return (IReadOnlyConfigProvider<CFG>)this;
		}
		
		public ConfigProviderState State 
		{
			get
			{
				throw new NotImplementedException("TODO:");
			}
		}
		
		public ISafeEvent<ConfigProviderStateEventArgs> StateChangeEvent
		{
			get
			{
				throw new NotImplementedException("TODO:");
			}
		}
		
		public CFG ReadConfig()
		{
			throw new NotImplementedException("TODO:");
		}
		
		public void DeleteConfig()
		{
			throw new NotImplementedException("TODO:");
		}
	}
}
