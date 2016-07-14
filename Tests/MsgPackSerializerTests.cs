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
			_CorrectSerialization(false);
			_CorrectSerialization(true);
		}
		
		private void _CorrectSerialization(bool storage)
		{
			int ctrlVal = (new Random()).Next();
			var test = new TestClass(ctrlVal);
			var ser=new MsgPackSerializationHelper<TestClass>(storage);
			var anotherSer=new MsgPackSerializationHelper<TestClass>(storage);
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
			_BadDataDeserialization(false);
			_BadDataDeserialization(true);
		}
		
		private void _BadDataDeserialization(bool storage)
		{
			var ser=new MsgPackSerializationHelper<TestClass>(storage);
			SerializationHelpersTests.BadDataDeserialize<TestClass,MsgPackSerializationHelper<TestClass>,MsgPackSerializationHelper<TestClass>>
				(typeof(MsgPackDeserializationException),ser,ser);
		}
		
		[Test]
		public void BadStringDeserialization()
		{
			_BadStringDeserialization(false);
			_BadStringDeserialization(true);
		}
		
		private void _BadStringDeserialization(bool storage)
		{
			var ser=new MsgPackSerializationHelper<TestClass>(storage);
			SerializationHelpersTests.BadStringDeserialize<TestClass,MsgPackSerializationHelper<TestClass>,MsgPackSerializationHelper<TestClass>>
				(typeof(MsgPackDeserializationException),ser,ser);
		}
		
		[Test]
		public void IncorrectSerialization()
		{
			_IncorrectSerialization(false);
			_IncorrectSerialization(true);
		}
		
		private void _IncorrectSerialization(bool storage)
		{
			int ctrlVal = (new Random()).Next();
			var objSer=new MsgPackSerializationHelper<TestClass>(storage);
			var testWrong = new TestClassWrong(ctrlVal,ctrlVal+1,ctrlVal+3);
			SerializationHelpersTests.WrongTypeSerialize(typeof(MsgPackSerializationException),testWrong,objSer);
		}
	}
}
