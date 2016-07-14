using System;
using NUnit.Framework;
using DarkCaster.Serialization;

namespace Tests
{
	[TestFixture]
	public class MsgPackSerializerTests
	{
		[Serializable]
		public class TestClass
		{
			private int v = 0;

			public TestClass(int v) { this.v = v; }

			public int V { get { return v; } }
		}

		[Serializable]
		public class TestClassWrong
		{
			private readonly int u = 100;
			private readonly int v = 0;
			private int x = 0;

			public TestClassWrong(int v, int x, int z) { this.v = v; this.x = u; this.x = z; this.x = x; }

			public int V { get { return v; } }
			public int X { get { return x; } }
		}

		[Test]
		public void CorrectSerialization()
		{
			int ctrlVal = (new Random()).Next();
			var test = new TestClass(ctrlVal);
			var ser=new MsgPackSerializationHelper<TestClass>();
			var anotherSer=new MsgPackSerializationHelper<TestClass>();
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
			var ser=new MsgPackSerializationHelper<TestClass>();
			SerializationHelpersTests.BadDataDeserialize<TestClass,MsgPackSerializationHelper<TestClass>,MsgPackSerializationHelper<TestClass>>
				(typeof(MsgPackDeserializationException),ser,ser);
		}
		
		[Test]
		public void IncorrectSerialization()
		{
			int ctrlVal = (new Random()).Next();
			var objSer=new MsgPackSerializationHelper<TestClass>();
			var testWrong = new TestClassWrong(ctrlVal,ctrlVal+1,ctrlVal+3);
			SerializationHelpersTests.WrongTypeSerialize(typeof(MsgPackSerializationException),testWrong,objSer);
		}
	}
}
