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
		public void DirectSerialization()
		{
			int ctrlVal = (new Random()).Next();
			var test = new TestClass(ctrlVal);
			var serializer = new BinarySerializationHelper<TestClass>();
			var data = serializer.Serialize(test);
			var data2 = serializer.SerializeObj(test);
			Assert.AreEqual(data, data2);
			var test2 = serializer.Deserialize(data);
			var test3 = (TestClass)serializer.DeserializeObj(data);
			Assert.AreEqual(test.V, test2.V);
			Assert.AreEqual(test2.V, test3.V);
		}

		[Test]
		public void InterfaceSerialization()
		{
			int ctrlVal = (new Random()).Next();
			var test = new TestClass(ctrlVal);
			var serializer = (ISerializationHelper<TestClass>)(new BinarySerializationHelper<TestClass>());
			var serializer2 = (ISerializationHelper)(new BinarySerializationHelper<TestClass>());
			var data = serializer.Serialize(test);
			var data2 = serializer2.SerializeObj(test);
			Assert.AreEqual(data, data2);
			var test2 = serializer.Deserialize(data);
			var test3 = (TestClass)serializer2.DeserializeObj(data);
			Assert.AreEqual(test.V, test2.V);
			Assert.AreEqual(test2.V, test3.V);
		}

		[Test]
		public void Interface2InterfaceSerialization()
		{
			int ctrlVal = (new Random()).Next();
			var test = (ITestClass)(new TestClass(ctrlVal));
			var serializer = (ISerializationHelper<TestClass>)(new BinarySerializationHelper<TestClass>());
			var serializer2 = (ISerializationHelper)(new BinarySerializationHelper<TestClass>());
			var data = serializer.Serialize((TestClass)test);
			var data2 = serializer2.SerializeObj(test);
			Assert.AreEqual(data, data2);
			var test2 = serializer.Deserialize(data);
			var test3 = (ITestClass)serializer2.DeserializeObj(data);
			Assert.AreEqual(test.V, test2.V);
			Assert.AreEqual(test2.V, test3.V);
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
