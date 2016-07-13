﻿using System;
using NUnit.Framework;
using DarkCaster.Serialization;

namespace Tests
{
	[TestFixture]
	public class BinarySerializerTests
	{
		public interface ITestClass
		{
			int V { get; }
		}

		[Serializable]
		public class TestClass : ITestClass
		{
			private int v = 0;

			public TestClass() { }
			public TestClass(int v) { this.v = v; }

			public int V { get { return v; } }
		}

		[Serializable]
		public class TestClassWrong : ITestClass
		{
			private int v = 0;
			private int x = 0;

			public TestClassWrong() { }
			public TestClassWrong(int v, int x) { this.v = v; this.x = x; }

			public int V { get { return v; } }
			public int X { get { return x; } }
		}
		
		[Test]
		public void CorrectSerialization()
		{
			int ctrlVal = (new Random()).Next();
			var test = new TestClass(ctrlVal);
			var ser=new BinarySerializationHelper<TestClass>();
			var anotherSer=new BinarySerializationHelper<TestClass>();
			SerializationHelpersTests.SerializersCompare(test,ser,anotherSer);
			var check1=SerializationHelpersTests.GenericInterfaceBinary(test,ser);
			Assert.AreEqual(test.V,check1.V);
			var check2=SerializationHelpersTests.GenericInterfaceText(test,ser);
			Assert.AreEqual(test.V,check2.V);
			var check3=SerializationHelpersTests.InterfaceBinary(test,ser);
			Assert.AreEqual(test.V,((TestClass)check3).V);
			var check4=SerializationHelpersTests.InterfaceText(test,ser);
			Assert.AreEqual(test.V,((TestClass)check4).V);
		}
		
		[Test]
		public void BadDataDeserialization()
		{
			var ser=new BinarySerializationHelper<TestClass>();
			SerializationHelpersTests.BadDataDeserialize<TestClass,BinarySerializationHelper<TestClass>,BinarySerializationHelper<TestClass>>
				(typeof(BinaryDeserializationException),ser,ser);
		}
		
		[Test]
		public void IncorrectSerialization()
		{
			int ctrlVal = (new Random()).Next();
			var objSer=new BinarySerializationHelper<TestClass>();
			var testWrong = new TestClassWrong(ctrlVal,ctrlVal+1);
			SerializationHelpersTests.WrongTypeSerialize(typeof(BinarySerializationException),testWrong,objSer);
		}
		
		[Test]
		public void IncorrectDeserialization()
		{
			int ctrlVal = (new Random()).Next();
			var objSer=(ISerializationHelper)(new BinarySerializationHelper<TestClass>());
			var wrongSer=(ISerializationHelper)(new BinarySerializationHelper<TestClassWrong>());
			var obj = new TestClass(ctrlVal);
			SerializationHelpersTests.WrongTypeDeserialize(typeof(BinaryDeserializationException),obj,objSer,wrongSer);
		}
	}
}