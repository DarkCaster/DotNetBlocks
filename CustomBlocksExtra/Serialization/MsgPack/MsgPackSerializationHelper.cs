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
using DarkCaster.Hash;

namespace DarkCaster.Serialization
{
	/// <summary>
	/// Mode of operation for MsgPackSerializationHelper.
	/// MsgPack-Cli could not ensure integrity of data and does not throw error when trying to deserialize broken data,
	/// sometimes producing incorrectly deserialized object instead.
	/// So, there are 2 modes that use additional checksum protection,
	/// to deny deserialization of damaged data and throw MsgPackDeserializationException in such cases.
	/// </summary>
	public enum MsgPackMode
	{
		/// <summary>
		/// Optimize for on-disk storage.
		/// Maximize compatibility with past and future MsgPack-Cli library versions, and possible serialized class changes.
		/// Produced data can be deserialized manually without use of MsgPackSerializationHelper.
		/// </summary>
		Storage=0,
		/// <summary>
		/// Optimize for on-disk storage. Also add checksum protection to ensure data integrity.
		/// Maximize compatibility with past and future MsgPack-Cli library versions, and possible serialized class changes.
		/// Produced data must be deserialized with MsgPackSerializationHelper.
		/// </summary>
		StorageCheckSum,
		/// <summary>
		/// Optimize for network data transfer. Maximize serialization performance, and minimize data size.
		/// Compatibility over different versions of MsgPack-Cli library is not guaranteed.
		/// Produced data can be deserialized manually without use of MsgPackSerializationHelper.
		/// </summary>
		Transfer,
		/// <summary>
		/// Optimize for network data transfer. Maximize serialization performance, and minimize data size.
		/// Also add checksum protection to ensure data integrity.
		/// Compatibility over different versions of MsgPack-Cli library is not guaranteed.
		/// Produced data must be deserialized with MsgPackSerializationHelper.
		/// </summary>
		TransferCheckSum
	}
	
	/// <summary>
	/// Simple serialization helper class, that performs serializaion to/from MsgPack format.
	/// Using Base85 converter to deal with serialization to/from string.
	/// </summary>
	public sealed class MsgPackSerializationHelper<T> : ISerializationHelper, ISerializationHelper<T>
	{
		private readonly MessagePackSerializer<T> serializer;
		private readonly bool useCheckSum;
		
		private MsgPackSerializationHelper() {}
		
		public MsgPackSerializationHelper(MsgPackMode mode = MsgPackMode.Storage)
		{
			var context=new SerializationContext();
			if( mode==MsgPackMode.Storage || mode==MsgPackMode.StorageCheckSum )
			{
				context.SerializationMethod = SerializationMethod.Map;
				context.EnumSerializationMethod = EnumSerializationMethod.ByName;
			}
			else
			{
				context.SerializationMethod = SerializationMethod.Array;
				context.EnumSerializationMethod = EnumSerializationMethod.ByUnderlyingValue;
			}
			context.CompatibilityOptions.PackerCompatibilityOptions=PackerCompatibilityOptions.None;
			if( mode==MsgPackMode.StorageCheckSum || mode==MsgPackMode.TransferCheckSum )
				useCheckSum=true;
			else
				useCheckSum=false;
			serializer = MessagePackSerializer.Get<T>(context);
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
					serializer.Pack(stream, target);
					if(useCheckSum)
					{
						var hash=MMHash32.GetHash(42,stream.ToArray());
						stream.WriteByte((byte)( hash & 0xff ));
						stream.WriteByte((byte)( hash >> 8 & 0xff));
						stream.WriteByte((byte)( hash >> 16 & 0xff));
						stream.WriteByte((byte)( hash >> 24 ));
					}
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
				var len = data.Length;
				if(useCheckSum)
				{
					if(data.Length<5)
						throw new ArgumentException("Data array is too small", "data");
					len -= 4;
					var check = unchecked((uint)( data[len] | data[len + 1] << 8 | data[len + 2] << 16 | data[len + 3] << 24 ));
					var hash = MMHash32.GetHash(42, data, 0, len);
					if(check!=hash)
						throw new Exception("Checksum do not match, cannot deserialize broken data!");
				}
				using(var stream = new MemoryStream(data,0,len))
				{
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
