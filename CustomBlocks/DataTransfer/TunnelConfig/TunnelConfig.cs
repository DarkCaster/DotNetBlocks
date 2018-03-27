// TunnelConfig.cs
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
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace DarkCaster.DataTransfer.Config
{
	[Serializable]
	public class TunnelConfig : ITunnelConfig
	{
		[Serializable]
		public class StorageRecord
		{
			public byte PType { get; set; }
			public byte[] Payload { get; set; }
		}

		private Dictionary<string, StorageRecord> storage=new Dictionary<string, StorageRecord>();

		public KeyValuePair<string, StorageRecord>[] Storage
		{
			get
			{
				lock (storage)
				{
					var tmp = new KeyValuePair<string, StorageRecord>[storage.Count];
					var pos = 0;
					foreach (var pair in storage)
						tmp[pos++] = pair;
					return tmp;
				}
			}
			set
			{
				storage = new Dictionary<string, StorageRecord>();
				lock(storage)
					for (int i = 0; i < value.Length; ++i)
						storage.Add(value[i].Key, value[i].Value);
			}
		}

		private static byte[] ConvertForBigEndian(byte[] input)
		{
			if (BitConverter.IsLittleEndian)
				return input;
			var reverse = new byte[input.Length];
			Buffer.BlockCopy(input,0,reverse,0,input.Length);
			Array.Reverse(reverse);
			return reverse;
		}

		private static StorageRecord WriteRecord(byte[] val)
		{
			var result = new StorageRecord()
			{
				PType = 0,
				Payload = val
			};
			return result;
		}

		private static StorageRecord WriteRecord(bool val)
		{
			var result = new StorageRecord()
			{
				PType = 1,
				Payload = new byte[1] { val ? (byte)1 : (byte)0 }
			};
			return result;
		}

		private static StorageRecord WriteRecord(int val)
		{
			var result = new StorageRecord()
			{
				PType = 2,
				Payload = ConvertForBigEndian(BitConverter.GetBytes(val))
			};
			return result;
		}

		private static StorageRecord WriteRecord(uint val)
		{
			var result = new StorageRecord()
			{
				PType = 3,
				Payload = ConvertForBigEndian(BitConverter.GetBytes(val))
			};
			return result;
		}

		private static StorageRecord WriteRecord(long val)
		{
			var result = new StorageRecord()
			{
				PType = 4,
				Payload = ConvertForBigEndian(BitConverter.GetBytes(val))
			};
			return result;
		}

		private static StorageRecord WriteRecord(ulong val)
		{
			var result = new StorageRecord()
			{
				PType = 5,
				Payload = ConvertForBigEndian(BitConverter.GetBytes(val))
			};
			return result;
		}

		private static StorageRecord WriteRecord(float val)
		{
			var result = new StorageRecord()
			{
				PType = 6,
				Payload = ConvertForBigEndian(BitConverter.GetBytes(val))
			};
			return result;
		}

		private static StorageRecord WriteRecord(double val)
		{
			var result = new StorageRecord()
			{
				PType = 7,
				Payload = ConvertForBigEndian(BitConverter.GetBytes(val))
			};
			return result;
		}

		private static StorageRecord WriteRecord(string val)
		{
			var result = new StorageRecord()
			{
				PType = 8,
				Payload = Encoding.UTF8.GetBytes(val)
			};
			return result;
		}

		private static byte[] ReadByteRecord(StorageRecord input)
		{
			return input.Payload;
		}

		private static bool ReadBoolRecord(StorageRecord input)
		{
			return input.Payload[0] != 0 ? true : false;
		}

		private static int ReadIntRecord(StorageRecord input)
		{
			return BitConverter.ToInt32(ConvertForBigEndian(input.Payload),0);
		}

		private static uint ReadUIntRecord(StorageRecord input)
		{
			return BitConverter.ToUInt32(ConvertForBigEndian(input.Payload), 0);
		}

		private static long ReadLongRecord(StorageRecord input)
		{
			return BitConverter.ToInt64(ConvertForBigEndian(input.Payload), 0);
		}

		private static ulong ReadULongRecord(StorageRecord input)
		{
			return BitConverter.ToUInt64(ConvertForBigEndian(input.Payload), 0);
		}

		private static float ReadFloatRecord(StorageRecord input)
		{
			return BitConverter.ToSingle(ConvertForBigEndian(input.Payload), 0);
		}

		private static double ReadDoubleRecord(StorageRecord input)
		{
			return BitConverter.ToDouble(ConvertForBigEndian(input.Payload), 0);
		}

		private static string ReadStringRecord(StorageRecord input)
		{
			return Encoding.UTF8.GetString(input.Payload);
		}

		//TODO: make proper restritions and conversions for supported T types.
		public void Set<T>(string key, T val)
		{
			StorageRecord result = null;
			if (typeof(T) == typeof(byte[]))
				result = WriteRecord((byte[])(object)val);
			else if (typeof(T) == typeof(bool))
				result = WriteRecord((bool)(object)val);
			else if (typeof(T) == typeof(int))
				result = WriteRecord((int)(object)val);
			else if (typeof(T) == typeof(uint))
				result = WriteRecord((uint)(object)val);
			else if (typeof(T) == typeof(long))
				result = WriteRecord((long)(object)val);
			else if (typeof(T) == typeof(ulong))
				result = WriteRecord((ulong)(object)val);
			else if (typeof(T) == typeof(float))
				result = WriteRecord((float)(object)val);
			else if (typeof(T) == typeof(double))
				result = WriteRecord((double)(object)val);
			else if (typeof(T) == typeof(string))
				result = WriteRecord((string)(object)val);
			else
				throw new NotSupportedException("Unsupported type!");
			lock (storage)
				storage[key.ToLower()] = result;
		}

		public T Get<T>(string key)
		{
			//TODO: add nullables support for primitives that do not support null value
			StorageRecord record;
			lock (storage)
				if (!storage.TryGetValue(key.ToLower(), out record))
					return default(T);
			if (record.PType == 0 && typeof(T) == typeof(byte[]))
				return (T)(object)ReadByteRecord(record);
			if (record.PType == 1 && typeof(T) == typeof(bool))
				return (T)(object)ReadBoolRecord(record);
			if (record.PType == 2 && typeof(T) == typeof(int))
				return (T)(object)ReadIntRecord(record);
			if (record.PType == 3 && typeof(T) == typeof(uint))
				return (T)(object)ReadUIntRecord(record);
			if (record.PType == 4 && typeof(T) == typeof(long))
				return (T)(object)ReadLongRecord(record);
			if (record.PType == 5 && typeof(T) == typeof(ulong))
				return (T)(object)ReadULongRecord(record);
			if (record.PType == 6 && typeof(T) == typeof(float))
				return (T)(object)ReadFloatRecord(record);
			if (record.PType == 7 && typeof(T) == typeof(double))
				return (T)(object)ReadDoubleRecord(record);
			if (record.PType == 8 && typeof(T) == typeof(string))
				return (T)(object)ReadStringRecord(record);
			return default(T);
		}
	}
}
