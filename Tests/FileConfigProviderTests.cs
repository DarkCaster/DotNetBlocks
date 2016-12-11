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
using System.Threading.Tasks;
using NUnit.Framework;
using Tests.Mocks;
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
			var backendFactory=new MockConfigBackendFactory(true, null, 0.0f);
			var backendMock=backendFactory.Create() as MockConfigBackend;
			var providerCtl=new FileConfigProvider<MockConfig>(new MockSerializationHelper<MockConfig>(), backendFactory);
			ConfigProviderTests.Init(providerCtl);
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			backendFactory.Destroy(backendMock);
		}
		
		[Test]
		public void ReadNull()
		{
			var backendFactory=new MockConfigBackendFactory(true, null, 0.0f);
			var backendMock=backendFactory.Create() as MockConfigBackend;
			var serializer=new MockSerializationHelper<MockConfig>();
			var providerCtl=new FileConfigProvider<MockConfig>(serializer, backendFactory);
			var check=ConfigProviderTests.Read(providerCtl, typeof(FileConfigProviderReadException));
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			Assert.LessOrEqual(2,backendMock.FetchCount);
			backendFactory.Destroy(backendMock);
			Assert.AreEqual(check.randomInt,MockConfig.intDefault);
			Assert.AreEqual(check.randomString,MockConfig.stringDefault);
			Assert.AreEqual(0,serializer.DataStorageCount);
		}
		
		[Test]
		public void Read()
		{
			var check=new MockConfig();
			check.Randomize();
			Assert.AreNotEqual(check.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(check.randomString,MockConfig.stringDefault);
			var serializer=new MockSerializationHelper<MockConfig>();
			var data=serializer.Serialize(check);
			var backendFactory=new MockConfigBackendFactory(true, data, 0.0f);
			var backendMock=backendFactory.Create() as MockConfigBackend;
			var providerCtl=new FileConfigProvider<MockConfig>(serializer, backendFactory);
			var verify=ConfigProviderTests.Read(providerCtl, typeof(FileConfigProviderReadException));
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			Assert.LessOrEqual(2,backendMock.FetchCount);
			backendFactory.Destroy(backendMock);
			Assert.AreNotSame(check,verify);
			Assert.AreNotEqual(check.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(check.randomString,MockConfig.stringDefault);
			Assert.AreEqual(check,verify);
			Assert.AreEqual(1,serializer.DataStorageCount);
		}
		
		[Test]
		public void Write()
		{
			var check=new MockConfig();
			check.Randomize();
			Assert.AreNotEqual(check.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(check.randomString,MockConfig.stringDefault);
			var serializer=new MockSerializationHelper<MockConfig>();
			var backendFactory=new MockConfigBackendFactory(true, null, 0.0f);
			var backendMock=backendFactory.Create() as MockConfigBackend;
			var providerCtl=new FileConfigProvider<MockConfig>(serializer, backendFactory);
			var verify=ConfigProviderTests.WriteRead(providerCtl, typeof(FileConfigProviderReadException), check);
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			Assert.LessOrEqual(1,backendMock.FetchCount);
			Assert.LessOrEqual(1,backendMock.WriteCount);
			backendFactory.Destroy(backendMock);
			Assert.AreNotSame(check,verify);
			Assert.AreEqual(check,verify);
			Assert.AreNotEqual(verify.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(verify.randomString,MockConfig.stringDefault);
			Assert.AreEqual(1,serializer.DataStorageCount);
		}
		
		[Test]
		public void WriteFail()
		{
			var check=new MockConfig();
			check.Randomize();
			Assert.AreNotEqual(check.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(check.randomString,MockConfig.stringDefault);
			var serializer=new MockSerializationHelper<MockConfig>();
			var backendFactory=new MockConfigBackendFactory(true, null, 1.0f);
			var backendMock=backendFactory.Create() as MockConfigBackend;
			var providerCtl=new FileConfigProvider<MockConfig>(serializer, backendFactory);
			ConfigProviderTests.WriteReadFail(providerCtl, typeof(FileConfigProviderReadException), typeof(FileConfigProviderWriteException), check);
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			Assert.LessOrEqual(1,backendMock.FetchCount);
			Assert.LessOrEqual(1,backendMock.WriteCount);
			backendFactory.Destroy(backendMock);
			Assert.AreEqual(1,serializer.DataStorageCount);
		}
		
		[Test]
		public void WriteAsyncFail()
		{
			var check=new MockConfig();
			check.Randomize();
			Assert.AreNotEqual(check.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(check.randomString,MockConfig.stringDefault);
			var serializer=new MockSerializationHelper<MockConfig>();
			var backendFactory=new MockConfigBackendFactory(true, null, 1.0f);
			var backendMock=backendFactory.Create() as MockConfigBackend;
			var providerCtl=new FileConfigProvider<MockConfig>(serializer, backendFactory);
			Task task=Task.Run(async ()=> await ConfigProviderTests.WriteReadFailAsync(providerCtl, typeof(FileConfigProviderReadException), typeof(FileConfigProviderWriteException), check));
			task.Wait();
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			Assert.LessOrEqual(1,backendMock.FetchCount);
			Assert.LessOrEqual(1,backendMock.WriteCount);
			backendFactory.Destroy(backendMock);
			Assert.AreEqual(1,serializer.DataStorageCount);
		}
		
		[Test]
		public void WriteAsync()
		{
			var check=new MockConfig();
			check.Randomize();
			Assert.AreNotEqual(check.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(check.randomString,MockConfig.stringDefault);
			var serializer=new MockSerializationHelper<MockConfig>();
			var backendFactory=new MockConfigBackendFactory(true, null, 0.0f);
			var backendMock=backendFactory.Create() as MockConfigBackend;
			var providerCtl=new FileConfigProvider<MockConfig>(serializer, backendFactory);
			var task=Task<MockConfig>.Run(async ()=> await ConfigProviderTests.WriteReadAsync(providerCtl, typeof(FileConfigProviderReadException), check));
			var verify=task.Result;
			providerCtl.Dispose();
			Assert.LessOrEqual(1,backendMock.WriteAllowedCount);
			Assert.LessOrEqual(1,backendMock.FetchCount);
			Assert.LessOrEqual(1,backendMock.WriteCount);
			backendFactory.Destroy(backendMock);
			Assert.AreNotSame(check,verify);
			Assert.AreEqual(check,verify);
			Assert.AreNotEqual(verify.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(verify.randomString,MockConfig.stringDefault);
			Assert.AreEqual(1,serializer.DataStorageCount);
		}
		
		[Test]
		public void MultiRead()
		{
			const int workerCount=8;
			const int iterations=25000;
			var check=new MockConfig();
			check.Randomize();
			Assert.AreNotEqual(check.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(check.randomString,MockConfig.stringDefault);
			var serializer=new MockSerializationHelper<MockConfig>();
			var data=serializer.Serialize(check);
			var backendFactory=new MockConfigBackendFactory(false, data, 0.0f);
			var backendMock=backendFactory.Create() as MockConfigBackend;
			var providerCtl=new FileConfigProvider<MockConfig>(serializer, backendFactory);
			ConfigProviderTests.MultiThreadRead(providerCtl,check,workerCount,iterations);
			providerCtl.Dispose();
			Assert.AreEqual(1,backendMock.WriteAllowedCount);
			Assert.AreEqual(workerCount*iterations,backendMock.FetchCount);
			backendFactory.Destroy(backendMock);
			Assert.AreEqual(1,serializer.DataStorageCount);
		}
		
		[Test]
		public void MultiReadWrite()
		{
			const int workerCount=8;
			const int iterations=25000;
			var check=new MockConfig();
			check.Randomize();
			Assert.AreNotEqual(check.randomInt,MockConfig.intDefault);
			Assert.AreNotEqual(check.randomString,MockConfig.stringDefault);
			var serializer=new MockSerializationHelper<MockConfig>();
			var backendFactory=new MockConfigBackendFactory(true, null, 0.0f);
			var backendMock=backendFactory.Create() as MockConfigBackend;
			var providerCtl=new FileConfigProvider<MockConfig>(serializer, backendFactory);
			ConfigProviderTests.MultiThreadReadWrite(providerCtl,check,workerCount,iterations);
			providerCtl.Dispose();
			Assert.AreEqual(2,backendMock.WriteAllowedCount);
			Assert.AreEqual(workerCount*iterations,backendMock.FetchCount);
			Assert.AreEqual(workerCount*iterations,backendMock.WriteCount);
			backendFactory.Destroy(backendMock);
			Assert.AreEqual(workerCount*iterations,serializer.DataStorageCount);
		}
		
	}
	#pragma warning restore 618
}
