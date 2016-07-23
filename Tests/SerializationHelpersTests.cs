using System;
using System.Text;
using DarkCaster.Serialization;
using NUnit.Framework;

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
			Assert.Throws(expectedException,()=>serializer.SerializeObj(wrongObject));
			Assert.Throws(expectedException,()=>serializer.SerializeObjToString(wrongObject));
		}
		
		//Some serializers (like json) actually CAN deserialize and apply data from different object type,
		//so this check may be skipped in such cases
		public static void WrongTypeDeserialize<SER>(Type expectedException, object obj, SER correctSerializer, SER wrongSerializer)
			where SER : ISerializationHelper
		{
			var bytes=correctSerializer.SerializeObj(obj);
			var str=correctSerializer.SerializeObjToString(obj);
			Assert.Throws(expectedException,()=>wrongSerializer.DeserializeObj(bytes));
			Assert.Throws(expectedException,()=>wrongSerializer.DeserializeObj(str));
		}

		[Serializable]
		public class SFTestClass
		{
			public int v = 0;
		}

		public static void SerializationFactoryTests(ISerializationHelperFactory factory)
		{
			var random = new Random();
			var test = new SFTestClass();
			test.v = random.Next();
			var serializer = factory.GetHelper<SFTestClass>();
			var deserializer = factory.GetHelper<SFTestClass>();
			var data=serializer.Serialize(test);
			var restore = deserializer.Deserialize(data);
			Assert.AreEqual(test.v, restore.v);
			var serializer2 = factory.GetHelper(typeof(SFTestClass));
			var deserializer2 = factory.GetHelper(typeof(SFTestClass));
			var data2 = serializer2.SerializeObj(test);
			Assert.AreEqual(data, data2);
			var restore2 = deserializer2.DeserializeObj(data);
			Assert.AreEqual(test.v, ((SFTestClass)restore2).v);
		}
	}
}
