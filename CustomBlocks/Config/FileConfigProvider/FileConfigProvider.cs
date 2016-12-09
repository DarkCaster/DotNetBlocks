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
		private readonly ReaderWriterLockSlim opLock;
		private readonly ISerializationHelper<CFG> serializer;
		private readonly ConfigFileId fileId;
		private readonly bool bkIsExt;
		private readonly ISafeEventCtrl<ConfigProviderStateEventArgs> stateEventCtl;
		private readonly ISafeEvent<ConfigProviderStateEventArgs> stateEvent;
		
		private IConfigStorageBackend backend;
		private ConfigProviderState state = ConfigProviderState.Offline;
		
		// IConfigProviderController's owner may use it in multithreaded scenarios, and share same instance between multiple threads 
		// so, add some protection for simultaneous and multiple dispose call, because dispose should be robust.
		// see https://msdn.microsoft.com/en-us/library/ms182334.aspx for more details (information about CA2202 warning)
		private readonly object disposeLock;
		private bool isDisposed;
		
		private FileConfigProvider() {} 
		
		/// <summary>
		/// This constructor is not recommended for direct use. For now only used for testing purposes.
		/// </summary>
		[Obsolete("This constructor is not recommended for direct use. For now only used for testing purposes.")]
		public FileConfigProvider(ISerializationHelper<CFG> serializer, IConfigStorageBackend backend)
		{
			this.serializer=serializer;
			this.fileId=new ConfigFileId("test","test");
			this.bkIsExt=true;
			this.backend=backend;
			this.state=ConfigProviderState.Init;
			this.opLock=new ReaderWriterLockSlim();
			#if DEBUG
			var ev=new SafeEventDbg<ConfigProviderStateEventArgs>();
			#else
			var ev=new SafeEvent<ConfigProviderStateEventArgs>();
			#endif
			this.stateEventCtl=ev;
			this.stateEvent=ev;
			this.disposeLock=new object();
			this.isDisposed=false;
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
			this.state=ConfigProviderState.Init;
			this.opLock=new ReaderWriterLockSlim();
			#if DEBUG
			var ev=new SafeEventDbg<ConfigProviderStateEventArgs>();
			#else
			var ev=new SafeEvent<ConfigProviderStateEventArgs>();
			#endif
			this.stateEventCtl=ev;
			this.stateEvent=ev;
			this.disposeLock=new object();
			this.isDisposed=false;
		}
		
		#region IConfigProviderController
		
		private class InitCancelledException : Exception {}
		
		public void Init()
		{
			try
			{
				Exception backendEx=null;
				stateEventCtl.Raise
					(this, () => {
					opLock.EnterWriteLock();
					try
					{
						if (state == ConfigProviderState.Online)
							throw new InitCancelledException();
						if (state == ConfigProviderState.Offline)
							throw new FileConfigProviderInitException(fileId.actualFilename, state, "This FileConfigProvider is offline, create new object", null);
						try
						{
							if (!bkIsExt)
								backend = FileConfigStorageBackendManager.GetBackend(fileId);
						}
						catch (Exception ex)
						{
							backendEx = ex;
							state = ConfigProviderState.Offline;
							return new ConfigProviderStateEventArgs(ConfigProviderState.Offline, false);
						}
						state = ConfigProviderState.Online;
						return new ConfigProviderStateEventArgs(ConfigProviderState.Online, backend.IsWriteAllowed);
					}
					catch (Exception ex)
					{
						opLock.ExitWriteLock();
						throw ex;
					}
				}, opLock.ExitWriteLock);
				if(backendEx!=null)
					throw new FileConfigProviderInitException(fileId.actualFilename, state, "Backend failed to initialise", backendEx);
			}
			catch(InitCancelledException) {}
		}
		
		public void Shutdown()
		{
			try
			{
				Exception backendEx=null;
				stateEventCtl.Raise
				(this, () => {
					opLock.EnterWriteLock();
					try 
					{
						if (state == ConfigProviderState.Offline)
							throw new InitCancelledException();
						state = ConfigProviderState.Offline;
						try
						{
							if (!bkIsExt && backend != null)
								FileConfigStorageBackendManager.FlushBackend(backend);
						}
						catch (Exception ex)
						{
							backendEx = ex;
						}
						backend = null;
						return new ConfigProviderStateEventArgs(ConfigProviderState.Offline, false);
					}
					catch (Exception ex)
					{
						opLock.ExitWriteLock();
						throw ex;
					}
				}, opLock.ExitWriteLock);
				if(backendEx!=null)
					throw new FileConfigProviderDeinitException(fileId.actualFilename, state, "Backend failed to deinitialize", backendEx);
			}
			catch(InitCancelledException) {}
		}
		
		public IConfigProvider<CFG> GetProvider()
		{
			return (IConfigProvider<CFG>)this;
		}
		
		public IReadOnlyConfigProvider<CFG> GetReadOnlyProvider()
		{
			return (IReadOnlyConfigProvider<CFG>)this;
		}
		
		public void Dispose()
		{
			lock(disposeLock)
			{
				if(isDisposed)
					return;
				isDisposed=true;
				Shutdown();
				stateEventCtl.Dispose();
			}
		}
		
		#endregion
		
		#region IConfigProvider
		
		public bool IsWriteEnabled
		{
			get
			{
				opLock.EnterReadLock();
				try
				{
					if(state == ConfigProviderState.Init || state == ConfigProviderState.Offline)
						return false;
					return backend.IsWriteAllowed;
				}
				finally { opLock.ExitReadLock(); }
			}
		}
		
		public void WriteConfig(CFG config)
		{
			opLock.EnterReadLock();
			try
			{
				if(state == ConfigProviderState.Init || state == ConfigProviderState.Offline)
					throw new FileConfigProviderWriteException(fileId.actualFilename,state,"Config provider is not online",null);
				var data=serializer.Serialize(config);
				try { backend.Commit(data); }
				catch(Exception ex)
				{
					throw new FileConfigProviderWriteException(fileId.actualFilename,state,"Failed to write config data",ex);
				}
			}
			finally { opLock.ExitReadLock(); }
		}
		
		public async Task WriteConfigAsync(CFG config)
		{
			//should not lock for long, except when performing init\deinit (but write should not occur in such situations).
			//TODO: check, maybe it is ok to wrap this in separate Task()
			opLock.EnterReadLock();
			try
			{
				if(state == ConfigProviderState.Init || state == ConfigProviderState.Offline)
					throw new FileConfigProviderWriteException(fileId.actualFilename,state,"Config provider is not online",null);
				var data=serializer.Serialize(config);
				try { await backend.CommitAsync(data); }
				catch(Exception ex)
				{
					throw new FileConfigProviderWriteException(fileId.actualFilename,state,"Failed to write config data",ex);
				}
			}
			finally { opLock.ExitReadLock(); }
		}
		
		public ConfigProviderState State 
		{
			get
			{
				opLock.EnterReadLock();
				try { return state; }
				finally { opLock.ExitReadLock(); }
			}
		}
		
		public ISafeEvent<ConfigProviderStateEventArgs> StateChangeEvent { get { return stateEvent; } }
		
		public CFG ReadConfig()
		{
			opLock.EnterReadLock();
			try
			{
				if(state == ConfigProviderState.Init || state == ConfigProviderState.Offline)
					throw new FileConfigProviderReadException(fileId.actualFilename,state,"Config provider is not online",null);
				byte[] data=null;
				try{ data=backend.Fetch(); }
				catch(Exception ex)
				{
					throw new FileConfigProviderReadException(fileId.actualFilename,state,"Failed to read raw config data from backend",ex);
				}
				//create new CFG instance
				if(data == null || data.Length == 0)
					return new CFG();
				return serializer.Deserialize(data);
			}
			finally { opLock.ExitReadLock(); }
		}
		
		public void MarkConfigForDelete()
		{
			opLock.EnterReadLock();
			try
			{
				if(state == ConfigProviderState.Init || state == ConfigProviderState.Offline)
					throw new FileConfigProviderWriteException(fileId.actualFilename,state,"Config provider is not online",null);
				try{ backend.MarkForDelete(); }
				catch(Exception ex)
				{
					throw new FileConfigProviderWriteException(fileId.actualFilename,state,"Failed to mark config to delete",ex);
				}
			}
			finally { opLock.ExitReadLock(); }
		}
		
		#endregion
	}
}
