// SerializationHelperProxy.cs
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
namespace DarkCaster.Serialization
{
	public class SerializationHelperProxy<ExtType> : ISerializationHelper<ExtType>
	{
		private readonly ISerializationHelper serializer;
		public SerializationHelperProxy(ISerializationHelper serializer)
		{
			this.serializer = serializer;
		}

		public byte[] Serialize(ExtType target)
		{
			return serializer.SerializeObj(target);
		}

		public string SerializeToString(ExtType target)
		{
			return serializer.SerializeObjToString(target);
		}

		public int Serialize(ExtType target, byte[] dest, int offset = 0)
		{
			return serializer.SerializeObj(target, dest, offset);
		}

		public ExtType Deserialize(byte[] data, int offset = 0, int len = 0)
		{
			return (ExtType)serializer.DeserializeObj(data, offset, len);
		}

		public ExtType Deserialize(string data)
		{
			return (ExtType)serializer.DeserializeObj(data);
		}
	}
}
