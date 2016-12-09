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
			var backendMock=new MockConfigProviderBackend(true, null, 0.0f);
			var providerCtl=new FileConfigProvider<MockConfig>(new MockSerializationHelper<MockConfig>(), backendMock);
			ConfigProviderTests.Init(providerCtl);
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			backendMock.Dispose();
		}
		
		[Test]
		public void ReadNull()
		{
			var backendMock=new MockConfigProviderBackend(true, null, 0.0f);
			var providerCtl=new FileConfigProvider<MockConfig>(new MockSerializationHelper<MockConfig>(), backendMock);
			ConfigProviderTests.Read(providerCtl, typeof(FileConfigProviderReadException));
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			Assert.LessOrEqual(2,backendMock.FetchCount);
			backendMock.Dispose();
		}
		
		[Test]
		public void Read()
		{
			var check=new MockConfig();
			check.Randomize();
			var serializer=new MockSerializationHelper<MockConfig>();
			var data=serializer.Serialize(check);
			var backendMock=new MockConfigProviderBackend(true, data, 0.0f);
			var providerCtl=new FileConfigProvider<MockConfig>(serializer, backendMock);
			var verify=ConfigProviderTests.Read(providerCtl, typeof(FileConfigProviderReadException));
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			Assert.LessOrEqual(2,backendMock.FetchCount);
			backendMock.Dispose();
			Assert.AreNotSame(check,verify);
			Assert.AreEqual(check,verify);
		}
		
	}
	#pragma warning restore 618
}
