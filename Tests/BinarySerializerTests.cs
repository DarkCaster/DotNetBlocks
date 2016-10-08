using System;
using System.Linq;
using NUnit.Framework;
using DarkCaster.Serialization;
using DarkCaster.Serialization.Binary;

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
			var check5 = SerializationHelpersTests.GenericInterfaceBinaryRange(test, ser);
			Assert.AreEqual(test.V, check5.V);
			var check6 = SerializationHelpersTests.InterfaceBinaryRange(test, ser);
			Assert.AreEqual(test.V, ((TestClass)check6).V);
		}
		
		[Test]
		public void BadDataDeserialization()
		{
			var ser=new BinarySerializationHelper<TestClass>();
			SerializationHelpersTests.BadDataDeserialize<TestClass,BinarySerializationHelper<TestClass>,BinarySerializationHelper<TestClass>>
				(typeof(BinaryDeserializationException),ser,ser);
		}
		
		[Test]
		public void BadStringDeserialization()
		{
			var ser=new BinarySerializationHelper<TestClass>();
			SerializationHelpersTests.BadStringDeserialize<TestClass,BinarySerializationHelper<TestClass>,BinarySerializationHelper<TestClass>>
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

		[Test]
		public void FactoryTest()
		{
			SerializationHelpersTests.SerializationFactoryTests(new BinarySerializationHelperFactory());
		}
		
		[Serializable]
		public class ThreadSafetyTestObject : IEquatable<ThreadSafetyTestObject>
		{
			public int intValue=0;
			public long longValue=0;
			public string strValue="";
			
			public bool Equals(ThreadSafetyTestObject obj)
			{
				return( intValue==obj.intValue && longValue==obj.longValue && strValue==obj.strValue );
			}
		}
		
		[Test]
		public void ThreadSafetyTest()
		{
			const int runnersCounts=8;
			const int iterationsCount=10000;
			
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var random=new Random();
			
			//create different test objects
			var testObjects=new ThreadSafetyTestObject[runnersCounts];
			for(int i=0; i<runnersCounts; ++i)
			{
				testObjects[i]=new ThreadSafetyTestObject();
				testObjects[i].intValue=random.Next();
				testObjects[i].longValue=random.Next();
				testObjects[i].strValue=new string(Enumerable.Repeat(chars, random.Next(2,100)).Select(s => s[random.Next(s.Length)]).ToArray());
			}
			
			for(int i=1; i<runnersCounts; ++i)
				Assert.False(testObjects[i].Equals(testObjects[i-1]));
			
			//create serializers
			var genSerializer=(ISerializationHelper<ThreadSafetyTestObject>)(new BinarySerializationHelper<ThreadSafetyTestObject>());
			var objSerializer=(ISerializationHelper)genSerializer;
			SerializationHelpersThreadSafetyTests.ThreadSafetyTest(testObjects,objSerializer,genSerializer,iterationsCount);
		}
	}
}
