// MockSerializationHelper.cs
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
using System.Collections.Generic;
using System.Linq;
using DarkCaster.Serialization;
using DarkCaster.Converters;

namespace Tests.Mocks
{
	/// <summary>
	/// Simpliest ISerializationHelper mock.
	/// May be used as ISerializationHelper in some tests.
	/// For now compatible only with MockConfig.
	/// </summary>
	public class MockSerializationHelper<CFG> : ISerializationHelper<CFG>
			where CFG : IEquatable<CFG>, ICloneable
	{
		private object dataLocker=new object();
		private List<CFG> dataStorage=new List<CFG>();
		
		private int ReadFromByteArray(byte[] data)
		{
			return unchecked(data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24);
		}
		
		private byte[] WriteToByteArray(int val)
		{
			var data=new byte[4];
			data[0]=(byte)( val & 0xff);
			data[1]=(byte)( val >> 8 & 0xff);
			data[2]=(byte)( val >> 16 & 0xff);
			data[3]=(byte)( val >> 24 );
			return data;
		}
		
		public byte[] Serialize(CFG target)
		{
			lock(dataLocker)
			{
				dataStorage.Add((CFG)target.Clone());
				return WriteToByteArray((dataStorage.Count-1));
			}
		}
		
		public string SerializeToString(CFG target)
		{
			lock(dataLocker)
			{
				dataStorage.Add((CFG)target.Clone());
				return Base85.Encode(WriteToByteArray((dataStorage.Count-1)));
			}
		}
		
		public int Serialize(CFG target, byte[] dest, int offset = 0)
		{
			throw new NotSupportedException("NOPE");
		}
		
		public CFG Deserialize(byte[] data, int offset = 0, int len = 0)
		{
			if( offset != 0 || len != 0 )
				throw new NotSupportedException("NOPE");
			lock(dataLocker)
			{
				var index=ReadFromByteArray(data);
				return (CFG)dataStorage[index].Clone();
			}
		}
		
		public CFG Deserialize(string data)
		{
			lock(dataLocker)
			{
				var index=ReadFromByteArray(Base85.Decode(data));
				return (CFG)dataStorage[index].Clone();
			}
		}
	}
}
