using System;
using NUnit.Framework;
using DarkCaster.Serialization.Json;

namespace Tests
{
	[TestFixture]
	public class JsonSerializerTests
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
			var ser=new JsonSerializationHelper<TestClass>();
			var anotherSer=new JsonSerializationHelper<TestClass>();
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
			var ser=new JsonSerializationHelper<TestClass>();
			SerializationHelpersTests.BadDataDeserialize<TestClass,JsonSerializationHelper<TestClass>,JsonSerializationHelper<TestClass>>
				(typeof(JsonDeserializationException),ser,ser);
		}
		
		[Test]
		public void BadStringDeserialization()
		{
			var ser=new JsonSerializationHelper<TestClass>();
			SerializationHelpersTests.BadStringDeserialize<TestClass,JsonSerializationHelper<TestClass>,JsonSerializationHelper<TestClass>>
				(typeof(JsonDeserializationException),ser,ser);
		}
		
		[Test]
		public void IncorrectSerialization()
		{
			int ctrlVal = (new Random()).Next();
			var objSer=new JsonSerializationHelper<TestClass>();
			var testWrong = new TestClassWrong(ctrlVal,ctrlVal+1,ctrlVal+2);
			SerializationHelpersTests.WrongTypeSerialize(typeof(JsonSerializationException),testWrong,objSer);
		}

		[Test]
		public void FactoryTest()
		{
			SerializationHelpersTests.SerializationFactoryTests(new JsonSerializationHelperFactory());
		}
	}
}
