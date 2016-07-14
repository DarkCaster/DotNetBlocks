using System;
using DarkCaster.Hash;

namespace PerfTests
{
	public static class MMHashSpeedTest
	{
		private static volatile uint hash;
		
		public static void Test()
		{
			const int iter=2000;
			var random=new Random();
			var seed=unchecked((uint)random.Next(int.MinValue,int.MaxValue));
			var data=new byte[1024*1024+3];
			random.NextBytes(data);
			var mmHash=new MMHash32(seed);
			
			var sTime=DateTime.UtcNow;
			for(int i=0;i<iter;++i)
				hash=mmHash.GetHash(data);
			var diff=(DateTime.UtcNow-sTime).TotalSeconds;
			var totalSize=data.Length*iter;
			var speed=(totalSize/(1024*1024))/diff;
			Console.WriteLine(string.Format("MMHash32.GetHash: {0} B in {1:##.##} S. Speed=={2:##.###} Mb/S.",totalSize,diff,speed));
		}
	}
}
