// MockConfig.cs
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
using System.Linq;

namespace Tests.Mocks
{
	/// <summary>
	/// Mock config class for use with MockSerializationHelper when testing config provider.
	/// </summary>
	[Serializable]
	public class MockConfig : IEquatable<MockConfig>, ICloneable
	{
		[NonSerialized]
		private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		[NonSerialized]
		public static readonly int intDefault=
			(new Random()).Next(int.MinValue, int.MaxValue);
		[NonSerialized]
		public static readonly string stringDefault=
			new string(Enumerable.Repeat(chars, (new Random()).Next(2,100)).Select(s => s[(new Random()).Next(s.Length)]).ToArray());
		
		public int randomInt;
		public string randomString;
		
		public bool Equals(MockConfig other)
		{
			return (randomInt == other.randomInt && randomString == other.randomString);
		}
			
		public object Clone()
		{
			var clone = new MockConfig();
			clone.randomInt = randomInt;
			clone.randomString = randomString;
			return clone;
		}
		public MockConfig()
		{
			randomInt=intDefault;
			randomString=stringDefault;
		}
		
		public void Randomize()
		{
			var random = new Random();
			
			//mitigate bad random init: on some platforms (windows .net 4.5) when performing "new Random()" calls
			//in short time intervals it produce same seed and leads us to identical random sequences.
			int oldValue=randomInt;
			while(oldValue==randomInt)
				randomInt = random.Next(int.MinValue, int.MaxValue);	
			string oldString=randomString;
			while(oldString==randomString)
				randomString=new string(Enumerable.Repeat(chars, random.Next(2,100)).Select(s => s[random.Next(s.Length)]).ToArray());
		}
	}
}
