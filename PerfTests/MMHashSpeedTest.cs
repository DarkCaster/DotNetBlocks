using System;
using System.Diagnostics;
using DarkCaster.Hash;

namespace PerfTests
{
	public static class MMHashSpeedTest
	{
		#pragma warning disable 414
		private static volatile uint hash;
		#pragma warning restore 414

		public static void Test()
		{
			const int iter=2000;
			var random=new Random();
			var seed=unchecked((uint)random.Next(int.MinValue,int.MaxValue));
			var data=new byte[1024*1024+3];
			random.NextBytes(data);
			var mmHash=new MMHash32(seed);	
			var sw=new Stopwatch();
			var process=Process.GetCurrentProcess();	
			var startCpuTime=process.TotalProcessorTime;
			sw.Start();
			for(int i=0;i<iter;++i)
				hash=mmHash.GetHash(data);
			sw.Stop();
			var stopCpuTime=process.TotalProcessorTime;
			var cpuDiff=(stopCpuTime-startCpuTime).TotalSeconds;
			var diff=sw.Elapsed.TotalSeconds;
			var totalSize=data.Length*iter;
			var speed=(totalSize/(1024*1024))/diff;
			Console.WriteLine(string.Format("MMHash32.GetHash: {0} B in {1:##.##} S. Speed=={2:##.###} Mb/S.",totalSize,diff,speed));
			Console.WriteLine(string.Format("MMHash32.GetHash: total processor time {0:##.###} S",cpuDiff));
		}
	}
}
