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
using NUnit.Framework;
using DarkCaster.DataTransfer.Config;
using DarkCaster.Serialization;
using DarkCaster.Serialization.Binary;
using DarkCaster.Serialization.Json;
using DarkCaster.Serialization.MsgPack;

namespace Tests
{
	[TestFixture]
	public class TunnelConfigTests
	{
		/*
		 * //create original config
			ITunnelConfig config = factory.CreateNew();
			int iTestVal = new Random().Next();
			config.Set("iTest", iTestVal);
			uint uiTestVal = (uint)(new Random().Next());
			config.Set("uiTest", uiTestVal);
			long lTestVal = new Random().Next();
			config.Set("lTest", lTestVal);
			ulong ulTestVal = (ulong)(new Random().Next());
			config.Set("ulTest", ulTestVal);
			bool bTestVal = (new Random().NextDouble()) > 0.5;
			config.Set("bTest", bTestVal);
			var byTestVal=new byte[10]; new Random().NextBytes(byTestVal);
			config.Set("byTest", byTestVal);
			float fTestVal = (float)(new Random().NextDouble());
			config.Set("fTest", fTestVal);
			double dTestVal = new Random().NextDouble();
			config.Set("dTest", dTestVal);
			string sTestVal = "test";
			config.Set("sTest", sTestVal);
		*/

		[Test]
		public void Int_GetSet()
		{
			var factory = new TunnelConfigFactory(new BinarySerializationHelperFactory());
			var config = factory.CreateNew();
			int val = new Random().Next();
			CommonTunnelConfigTests.ITunnelConfig_CorrectSetGet(config,val);
		}

		[Test]
		public void UInt_GetSet()
		{
			var factory = new TunnelConfigFactory(new BinarySerializationHelperFactory());
			var config = factory.CreateNew();
			uint val = (uint)(new Random().Next());
			CommonTunnelConfigTests.ITunnelConfig_CorrectSetGet(config, val);
		}

		[Test]
		public void Long_GetSet()
		{
			var factory = new TunnelConfigFactory(new BinarySerializationHelperFactory());
			var config = factory.CreateNew();
			long val = new Random().Next();
			CommonTunnelConfigTests.ITunnelConfig_CorrectSetGet(config, val);
		}

		[Test]
		public void ULong_GetSet()
		{
			var factory = new TunnelConfigFactory(new BinarySerializationHelperFactory());
			var config = factory.CreateNew();
			ulong val = (ulong)(new Random().Next());
			CommonTunnelConfigTests.ITunnelConfig_CorrectSetGet(config, val);
		}

		[Test]
		public void Float_GetSet()
		{
			var factory = new TunnelConfigFactory(new BinarySerializationHelperFactory());
			var config = factory.CreateNew();
			float val = (float)new Random().NextDouble();
			CommonTunnelConfigTests.ITunnelConfig_CorrectSetGet(config, val);
		}

		[Test]
		public void Double_GetSet()
		{
			var factory = new TunnelConfigFactory(new BinarySerializationHelperFactory());
			var config = factory.CreateNew();
			double val = new Random().NextDouble();
			CommonTunnelConfigTests.ITunnelConfig_CorrectSetGet(config, val);
		}

		[Test]
		public void Byte_GetSet()
		{
			var factory = new TunnelConfigFactory(new BinarySerializationHelperFactory());
			var config = factory.CreateNew();
			byte[] val = new byte[50]; new Random().NextBytes(val);
			CommonTunnelConfigTests.ITunnelConfig_CorrectSetGet(config, val);
		}

		[Test]
		public void Bool_GetSet()
		{
			var factory = new TunnelConfigFactory(new BinarySerializationHelperFactory());
			var config = factory.CreateNew();
			bool val = (new Random().NextDouble()) > 0.5;
			CommonTunnelConfigTests.ITunnelConfig_CorrectSetGet(config, val);
		}

		[Test]
		public void String_GetSet()
		{
			var factory = new TunnelConfigFactory(new BinarySerializationHelperFactory());
			var config = factory.CreateNew();
			string val = "The quick brown fox jumps over the lazy dog";
			CommonTunnelConfigTests.ITunnelConfig_CorrectSetGet(config, val);
		}
	}
}
