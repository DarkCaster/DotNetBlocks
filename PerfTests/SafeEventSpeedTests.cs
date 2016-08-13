// SafeEventSpeedTests.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2016 DarkCaster <dark.caster@outlook.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Diagnostics;
using DarkCaster.Events;
using Tests.SafeEventStuff;

namespace PerfTests
{
	public static class SafeEventSpeedTests
	{
		private static void ISafeEvents_Subscribe(IPublisher pub, ISubscriber[] subs, string className)
		{
			var sw = new Stopwatch();
			var process = Process.GetCurrentProcess();

			var startCpuTime = process.TotalProcessorTime;
			sw.Start();
			for(int i = 0; i < subs.Length; ++i)
				pub.TheEvent.Subscribe(subs[i].OnEvent, false);
			sw.Stop();
			var stopCpuTime = process.TotalProcessorTime;

			var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
			var diff = sw.Elapsed.TotalSeconds;
			var speed = subs.Length / diff;

			Console.WriteLine(string.Format("{0} Subscribe: {1} delegates in {2:##.##} S. Speed=={3:##.###} subs/S.", className, subs.Length, diff, speed));
			Console.WriteLine(string.Format("{0} Subscribe: total processor time {1:##.###} S", className, cpuDiff));
		}

		private static void ISafeEvents_Unsubscribe(IPublisher pub, ISubscriber[] subs, string className)
		{
			var sw = new Stopwatch();
			var process = Process.GetCurrentProcess();

			var startCpuTime = process.TotalProcessorTime;
			sw.Start();
			for(int i = 0; i < subs.Length; ++i)
				pub.TheEvent.Unsubscribe(subs[i].OnEvent, false, true);
			sw.Stop();
			var stopCpuTime = process.TotalProcessorTime;

			var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
			var diff = sw.Elapsed.TotalSeconds;
			var speed = subs.Length / diff;

			Console.WriteLine(string.Format("{0} Unsubscribe: {1} delegates in {2:##.##} S. Speed=={3:##.###} subs/S.", className, subs.Length, diff, speed));
			Console.WriteLine(string.Format("{0} Unsubscribe: total processor time {1:##.###} S", className, cpuDiff));
		}

		public class SafeEventSpeedTempObject
		{
			public IPublisher pub;
			public ISubscriber[] subs;
		}

		public static SafeEventSpeedTempObject SafeEvent_Subscribe(int iter)
		{
			var ev = new SafeEvent<TestEventArgs>();
			var tmp = new SafeEventSpeedTempObject();
			tmp.pub = new SimplePublisher<SafeEvent<TestEventArgs>, SafeEvent<TestEventArgs>>(ev, ev);
			tmp.subs = new SimpleSubscriber[iter];
			for(int i = 0; i < iter; ++i)
				tmp.subs[i] = new SimpleSubscriber();
			ISafeEvents_Subscribe(tmp.pub, tmp.subs, "SafeEvent");
			return tmp;
		}

		public static void SafeEvent_Unsubscribe(SafeEventSpeedTempObject tmp)
		{
			ISafeEvents_Unsubscribe(tmp.pub,tmp.subs, "SafeEvent");
		}
		
		public static SafeEventSpeedTempObject SafeEvent_SubscribeRaise(int subCount)
		{
			var ev = new SafeEvent<TestEventArgs>();
			var tmp = new SafeEventSpeedTempObject();
			tmp.pub = new SimplePublisher<SafeEvent<TestEventArgs>, SafeEvent<TestEventArgs>>(ev, ev);
			tmp.subs = new SimpleSubscriber[subCount];
			for(int i = 0; i < subCount; ++i)
				tmp.subs[i] = new SimpleSubscriber();
			SubscribeRaise(tmp.pub,tmp.subs,"SafeEvent");
			return tmp;
		}
		
		private static void SubscribeRaise(IPublisher pub, ISubscriber[] subs, string className)
		{
			var sw = new Stopwatch();
			var process = Process.GetCurrentProcess();

			var startCpuTime = process.TotalProcessorTime;
			sw.Start();
			for(int i = 0; i < subs.Length; ++i)
			{
				pub.TheEvent.Subscribe(subs[i].OnEvent);
				pub.Raise();
			}
			sw.Stop();
			var stopCpuTime = process.TotalProcessorTime;

			var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
			var diff = sw.Elapsed.TotalSeconds;
			var speed = subs.Length / diff;

			Console.WriteLine(string.Format("{0} Seq Sub+Raise: {1} delegates in {2:##.##} S. Speed=={3:##.###} ops/S.", className, subs.Length, diff, speed));
			Console.WriteLine(string.Format("{0} Seq Sub+Raise: total processor time {1:##.###} S", className, cpuDiff));
		}
		
		public static void SafeEvent_Raise(SafeEventSpeedTempObject subs, int iter)
		{
			Raise(subs.pub,subs.subs,iter,"SafeEvent");
		}
		
		private static void Raise(IPublisher pub, ISubscriber[] subs, int iter, string className)
		{
			var sw = new Stopwatch();
			var process = Process.GetCurrentProcess();

			var startCpuTime = process.TotalProcessorTime;
			sw.Start();
			for(int i = 0; i < iter; ++i)
				pub.Raise();
			sw.Stop();
			var stopCpuTime = process.TotalProcessorTime;

			var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
			var diff = sw.Elapsed.TotalSeconds;
			var speed = iter / diff;

			Console.WriteLine(string.Format("{0} Raise {1} delegates: {2} calls in {3:##.##} S. Speed=={4:##.###} calls/S.", className, subs.Length, iter, diff, speed));
			Console.WriteLine(string.Format("{0} Raise: total processor time {1:##.###} S", className, cpuDiff));
		}
		
		public static void SafeEvent_UnsubscribeRaise(SafeEventSpeedTempObject subs)
		{
			UnsubscribeRaise(subs.pub,subs.subs,"SafeEvent");
		}
		
		private static void UnsubscribeRaise(IPublisher pub, ISubscriber[] subs, string className)
		{
			var sw = new Stopwatch();
			var process = Process.GetCurrentProcess();

			var startCpuTime = process.TotalProcessorTime;
			sw.Start();
			for(int i = 0; i < subs.Length; ++i)
			{
				pub.TheEvent.Unsubscribe(subs[i].OnEvent);
				pub.Raise();
			}
			sw.Stop();
			var stopCpuTime = process.TotalProcessorTime;

			var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
			var diff = sw.Elapsed.TotalSeconds;
			var speed = subs.Length / diff;

			Console.WriteLine(string.Format("{0} Seq UnSub+Raise: {1} delegates in {2:##.##} S. Speed=={3:##.###} ops/S.", className, subs.Length, diff, speed));
			Console.WriteLine(string.Format("{0} Seq UnSub+Raise: total processor time {1:##.###} S", className, cpuDiff));
		}
	}
}
