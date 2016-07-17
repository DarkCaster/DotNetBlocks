﻿using System;
using System.Diagnostics;
using DarkCaster.Serialization;

namespace PerfTests
{
	public static class BinarySerHelperSpeedTest
	{
		[Serializable]
		public class TestClass
		{
			public int x;
			public uint y;
			public long z;
			public ulong u;
			public float v;
			public double w;
			public TestClass()
			{
				x=1;
				y=2;
				z=3;
				u=4;
				v=5.0f;
				w=6.0;
			}
		}
		
		private static volatile TestClass tmp;
		private static volatile byte[] tmpBytes;
		
		public static void TestSerialize()
		{
			const int iter=1000000;
			var testObj=new TestClass();
			var serializer=new BinarySerializationHelper<TestClass>();
			
			var sw=new Stopwatch();
			var process=Process.GetCurrentProcess();
			
			var startCpuTime=process.TotalProcessorTime;
			sw.Start();
			for(int i=0;i<iter;++i)
				tmpBytes=serializer.Serialize(testObj);
			sw.Stop();
			var stopCpuTime=process.TotalProcessorTime;
			
			var cpuDiff=(stopCpuTime-startCpuTime).TotalSeconds;
			var diff=sw.Elapsed.TotalSeconds;
			var speed=iter/diff;
			
			Console.WriteLine(string.Format("BinarySH S: {0} obj in {1:##.##} S. Speed=={2:##.###} obj/S.",iter,diff,speed));
			Console.WriteLine(string.Format("BinarySH S: total processor time {0:##.###} S",cpuDiff));
		}
		
		public static void TestDeserialize()
		{
			const int iter=1000000;
			var serializer=new BinarySerializationHelper<TestClass>();
			
			var sw=new Stopwatch();
			var process=Process.GetCurrentProcess();
			
			var startCpuTime=process.TotalProcessorTime;
			sw.Start();
			for(int i=0;i<iter;++i)
				tmp=serializer.Deserialize(tmpBytes);
			sw.Stop();
			var stopCpuTime=process.TotalProcessorTime;
			
			var cpuDiff=(stopCpuTime-startCpuTime).TotalSeconds;
			var diff=sw.Elapsed.TotalSeconds;
			var speed=iter/diff;
			
			Console.WriteLine(string.Format("BinarySH D: {0} obj in {1:##.##} S. Speed=={2:##.###} obj/S.",iter,diff,speed));
			Console.WriteLine(string.Format("BinarySH D: total processor time {0:##.###} S",cpuDiff));
		}
	}
}
