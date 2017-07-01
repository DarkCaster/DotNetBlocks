// JsonSerializationHelper.cs
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
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using DarkCaster.Serialization.Private;
using DarkCaster.Compression;
using DarkCaster.Compression.FastLZ;

namespace DarkCaster.Serialization.Json
{
	public sealed class JsonSerializationHelper<T> : ISerializationHelper, ISerializationHelper<T>
	{
		private readonly IThreadSafeBlockCompressor compressor;

		public JsonSerializationHelper()
		{
			this.compressor = new FastLZBlockCompressor(16384, true);
		}

		public JsonSerializationHelper(IBlockCompressorFactory compFactory, int blockSz=16384)
		{
			throw new NotImplementedException("TODO");
		}

		public byte[] SerializeObj(object target)
		{
			var str = SerializeObjToString(target);
			try
			{
				var data = Encoding.UTF8.GetBytes(str);
				var output = new byte[MultiblockCompressionHelper.GetOutBuffSZ(data.Length, compressor)];
				var outLen = MultiblockCompressionHelper.Compress(data, data.Length, 0, output, 0, compressor);
				var result = new byte[outLen];
				Buffer.BlockCopy(output, 0, result, 0, outLen);
				return result;
			}
			catch(Exception ex)
			{
				throw new JsonSerializationException(typeof(T), ex);
			}
		}

		public int SerializeObj(object target, byte[] dest, int offset = 0)
		{
			var str = SerializeObjToString(target);
			try
			{
				var data = Encoding.UTF8.GetBytes(str);
				return MultiblockCompressionHelper.Compress(data, data.Length, 0, dest, offset, compressor);
			}
			catch(Exception ex)
			{
				throw new JsonSerializationException(typeof(T), ex);
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
			string str=null;
			try
			{
				var bytes = new byte[MultiblockCompressionHelper.DecodeDecomprSZ(data, offset)];
				MultiblockCompressionHelper.Decompress(data, offset, bytes, 0, compressor);
				str=Encoding.UTF8.GetString(bytes);
			}
			catch(Exception ex)
			{
				throw new JsonDeserializationException(typeof(T), ex);
			}
			return Deserialize(str);
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
				return JsonConvert.DeserializeObject<T>(data);
			}
			catch(Exception ex)
			{
				throw new JsonDeserializationException(typeof(T), ex);
			}
		}

		public string SerializeObjToString(object target)
		{
			try
			{
				if(!(target is T))
					throw new ArgumentException("Cannot serialize object with wrong type", nameof(target));
				return JsonConvert.SerializeObject(target, Formatting.Indented);
			}
			catch(Exception ex)
			{
				throw new JsonSerializationException(typeof(T), ex);
			}
		}

		public object DeserializeObj(string data)
		{
			return Deserialize(data);
		}
	}
}
