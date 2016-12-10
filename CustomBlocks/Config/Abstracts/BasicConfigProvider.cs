// BasicConfigProvider.cs
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
using DarkCaster.Config.Private;

namespace DarkCaster.Config
{
	/// <summary>
	/// Config provider base class, with basic abilities to store and restore config data.
	/// Main config storage methods performed by backend class, that should be provided by inerhited class.
	/// Data serialization is provided by ISerializationHelper class,
	/// instance of particular serializer should be provided as constructor parameter.
	/// </summary>
	public abstract class BasicConfigProvider<CFG> : IConfigProviderController<CFG>, IConfigProvider<CFG>
		where CFG: class, new()
	{
		private readonly ReaderWriterLockSlim opLock;
		private readonly ISerializationHelper<CFG> serializer;
		private readonly IConfigStorageBackendFactory backendFactory;
		private readonly ISafeEventCtrl<ConfigProviderStateEventArgs> stateEventCtl;
		private readonly ISafeEvent<ConfigProviderStateEventArgs> stateEvent;
		
		private IConfigStorageBackend backend;
		private ConfigProviderState state = ConfigProviderState.Offline;
		
		// IConfigProviderController's owner may use it in multithreaded scenarios, and share same instance between multiple threads 
		// so, add some protection for simultaneous and multiple dispose call, because dispose should be robust.
		// see https://msdn.microsoft.com/en-us/library/ms182334.aspx for more details (information about CA2202 warning)
		private readonly object disposeLock;
		private bool isDisposed;
		
		private BasicConfigProvider() {} 
		
		protected BasicConfigProvider(ISerializationHelper<CFG> serializer, IConfigStorageBackendFactory backendFactory)
		{
			this.serializer=serializer;
			this.backendFactory=backendFactory;
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
							throw new ConfigProviderException(backendFactory.GetId(), state, "This FileConfigProvider is offline, create new object", null);
						try { backend = backendFactory.Create(); }
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
					throw new ConfigProviderException(backendFactory.GetId(), state, "Backend failed to initialise", backendEx);
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
						try { backendFactory.Destroy(backend); }
						catch (Exception ex) { backendEx = ex; }
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
					throw new ConfigProviderException(backendFactory.GetId(), state, "Backend failed to deinitialize", backendEx);
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
					throw new ConfigProviderException(backendFactory.GetId(),state,"Config provider is not online",null);
				var data=serializer.Serialize(config);
				try { backend.Commit(data); }
				catch(Exception ex)	{ throw new ConfigProviderException(backendFactory.GetId(),state,"Failed to write config data",ex); }
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
					throw new ConfigProviderException(backendFactory.GetId(),state,"Config provider is not online",null);
				var data=serializer.Serialize(config);
				try { await backend.CommitAsync(data); }
				catch(Exception ex) { throw new ConfigProviderException(backendFactory.GetId(),state,"Failed to write config data",ex); }
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
					throw new ConfigProviderException(backendFactory.GetId(),state,"Config provider is not online",null);
				byte[] data=null;
				try{ data=backend.Fetch(); }
				catch(Exception ex) { throw new ConfigProviderException(backendFactory.GetId(),state,"Failed to read raw config data from backend",ex); }
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
					throw new ConfigProviderException(backendFactory.GetId(),state,"Config provider is not online",null);
				try{ backend.MarkForDelete(); }
				catch(Exception ex)
				{
					throw new ConfigProviderException(backendFactory.GetId(),state,"Failed to mark config to delete",ex);
				}
			}
			finally { opLock.ExitReadLock(); }
		}
		
		#endregion
	}
}
