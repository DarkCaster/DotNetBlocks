// MsgPackSerializationHelper.cs
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
using MsgPack;
using MsgPack.Serialization;
using DarkCaster.Converters;

namespace DarkCaster.Serialization
{
	/// <summary>
	/// Simple serialization helper class, that performs serializaion to/from MsgPack format.
	/// Using Base85 converter to deal with serialization to/from string.
	/// </summary>
	public sealed class MsgPackSerializationHelper<T> : ISerializationHelper, ISerializationHelper<T>
	{
		private readonly SerializationContext context;
		
		private MsgPackSerializationHelper() {}
		
		public MsgPackSerializationHelper(bool useForStorage)
		{
			context=new SerializationContext();
			if(useForStorage)
			{
				context.SerializationMethod=SerializationMethod.Map;
				context.EnumSerializationMethod=EnumSerializationMethod.ByName;
			}
			else
			{
				context.SerializationMethod = SerializationMethod.Array;
				context.EnumSerializationMethod = EnumSerializationMethod.ByUnderlyingValue;
			}
			context.CompatibilityOptions.PackerCompatibilityOptions=PackerCompatibilityOptions.None;
		}
		
		public byte[] SerializeObj(object target)
		{
			if(target==null)
				throw new MsgPackSerializationException(typeof(T),
					new ArgumentException("Object to serialize is NULL", "target"));
			if(!(target is T))
				throw new MsgPackSerializationException(typeof(T),
					new ArgumentException("Cannot serialize object with wrong type", "target"));	
			return Serialize((T)target);
		}

		public object DeserializeObj(byte[] data)
		{
			return Deserialize(data);
		}

		public byte[] Serialize(T target)
		{
			try
			{
				if(!typeof(T).IsValueType && target==null)
					throw new ArgumentException("Object to serialize is NULL");
				using(var stream = new MemoryStream())
				{
					var serializer = MessagePackSerializer.Get<T>(context);
					serializer.Pack(stream, target);
					return stream.ToArray();
				}
			}
			catch(Exception ex)
			{
				throw new MsgPackSerializationException(typeof(T), ex);
			}
		}

		public T Deserialize(byte[] data)
		{
			try
			{
				if( data==null || data.Length==0 )
					throw new ArgumentException("Could not deserialize object from empty data", "data");
				using(var stream = new MemoryStream(data))
				{
					var serializer = MessagePackSerializer.Get<T>(context);
					var result=serializer.Unpack(stream);
					if(!typeof(T).IsValueType && result==null)
						throw new Exception("Deserialized object is null!");
					return result;
				}
			}
			catch(Exception ex)
			{
				throw new MsgPackDeserializationException(typeof(T), ex);
			}
		}

		public string SerializeToString(T target)
		{
			var bytes = Serialize(target);
			try
			{
				return Base85.Encode(bytes);
			}
			catch(Exception ex)
			{
				throw new MsgPackSerializationException(typeof(T), ex);
			}
		}

		public T Deserialize(string data)
		{
			byte[] bytes;
			try
			{
				bytes = Base85.Decode(data);
			}
			catch(Exception ex)
			{
				throw new MsgPackDeserializationException(typeof(T), ex);
			}
			return Deserialize(bytes);
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
				throw new MsgPackSerializationException(typeof(T), ex);
			}
		}

		public object DeserializeObj(string data)
		{
			return Deserialize(data);
		}
	}
}
