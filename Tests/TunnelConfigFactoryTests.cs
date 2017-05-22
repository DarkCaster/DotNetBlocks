// TunnelConfigFactoryTests.cs
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
	public class TunnelConfigFactoryTests
	{
		public static void TunnelConfigFactory_CreateNew(ISerializationHelperFactory sf)
		{
			CommonTunnelConfigTests.ITunnelConfigFactory_CreateNew(new TunnelConfigFactory(sf));
		}

		public static void TunnelConfigFactory_CreateSerializer(ISerializationHelperFactory sf)
		{
			CommonTunnelConfigTests.ITunnelConfigFactory_CreateSerializer(new TunnelConfigFactory(sf));
		}

		public static void TunnelConfigFactory_Serializer(ISerializationHelperFactory sf)
		{
			CommonTunnelConfigTests.ITunnelConfigFactory_Serializer(new TunnelConfigFactory(sf));
		}

		[Test]
		public void TunnelConfigFactory_Json_CreateNew()
		{
			TunnelConfigFactory_CreateNew(new JsonSerializationHelperFactory());
		}

		[Test]
		public void TunnelConfigFactory_Binary_CreateNew()
		{
			TunnelConfigFactory_CreateNew(new BinarySerializationHelperFactory());
		}

		[Test]
		public void TunnelConfigFactory_MsgPack_CreateNew()
		{
			TunnelConfigFactory_CreateNew(new MsgPackSerializationHelperFactory(MsgPackMode.Storage));
			TunnelConfigFactory_CreateNew(new MsgPackSerializationHelperFactory(MsgPackMode.StorageCheckSum));
			TunnelConfigFactory_CreateNew(new MsgPackSerializationHelperFactory(MsgPackMode.Transfer));
			TunnelConfigFactory_CreateNew(new MsgPackSerializationHelperFactory(MsgPackMode.TransferCheckSum));
		}

		[Test]
		public void TunnelConfigFactory_Json_CreateSerializer()
		{
			TunnelConfigFactory_CreateSerializer(new JsonSerializationHelperFactory());
		}

		[Test]
		public void TunnelConfigFactory_Binary_CreateSerializer()
		{
			TunnelConfigFactory_CreateSerializer(new BinarySerializationHelperFactory());
		}

		[Test]
		public void TunnelConfigFactory_MsgPack_CreateSerializer()
		{
			TunnelConfigFactory_CreateSerializer(new MsgPackSerializationHelperFactory(MsgPackMode.Storage));
			TunnelConfigFactory_CreateSerializer(new MsgPackSerializationHelperFactory(MsgPackMode.StorageCheckSum));
			TunnelConfigFactory_CreateSerializer(new MsgPackSerializationHelperFactory(MsgPackMode.Transfer));
			TunnelConfigFactory_CreateSerializer(new MsgPackSerializationHelperFactory(MsgPackMode.TransferCheckSum));
		}

		[Test]
		public void TunnelConfigFactory_Json_Serializer()
		{
			TunnelConfigFactory_Serializer(new JsonSerializationHelperFactory());
		}

		[Test]
		public void TunnelConfigFactory_Binary_Serializer()
		{
			TunnelConfigFactory_Serializer(new BinarySerializationHelperFactory());
		}

		[Test]
		public void TunnelConfigFactory_MsgPack_Serializer()
		{
			TunnelConfigFactory_Serializer(new MsgPackSerializationHelperFactory(MsgPackMode.Storage));
			TunnelConfigFactory_Serializer(new MsgPackSerializationHelperFactory(MsgPackMode.StorageCheckSum));
			TunnelConfigFactory_Serializer(new MsgPackSerializationHelperFactory(MsgPackMode.Transfer));
			TunnelConfigFactory_Serializer(new MsgPackSerializationHelperFactory(MsgPackMode.TransferCheckSum));
		}
	}
}
