// FileConfigProviderTests.cs
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
using NUnit.Framework;
using Tests.Mocks;
using DarkCaster.Serialization;
using DarkCaster.Config;
using DarkCaster.Config.Files;

namespace Tests
{
	//disable warning about obsolete FileConfigProvider's constructor used for tests. 
	#pragma warning disable 618
	/// <summary>
	/// Test for FileConfigProvider
	/// </summary>
	[TestFixture]
	public class FileConfigProviderTests
	{
		
		[Test]
		public void Init()
		{
			/*var backendMock=new MockConfigProviderBackend();
			//create provider
			var providerCtl=new FileConfigProvider<MockConfig>(new MockSerializationHelper<MockConfig>(), backendMock, "Tests", "Test.cfg");
			//check state
			Assert.AreEqual(ConfigProviderState.Init, providerCtl.State);
			//creating another provider with same domain and id must throw exception because of resource conflict
			Assert.Throws(typeof(FileConfigProviderInitException),()=>new FileConfigProvider<MockConfig>(new MockSerializationHelper<MockConfig>(), backendMock, "Tests", "Test.cfg"));
			//check state
			Assert.AreEqual(ConfigProviderState.Init, providerCtl.State);
			//switch to offline
			providerCtl.Shutdown();
			//check state
			Assert.AreEqual(ConfigProviderState.Offline, providerCtl.State);
			//dispose
			providerCtl.Dispose();*/
		}
	}
	#pragma warning restore 618
}
