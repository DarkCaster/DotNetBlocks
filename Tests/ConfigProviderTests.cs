using System;
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
			//check state
			Assert.AreEqual(ConfigProviderState.Init, provider.State);
			//write enabled 
			Assert.AreEqual(false, provider.IsWriteEnabled);
			//switch to offline
			providerCtl.Shutdown();
			//check state
			Assert.AreEqual(ConfigProviderState.Offline, provider.State);
		}
	}
}
