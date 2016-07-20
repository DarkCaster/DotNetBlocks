// BinarySerializationHelper.cs
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DarkCaster.Converters;
using DarkCaster.Serialization.Private;

namespace DarkCaster.Serialization
{
	/// <summary>
	/// Sample serialization herlper, using binary BinaryFormatter
	/// </summary>
	public sealed class BinarySerializationHelper<T> : ISerializationHelper, ISerializationHelper<T>
	{
		public byte[] SerializeObj(object target)
		{
			try
			{
				if(!(target is T))
					throw new ArgumentException("Cannot serialize object with wrong type", "target");
				using(var stream = new MemoryStream())
				{
					var formatter = new BinaryFormatter();
					formatter.Serialize(stream, (T)target);
					return stream.ToArray();
				}
			}
			catch(Exception ex)
			{
				throw new BinarySerializationException(typeof(T), ex);
			}
		}

		public int SerializeObj(object target, byte[] dest, int offset = 0)
		{
			try
			{
				if(!(target is T))
					throw new ArgumentException("Cannot serialize object with wrong type", "target");
				using(var stream = new ByteWriterStream(dest, offset))
				{
					var formatter = new BinaryFormatter();
					formatter.Serialize(stream, (T)target);
					return (int)stream.Length;
				}
			}
			catch(Exception ex)
			{
				throw new BinarySerializationException(typeof(T), ex);
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
			return (T)(DeserializeObj(data, offset, len));
		}

		public object DeserializeObj(byte[] data, int offset = 0, int len = 0)
		{
			try
			{
				using(var stream = new ByteReaderStream(data, offset, len))
				{
					var formatter = new BinaryFormatter();
					var result = formatter.Deserialize(stream);
					if(!(result is T))
						throw new ArgumentException("Deserialized object has wrong type", "data");
					return result;
				}
			}
			catch(Exception ex)
			{
				throw new BinaryDeserializationException(typeof(T), ex);
			}
		}

		public string SerializeToString(T target)
		{
			return SerializeObjToString(target);
		}

		public T Deserialize(string data)
		{
			return (T)DeserializeObj(data);
		}

		public string SerializeObjToString(object target)
		{
			var bytes = SerializeObj(target);
			try
			{
				return Base85.Encode(bytes);
			}
			catch(Exception ex)
			{
				throw new BinarySerializationException(typeof(T), ex);
			}
		}

		public object DeserializeObj(string data)
		{
			byte[] bytes;
			try
			{
				bytes = Base85.Decode(data);
			}
			catch(Exception ex)
			{
				throw new BinaryDeserializationException(typeof(T), ex);
			}
			return DeserializeObj(bytes);
		}
	}
}
