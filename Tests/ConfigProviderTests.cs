using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using DarkCaster.Config;

namespace Tests
{
	/// <summary>
	/// Common tests for config provider
	/// </summary>
	public static class ConfigProviderTests
	{
		public static void Init<CFG>(IConfigProviderController<CFG> providerCtl) where CFG: class, new()
		{
			var provider=providerCtl.GetProvider();
			Assert.AreEqual(ConfigProviderState.Init, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
			providerCtl.Init();
			Assert.AreEqual(ConfigProviderState.Online, provider.State);
			Assert.AreEqual(true, provider.IsWriteEnabled);
			providerCtl.Shutdown();
			Assert.AreEqual(ConfigProviderState.Offline, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
		}
		
		public static CFG Read<CFG>(IConfigProviderController<CFG> providerCtl, Type ReadExceptionType) where CFG: class, new()
		{
			var provider=providerCtl.GetProvider();
			Assert.AreEqual(ConfigProviderState.Init, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
			Assert.Throws(ReadExceptionType,()=>provider.ReadConfig());
			providerCtl.Init();
			Assert.AreEqual(ConfigProviderState.Online, provider.State);
			Assert.AreEqual(true, provider.IsWriteEnabled);
			var config=provider.ReadConfig();
			var config2=provider.ReadConfig();
			Assert.AreNotSame(config,config2); //references must differ
			providerCtl.Shutdown();
			Assert.AreEqual(ConfigProviderState.Offline, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
			return config;
		}
		
		public static CFG WriteRead<CFG>(IConfigProviderController<CFG> providerCtl, Type ReadExceptionType, CFG config) where CFG: class, new()
		{
			var provider=providerCtl.GetProvider();
			Assert.AreEqual(ConfigProviderState.Init, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
			Assert.Throws(ReadExceptionType,()=>provider.ReadConfig());
			providerCtl.Init();
			Assert.AreEqual(ConfigProviderState.Online, provider.State);
			Assert.AreEqual(true, provider.IsWriteEnabled);
			provider.WriteConfig(config);
			var config2=provider.ReadConfig();
			Assert.AreNotSame(config,config2); //references must differ
			providerCtl.Shutdown();
			Assert.AreEqual(ConfigProviderState.Offline, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
			return config2;
		}
		
		public static async Task<CFG> WriteReadAsync<CFG>(IConfigProviderController<CFG> providerCtl, Type ReadExceptionType, CFG config) where CFG: class, new()
		{
			var provider=providerCtl.GetProvider();
			Assert.AreEqual(ConfigProviderState.Init, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
			Assert.Throws(ReadExceptionType,()=>provider.ReadConfig());
			providerCtl.Init();
			Assert.AreEqual(ConfigProviderState.Online, provider.State);
			Assert.AreEqual(true, provider.IsWriteEnabled);
			await provider.WriteConfigAsync(config);
			var config2=provider.ReadConfig();
			Assert.AreNotSame(config,config2); //references must differ
			providerCtl.Shutdown();
			Assert.AreEqual(ConfigProviderState.Offline, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
			return config2;
		}
		
		public static class TestReader
		{
			public volatile static bool trigger=false;
		}
		
		public class TestReader<CFG>
			where CFG: class, IEquatable<CFG>, new()
		{
			private readonly IReadOnlyConfigProvider<CFG> provider;
			private readonly Thread worker; 
			private readonly int maxCount;
			private readonly CFG sample;
			
			public volatile bool ready;
			public volatile Exception result;
			
			public TestReader(IConfigProviderController<CFG> providerCtl, int readIterations, CFG sample)
			{
				this.provider=providerCtl.GetReadOnlyProvider();
				this.worker=new Thread(Worker);
				this.maxCount=readIterations;
				this.provider.StateChangeEvent.Subscribe(OnEvent);
				this.result=null;
				this.ready=false;
				this.sample=sample;
			}
			
			private void OnEvent(object sender, ConfigProviderStateEventArgs args)
			{
				try
				{
					if(args.State==ConfigProviderState.Online)
						worker.Start();
					if(args.State==ConfigProviderState.Offline)
						provider.StateChangeEvent.Unsubscribe(OnEvent);
				}
				catch(Exception ex) { result=ex; }
			}
			
			private void Worker()
			{
				ready=true;
				while(!TestReader.trigger) {}
				try
				{
					for(int i=0; i<maxCount; ++i)
					{
						var config=provider.ReadConfig();
						Assert.AreNotSame(sample,config);
						Assert.AreEqual(sample,config);
					}
				}
				catch(Exception ex) { result=ex; }
			}
			
			public void WaitForExit()
			{
				worker.Join();
			}
		}
		
		public static void MultiThreadRead<CFG>(IConfigProviderController<CFG> providerCtl, CFG sample, int workersCount, int iterations)
			where CFG: class, IEquatable<CFG>, new() 
		{
			var provider=providerCtl.GetProvider();
			Assert.AreEqual(ConfigProviderState.Init, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
			
			var workers=new TestReader<CFG>[workersCount];
			for(int i=0; i<workersCount; ++i)
				workers[i]=new TestReader<CFG>(providerCtl,iterations,sample);
			
			providerCtl.Init();
			Assert.AreEqual(ConfigProviderState.Online, provider.State);
			
			while(!((Func<bool>)(()=>{
				for(int i=0; i<workersCount; ++i)
					if(!workers[i].ready)
						return false;
				return true;
			}))()) {}
			
			TestReader.trigger=true;
			for(int i=0; i<workersCount; ++i)
			{
				workers[i].WaitForExit();
				if(workers[i].result!=null)
					throw workers[i].result;
			}
			
			providerCtl.Shutdown();
			Assert.AreEqual(ConfigProviderState.Offline, provider.State);
			Assert.AreEqual(false, provider.IsWriteEnabled);
		}
	}
}
