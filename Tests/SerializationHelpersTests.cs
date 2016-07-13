using System;
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
	}
}
