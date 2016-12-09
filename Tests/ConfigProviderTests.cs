using System;
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
	}
}
