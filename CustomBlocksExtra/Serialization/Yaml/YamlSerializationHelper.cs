// YamlSerializationHelper.cs
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
using System.Text;
using YamlDotNet.Serialization;
using DarkCaster.Compression;
using DarkCaster.Serialization.Private;

namespace DarkCaster.Serialization.Yaml
{
	public sealed class YamlSerializationHelper<T> : ISerializationHelper, ISerializationHelper<T>
	{
		private readonly Serializer yamlSer = new SerializerBuilder().Build();
		private readonly Deserializer yamlDes = new DeserializerBuilder().Build();
		//TODO: private readonly IBlockCompressor compressor;

		public byte[] SerializeObj(object target)
		{
			try
			{
				if(!(target is T))
					throw new ArgumentException("Cannot serialize object with wrong type", nameof(target));
				var bytes=Encoding.UTF8.GetBytes(yamlSer.Serialize(target));
				//TODO: compress
				return bytes;
			}
			catch(Exception ex)
			{
				throw new YamlSerializationException(typeof(T), ex);
			}
		}

		public int SerializeObj(object target, byte[] dest, int offset = 0)
		{
			try
			{
				if(!(target is T))
					throw new ArgumentException("Cannot serialize object with wrong type", nameof(target));
				var bytes = Encoding.UTF8.GetBytes(yamlSer.Serialize(target));
				//TODO: compress
				Buffer.BlockCopy(bytes, 0, dest, offset, bytes.Length);
				return bytes.Length;
			}
			catch(Exception ex)
			{
				throw new YamlSerializationException(typeof(T), ex);
			}
		}

		public byte[] Serialize(T target)
		{
			return SerializeObj(target);
		}

		public int Serialize(T target, byte[] dest, int offset = 0)
		{
			return SerializeObj(target, dest, offset);
		}

		public T Deserialize(byte[] data, int offset = 0, int len = 0)
		{
			try
			{
				if (len == 0)
					len = data.Length - offset;
				//TODO: decompress
				var bytes = new byte[len];
				Buffer.BlockCopy(data, offset, bytes, 0, len);
				return yamlDes.Deserialize<T>(Encoding.UTF8.GetString(bytes));
			}
			catch(Exception ex)
			{
				throw new YamlDeserializationException(typeof(T), ex);
			}
		}

		public object DeserializeObj(byte[] data, int offset = 0, int len = 0)
		{
			return Deserialize(data, offset, len);
		}

		public string SerializeToString(T target)
		{
			return SerializeObjToString(target);
		}

		public T Deserialize(string data)
		{
			try
			{
				return yamlDes.Deserialize<T>(data);
			}
			catch(Exception ex)
			{
				throw new YamlDeserializationException(typeof(T), ex);
			}
		}

		public string SerializeObjToString(object target)
		{
			try
			{
				if(!(target is T))
					throw new ArgumentException("Cannot serialize object with wrong type", nameof(target));
				return yamlSer.Serialize(target);
			}
			catch(Exception ex)
			{
				throw new YamlSerializationException(typeof(T), ex);
			}
		}

		public object DeserializeObj(string data)
		{
			return Deserialize(data);
		}
	}
}
