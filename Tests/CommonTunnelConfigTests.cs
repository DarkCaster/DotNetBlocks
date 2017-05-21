// TunnelConfigTests.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2017 DarkCaster <dark.caster@outlook.com>
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
using DarkCaster.Serialization;
using NUnit.Framework;
using DarkCaster.DataTransfer.Config;

namespace Tests
{
	public static class CommonTunnelConfigTests
	{
		public static void ITunnelConfigFactory_CreateNew(ITunnelConfigFactory factory)
		{
			ITunnelConfig config = null;
			Assert.DoesNotThrow(() => { config = factory.CreateNew(); });
			Assert.IsNotNull(config);
			ITunnelConfig config2 = null;
			Assert.DoesNotThrow(() => { config2 = factory.CreateNew(); });
			Assert.IsNotNull(config2);
			Assert.AreNotSame(config, config2);
		}

		public static void ITunnelConfigFactory_CreateSerializer(ITunnelConfigFactory factory)
		{
			ISerializationHelper<ITunnelConfig> serializer = null;
			Assert.DoesNotThrow(() => { serializer = factory.CreateSerializationHelper(); });
			Assert.IsNotNull(serializer);
			ISerializationHelper<ITunnelConfig> serializer2 = null;
			Assert.DoesNotThrow(() => { serializer2 = factory.CreateSerializationHelper(); });
			Assert.IsNotNull(serializer2);
			Assert.AreNotSame(serializer, serializer2);
		}

		public static void ITunnelConfigFactory_Serializer(ITunnelConfigFactory factory)
		{
			ISerializationHelper<ITunnelConfig> serializer = factory.CreateSerializationHelper();
			ITunnelConfig config = factory.CreateNew();
			int testVal = new Random().Next();
			config.Set("test",testVal);
			byte[] data = null;
			Assert.DoesNotThrow(()=> { data = serializer.Serialize(config); });
			Assert.IsNotNull(data);
			ITunnelConfig config2 = null;
			Assert.DoesNotThrow(()=> { config2 = serializer.Deserialize(data); });
			Assert.IsNotNull(config2);
			Assert.AreNotSame(config,config2);
			int testVal2 = (int)config2.Get("test");
			Assert.AreEqual(testVal, testVal2);
		}
	}
}
