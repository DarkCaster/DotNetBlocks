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
			_CorrectSerialization(MsgPackMode.Storage);
			_CorrectSerialization(MsgPackMode.StorageCheckSum);
			_CorrectSerialization(MsgPackMode.Transfer);
			_CorrectSerialization(MsgPackMode.TransferCheckSum);
		}
		
		private void _CorrectSerialization(MsgPackMode storage)
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
			var check5 = SerializationHelpersTests.GenericInterfaceBinaryRange(test, ser);
			Assert.AreEqual(test.V, check5.V);
			var check6 = SerializationHelpersTests.InterfaceBinaryRange(test, ser);
			Assert.AreEqual(test.V, ((TestClass)check6).V);
		}
		
		[Test]
		public void BadDataDeserialization()
		{
			_BadDataDeserialization(MsgPackMode.StorageCheckSum);
			_BadDataDeserialization(MsgPackMode.TransferCheckSum);
		}
		
		private void _BadDataDeserialization(MsgPackMode storage)
		{
			var ser=new MsgPackSerializationHelper<TestClass>(storage);
			SerializationHelpersTests.BadDataDeserialize<TestClass,MsgPackSerializationHelper<TestClass>,MsgPackSerializationHelper<TestClass>>
				(typeof(MsgPackDeserializationException),ser,ser);
		}
		
		[Test]
		public void BadStringDeserialization()
		{
			_BadStringDeserialization(MsgPackMode.Storage);
			_BadStringDeserialization(MsgPackMode.StorageCheckSum);
			_BadStringDeserialization(MsgPackMode.Transfer);
			_BadStringDeserialization(MsgPackMode.TransferCheckSum);
		}
		
		private void _BadStringDeserialization(MsgPackMode storage)
		{
			var ser=new MsgPackSerializationHelper<TestClass>(storage);
			SerializationHelpersTests.BadStringDeserialize<TestClass,MsgPackSerializationHelper<TestClass>,MsgPackSerializationHelper<TestClass>>
				(typeof(MsgPackDeserializationException),ser,ser);
		}
		
		[Test]
		public void IncorrectSerialization()
		{
			_IncorrectSerialization(MsgPackMode.Storage);
			_IncorrectSerialization(MsgPackMode.StorageCheckSum);
			_IncorrectSerialization(MsgPackMode.Transfer);
			_IncorrectSerialization(MsgPackMode.TransferCheckSum);
		}
		
		private void _IncorrectSerialization(MsgPackMode storage)
		{
			int ctrlVal = (new Random()).Next();
			var objSer=new MsgPackSerializationHelper<TestClass>(storage);
			var testWrong = new TestClassWrong(ctrlVal,ctrlVal+1,ctrlVal+3);
			SerializationHelpersTests.WrongTypeSerialize(typeof(MsgPackSerializationException),testWrong,objSer);
		}

		[Test]
		public void FactoryTest()
		{
			SerializationHelpersTests.SerializationFactoryTests(new MsgPackSerializationHelperFactory(MsgPackMode.Storage));
			SerializationHelpersTests.SerializationFactoryTests(new MsgPackSerializationHelperFactory(MsgPackMode.StorageCheckSum));
			SerializationHelpersTests.SerializationFactoryTests(new MsgPackSerializationHelperFactory(MsgPackMode.Transfer));
			SerializationHelpersTests.SerializationFactoryTests(new MsgPackSerializationHelperFactory(MsgPackMode.TransferCheckSum));
		}
	}
}
