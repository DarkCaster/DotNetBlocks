using System;
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
			var genSer=(ISerializationHelper<TestClass>)(new BinarySerializationHelper<TestClass>());
			var objSer=(ISerializationHelper)(new BinarySerializationHelper<TestClass>());
			SerializationHelpersTests.SerializersCompare(test,genSer,objSer);
			var check1=SerializationHelpersTests.GenericInterfaceBinary(test,genSer);
			Assert.AreEqual(test.V,check1.V);
			var check2=SerializationHelpersTests.GenericInterfaceText(test,genSer);
			Assert.AreEqual(test.V,check2.V);
			var check3=SerializationHelpersTests.InterfaceBinary(test,objSer);
			Assert.AreEqual(test.V,((TestClass)check3).V);
			var check4=SerializationHelpersTests.InterfaceText(test,objSer);
			Assert.AreEqual(test.V,((TestClass)check4).V);
		}

		[Test]
		public void WrongTypeExceptions()
		{
			int ctrlVal = (new Random()).Next();
			var test = new TestClass(ctrlVal);
			var testWrong = new TestClassWrong(ctrlVal,ctrlVal+1);
			var serializer = (ISerializationHelper)(new BinarySerializationHelper<TestClass>());
			var serializerWrong = (ISerializationHelper)(new BinarySerializationHelper<TestClassWrong>());

			var data = serializer.SerializeObj(test);
			var dataWrong = serializerWrong.SerializeObj(testWrong);

			Assert.Throws(typeof(BinarySerializationException), () => serializer.SerializeObj(testWrong));
			Assert.Throws(typeof(BinarySerializationException), () => serializerWrong.SerializeObj(test));

			Assert.Throws(typeof(BinaryDeserializationException), () => serializer.DeserializeObj(dataWrong));
			Assert.Throws(typeof(BinaryDeserializationException), () => serializerWrong.DeserializeObj(data));
		}
	}
}
