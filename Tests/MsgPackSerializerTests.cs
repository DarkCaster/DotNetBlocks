using System;
using System.Linq;
using NUnit.Framework;
using DarkCaster.Serialization;
using DarkCaster.Serialization.MsgPack;

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

		[Test]
		public void FactoryExceptionTest()
		{
			SerializationHelpersTests.SerializationFactoryExceptionTests(new MsgPackSerializationHelperFactory(MsgPackMode.Storage),
			                                                             typeof(MsgPackSerializationFactoryException));
			SerializationHelpersTests.SerializationFactoryExceptionTests(new MsgPackSerializationHelperFactory(MsgPackMode.StorageCheckSum),
			                                                             typeof(MsgPackSerializationFactoryException));
			SerializationHelpersTests.SerializationFactoryExceptionTests(new MsgPackSerializationHelperFactory(MsgPackMode.Transfer),
			                                                             typeof(MsgPackSerializationFactoryException));
			SerializationHelpersTests.SerializationFactoryExceptionTests(new MsgPackSerializationHelperFactory(MsgPackMode.TransferCheckSum),
			                                                             typeof(MsgPackSerializationFactoryException));
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
			const int iterationsCount=20000;
			
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
			
			var genSerializer=(ISerializationHelper<ThreadSafetyTestObject>)(new MsgPackSerializationHelper<ThreadSafetyTestObject>(MsgPackMode.Storage));
			var objSerializer=(ISerializationHelper)genSerializer;
			SerializationHelpersThreadSafetyTests.ThreadSafetyTest(testObjects,objSerializer,genSerializer,iterationsCount);
			
			genSerializer=(ISerializationHelper<ThreadSafetyTestObject>)(new MsgPackSerializationHelper<ThreadSafetyTestObject>(MsgPackMode.StorageCheckSum));
			objSerializer=(ISerializationHelper)genSerializer;
			SerializationHelpersThreadSafetyTests.ThreadSafetyTest(testObjects,objSerializer,genSerializer,iterationsCount);
			
			genSerializer=(ISerializationHelper<ThreadSafetyTestObject>)(new MsgPackSerializationHelper<ThreadSafetyTestObject>(MsgPackMode.Transfer));
			objSerializer=(ISerializationHelper)genSerializer;
			SerializationHelpersThreadSafetyTests.ThreadSafetyTest(testObjects,objSerializer,genSerializer,iterationsCount);
			
			genSerializer=(ISerializationHelper<ThreadSafetyTestObject>)(new MsgPackSerializationHelper<ThreadSafetyTestObject>(MsgPackMode.TransferCheckSum));
			objSerializer=(ISerializationHelper)genSerializer;
			SerializationHelpersThreadSafetyTests.ThreadSafetyTest(testObjects,objSerializer,genSerializer,iterationsCount);
		}

		[Test]
		public void BinaryLargeObjectWithOffset()
		{
			SerializationHelpersTests.LargeObjectSerializaionTests(new MsgPackSerializationHelperFactory(MsgPackMode.Storage), 16384 * 1024, 8192);
			SerializationHelpersTests.LargeObjectSerializaionTests(new MsgPackSerializationHelperFactory(MsgPackMode.StorageCheckSum), 16384 * 1024, 8192);
			SerializationHelpersTests.LargeObjectSerializaionTests(new MsgPackSerializationHelperFactory(MsgPackMode.Transfer), 16384 * 1024, 8192);
			SerializationHelpersTests.LargeObjectSerializaionTests(new MsgPackSerializationHelperFactory(MsgPackMode.TransferCheckSum), 16384 * 1024, 8192);
		}
	}
}
