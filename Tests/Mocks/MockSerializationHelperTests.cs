// MockSerializationHelperTests.cs
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
using DarkCaster.Serialization;
using NUnit.Framework;

namespace Tests.Mocks
{
	/// <summary>
	/// Some basic tests for mock.
	/// </summary>
	[TestFixture]
	public class MockSerializationHelperTests
	{
		[Test]
		public void CorrectSerialization()
		{
			var test = new MockConfig();
			test.Randomize();
			
			var ser=new MockSerializationHelper<MockConfig>();
			var anotherSer=new MockSerializationHelper<MockConfig>();
			
			var check1=SerializationHelpersTests.GenericInterfaceBinary(test,ser);
			Assert.AreEqual(test.randomInt,check1.randomInt);
			Assert.AreEqual(test.randomString,check1.randomString);
			
			var check2=SerializationHelpersTests.GenericInterfaceText(test,ser);
			Assert.AreEqual(test.randomInt,check2.randomInt);
			Assert.AreEqual(test.randomString,check2.randomString);
		}
	}
}
