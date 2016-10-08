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
		private enum RunnerState
		{
			Init=0,
			Started=1,
			Complete=2,
			Failed=-1
		}
		
		private interface ITestRunner<STO, ISH, ISHT>
			where ISH: ISerializationHelper
			where ISHT: ISerializationHelper<STO>
		{
			RunnerState State { get; } /*must be volatile*/
			Exception Ex { get; } /*must be volatile*/
			void Start(); /*launch test loop, set state to started, when done*/
			void StartCounter(); /*reset counter*/
		}
		
		private sealed class TestRunner<STO, ISH, ISHT> : ITestRunner<STO, ISH, ISHT>
			where STO: class, IEquatable<STO> /* test object that will be serialized and deserilized */
			where ISH: ISerializationHelper
			where ISHT: ISerializationHelper<STO>
		{
			private volatile bool cntEnabled=false;
			private int ops=0;
			private readonly int opsLimit=1000;
			
			private volatile RunnerState state=RunnerState.Init;
			public RunnerState State { get {return state;} }
			
			private Exception ex;
			public Exception Ex { get {return ex;} }
			
			private readonly ISH objSerializer;
			private readonly ISHT genSerializer;
			
			private readonly STO sampleObject;
			private readonly byte[] sampleData;
			private readonly string sampleString;
			
			private readonly Thread worker;
			
			public TestRunner(ISH nonGenericSerializer, ISHT genericSerializer, STO sampleObject, byte[] sampleData, string sampleString, int opsLimit)
			{
				objSerializer=nonGenericSerializer;
				genSerializer=genericSerializer;
				this.sampleObject=sampleObject;
				this.sampleData=sampleData;
				this.sampleString=sampleString;
				this.opsLimit=opsLimit;
				worker=new Thread(Worker);
			}
			
			public void Start()
			{
				worker.Priority = ThreadPriority.Normal;
				worker.Start();
			}
			
			private void AssertArraysEqual(byte[] data)
			{
				if(data.Length != sampleData.Length)
					throw new Exception("data.Length != sampleData.Length");
				for(int i=0; i<data.Length; ++i)
					if(data[i] != sampleData[i])
				throw new Exception("data[i] != sampleData[i]");
			}
			
			private void AssertStringsEqual(string str)
			{
				if(str != sampleString)
					throw new Exception("str != sampleString");
			}
			
			private void AssertObjEqial(object obj)
			{
				if(!sampleObject.Equals((STO)obj))
					throw new Exception("!sampleObject.Equals((STO)obj)");
			}
			
			public void Worker()
			{				
				state=RunnerState.Started;
				try
				{
					while(ops<opsLimit)
					{
						//perform generic serialize
						AssertArraysEqual(genSerializer.Serialize(sampleObject));
						//perform non generic serialize
						AssertArraysEqual(objSerializer.SerializeObj(sampleObject));
						//perform generic serialize to string
						AssertStringsEqual(genSerializer.SerializeToString(sampleObject));
						//perform non generic serialize to string
						AssertStringsEqual(objSerializer.SerializeObjToString(sampleObject));
						//perform generic deserialize
						AssertObjEqial(genSerializer.Deserialize(sampleData));
						//perform non generic deserialize
						AssertObjEqial(objSerializer.DeserializeObj(sampleData));
						//perform generic deserialize from string
						AssertObjEqial(genSerializer.Deserialize(sampleString));
						//perform non generic deserialize from string
						AssertObjEqial(objSerializer.DeserializeObj(sampleString));
						if(cntEnabled)
							++ops;
					}
				}
				catch(Exception ex)
				{
					this.ex=ex;
					state=RunnerState.Failed;
					return;
				}
				state=RunnerState.Complete;
			}
			
			public void StartCounter()
			{
				cntEnabled=true;
			}
		}
		
		public static void ThreadSafetyTest<STO, ISH, ISHT>(STO[] sampleObjects, ISH objSerializer, ISHT genSerializer, int opsLimit)
			where STO: class, IEquatable<STO>
			where ISH: ISerializationHelper
			where ISHT: ISerializationHelper<STO>
		{
			var runnerCount=sampleObjects.Length;
			//create runners
			var runners=new TestRunner<STO,ISH,ISHT>[runnerCount];
			for(int i=0;i<runnerCount;++i)
			{
				runners[i]=new TestRunner<STO, ISH, ISHT>(objSerializer, genSerializer, sampleObjects[i],
				                                          genSerializer.Serialize(sampleObjects[i]),
				                                          genSerializer.SerializeToString(sampleObjects[i]),opsLimit);
				Assert.AreEqual(RunnerState.Init,runners[i].State);
			}
			//start runners
			for(int i=0;i<runnerCount;++i)
			{
				if(runners[i].State == RunnerState.Failed)
					throw runners[i].Ex;
				Assert.AreEqual(RunnerState.Init,runners[i].State);
				runners[i].Start();
			}
			//check runners is started
			for(int i=0;i<runnerCount;++i)
			{
				if(runners[i].State == RunnerState.Failed)
					throw runners[i].Ex;
				while(runners[i].State == RunnerState.Init)
					Thread.Sleep(1);
				Assert.AreEqual(RunnerState.Started,runners[i].State);
			}
			//enable counters
			for(int i=0;i<runnerCount;++i)
			{
				if(runners[i].State == RunnerState.Failed)
					throw runners[i].Ex;
				Assert.AreEqual(RunnerState.Started,runners[i].State);
				runners[i].StartCounter();
			}
			//wait for completion
			for(int i=0;i<runnerCount;++i)
			{
				while(runners[i].State == RunnerState.Started)
					Thread.Sleep(1);
				if(runners[i].State == RunnerState.Failed)
					throw runners[i].Ex;
				Assert.AreEqual(RunnerState.Complete,runners[i].State);
			}
		}
	}
}
