using System;
using System.Text;
using DarkCaster.Serialization;
using NUnit.Framework;
using System.Threading;

namespace Tests
{
	public static class SerializationHelpersTests
	{
		public static TC GenericInterfaceBinary<TC,SER>(TC source,SER serializer)
			where SER : ISerializationHelper<TC>
		{
			var data = serializer.Serialize(source);
			return serializer.Deserialize(data);
		}
		
		public static TC GenericInterfaceText<TC,SER>(TC source,SER serializer)
			where SER : ISerializationHelper<TC>
		{
			var data = serializer.SerializeToString(source);
			return serializer.Deserialize(data);
		}

		public static TC GenericInterfaceBinaryRange<TC, SER>(TC source, SER serializer)
			where SER : ISerializationHelper<TC>
		{
			var random = new Random();
			var data = new byte[8192];
			var offset = random.Next(0, 4096);
			random.NextBytes(data);
			var len = serializer.Serialize(source, data, offset);
			return serializer.Deserialize(data, offset, len);
		}
		
		public static object InterfaceBinary<SER>(object source,SER serializer)
			where SER : ISerializationHelper
		{
			var data = serializer.SerializeObj(source);
			return serializer.DeserializeObj(data);
		}
		
		public static object InterfaceText<SER>(object source,SER serializer)
			where SER : ISerializationHelper
		{
			var data = serializer.SerializeObjToString(source);
			return serializer.DeserializeObj(data);
		}

		public static object InterfaceBinaryRange<SER>(object source, SER serializer)
			where SER : ISerializationHelper
		{
			var random = new Random();
			var data = new byte[8192];
			var offset = random.Next(0, 4096);
			random.NextBytes(data);
			var len = serializer.SerializeObj(source, data, offset);
			return serializer.DeserializeObj(data, offset, len);
		}
		
		public static void SerializersCompare<TC,SER1,SER2>(TC source, SER1 genSer, SER2 objSer)
			where SER1 : ISerializationHelper<TC>
			where SER2 : ISerializationHelper
		{
			var str1=genSer.SerializeToString(source);
			var str2=objSer.SerializeObjToString(source);
			Assert.AreEqual(str1,str2);
			var data1=genSer.Serialize(source);
			var data2=objSer.SerializeObj(source);
			Assert.AreEqual(data1,data2);
		}
		
		public static void BadDataDeserialize<TC,SER1,SER2>(Type expectedException, SER1 genSer, SER2 objSer)
			where SER1 : ISerializationHelper<TC>
			where SER2 : ISerializationHelper
		{
			Assert.True(expectedException.IsSubclassOf(typeof(SerializationException)));
			Assert.True((typeof(SerializationException)).IsAssignableFrom(expectedException));			
			var random=new Random();
			var data=new byte[random.Next(512,8192)];
			random.NextBytes(data);
			Assert.Throws(expectedException,()=>genSer.Deserialize(data));
			Assert.Throws(expectedException,()=>objSer.DeserializeObj(data));
		}
		
		public static void BadStringDeserialize<TC,SER1,SER2>(Type expectedException, SER1 genSer, SER2 objSer)
			where SER1 : ISerializationHelper<TC>
			where SER2 : ISerializationHelper
		{
			Assert.True(expectedException.IsSubclassOf(typeof(SerializationException)));
			Assert.True((typeof(SerializationException)).IsAssignableFrom(expectedException));	
			var random=new Random();
			var data=new byte[random.Next(512,8192)];
			random.NextBytes(data);
			var encoding = Encoding.GetEncoding("us-ascii", new EncoderReplacementFallback(" "), new DecoderReplacementFallback(" "));
			var badString=encoding.GetString(data);
			Assert.Throws(expectedException,()=>genSer.Deserialize(badString));
			Assert.Throws(expectedException,()=>objSer.DeserializeObj(badString));
		}
		
		public static void WrongTypeSerialize<SER>(Type expectedException, object wrongObject, SER serializer)
			where SER : ISerializationHelper
		{
			Assert.True(expectedException.IsSubclassOf(typeof(SerializationException)));
			Assert.True((typeof(SerializationException)).IsAssignableFrom(expectedException));	
			Assert.Throws(expectedException,()=>serializer.SerializeObj(wrongObject));
			Assert.Throws(expectedException,()=>serializer.SerializeObjToString(wrongObject));
		}
		
		//Some serializers (like json) actually CAN deserialize and apply data from different object type,
		//so this check may be skipped in such cases
		public static void WrongTypeDeserialize<SER>(Type expectedException, object obj, SER correctSerializer, SER wrongSerializer)
			where SER : ISerializationHelper
		{
			Assert.True(expectedException.IsSubclassOf(typeof(SerializationException)));
			Assert.True((typeof(SerializationException)).IsAssignableFrom(expectedException));	
			var bytes=correctSerializer.SerializeObj(obj);
			var str=correctSerializer.SerializeObjToString(obj);
			Assert.Throws(expectedException,()=>wrongSerializer.DeserializeObj(bytes));
			Assert.Throws(expectedException,()=>wrongSerializer.DeserializeObj(str));
		}

		[Serializable]
		public class SFTestClass
		{
			public int v = 0;
			public string str = null;
		}

		private class PSFTestClass
		{
			public int v = 0;
			public string str = null;
		}

		public static void SerializationFactoryTests(ISerializationHelperFactory factory)
		{
			var random = new Random();
			var test = new SFTestClass();
			test.v = random.Next();
			test.str = "test" + random.Next().ToString();
			var serializer = factory.GetHelper<SFTestClass>();
			var deserializer = factory.GetHelper<SFTestClass>();
			var data=serializer.Serialize(test);
			var restore = deserializer.Deserialize(data);
			Assert.AreEqual(test.v, restore.v);
			Assert.AreEqual(test.str, restore.str);
			var serializer2 = factory.GetHelper(typeof(SFTestClass));
			var deserializer2 = factory.GetHelper(typeof(SFTestClass));
			var data2 = serializer2.SerializeObj(test);
			Assert.AreEqual(data, data2);
			var restore2 = deserializer2.DeserializeObj(data);
			Assert.AreEqual(test.v, ((SFTestClass)restore2).v);
			Assert.AreEqual(test.str, ((SFTestClass)restore2).str);
		}

		//For now it works only with MsgPackSerializationHelperFactory,
		//because MsgPackSerializationHelper create all needed serialization stuff at it's constructor,
		//other SerializationHelpers for now create it's stuff dynamically, so its will only throw SerializationException on serialization
		public static void SerializationFactoryExceptionTests(ISerializationHelperFactory factory, Type expectedException)
		{
			Assert.True(expectedException.IsSubclassOf(typeof(SerializationFactoryException)));
			Assert.True((typeof(SerializationFactoryException)).IsAssignableFrom(expectedException));	
			var random = new Random();
			var test = new PSFTestClass();
			test.v = random.Next();
			test.str = "test" + random.Next().ToString();
			Assert.Throws(expectedException, () => factory.GetHelper<PSFTestClass>());
			Assert.Throws(expectedException, () => factory.GetHelper(typeof(PSFTestClass)));
		}
	}
	
	public static class SerializationHelpersThreadSafetyTests
	{
		public enum TestObjectState
		{
			Init=0,
			Started=1,
			Complete=2,
			Failed=-1
		}
		
		public interface ITestRunner<STO, ISH, ISHT>
			where ISH: ISerializationHelper
			where ISHT: ISerializationHelper<STO>
		{
			int OpCount { get; } /*must be volatile*/
			TestObjectState State { get; } /*must be volatile*/
			Exception Ex { get; } /*must be volatile*/
			void Start(); /*launch test loop, set state to started, when done*/
			void StartCounter(); /*reset counter*/
		}
		
		public sealed class TestRunner<STO, ISH, ISHT> : ITestRunner<STO, ISH, ISHT>
			where STO: class, IEquatable<STO> /* test object that will be serialized and deserilized */
			where ISH: ISerializationHelper
			where ISHT: ISerializationHelper<STO>
		{
			protected volatile bool cntEnabled=false;
			protected volatile int ops=0;
			protected volatile int opsLimit=1000;
			public int OpCount { get {return ops;} }
			
			protected volatile TestObjectState state=TestObjectState.Init;
			public TestObjectState State { get {return state;} }
			
			protected Exception ex;
			public Exception Ex { get {return ex;} }
			
			protected readonly ISH objSerializer;
			protected readonly ISHT genSerializer;
			protected readonly STO sampleObject;
			protected readonly byte[] sampleData;
			protected readonly string sampleString;
			
			public TestRunner(ISH nonGenericSerializer, ISHT genericSerializer, STO sampleObject, byte[] sampleData, string sampleString, int opsLimit)
			{
				throw new NotImplementedException("TODO");
				
				
			}
			
			public void Start()
			{
				throw new NotImplementedException("TODO");
			}
			
			public void Worker()
			{				
				state=TestObjectState.Started;
				
				while(ops<opsLimit)
				{
					
					
					if(cntEnabled)
						++ops;
				}
				
				state=TestObjectState.Complete;
			}
			
			public void StartCounter()
			{
				cntEnabled=true;
			}
			
			
		}
	}
}
