using System;
using System.Diagnostics;
using DarkCaster.Events;

namespace PerfTests
{
	public static class SafeEventsSpeedTest
	{
		public class TestEventArgs : EventArgs
		{
			public int Val { get; set; }
		}

		public class TestSubscriber
		{
			public int counter;
			private readonly IEventPublisher<TestEventArgs> testEvent;

			private void OnTestEvent(object sender, TestEventArgs args)
			{
				counter = args.Val;
			}

			public TestSubscriber(IEventPublisher<TestEventArgs> testEvent)
			{
				counter = 0;
				this.testEvent = testEvent;
			}

			public void Subscribe()
			{
				testEvent.Subscribe(OnTestEvent);
			}

			public void Unsubscribe()
			{
				testEvent.Unsubscribe(OnTestEvent);
			}
		}

		public class TestSTPublisher
		{
			public SafeEventPublisher<TestEventArgs> testEvent = new SafeEventPublisher<TestEventArgs>();
			private int counter;

			public TestSTPublisher()
			{
				counter = 0;
			}

			public void Raise()
			{
				++counter;
				testEvent.Raise(this, new TestEventArgs() { Val = counter });
			}
		}

		public class TestDefaultPublisher : IEventPublisher<TestEventArgs>
		{
			private event EventHandler<TestEventArgs> testEvent;

			public void Subscribe(EventHandler<TestEventArgs> subscriber)
			{
				testEvent += subscriber;
			}

			public void Unsubscribe(EventHandler<TestEventArgs> subscriber)
			{
				testEvent -= subscriber;
			}

			private int counter;

			public TestDefaultPublisher()
			{
				counter = 0;
			}

			public void Raise()
			{
				++counter;
				testEvent(this, new TestEventArgs() { Val = counter });
			}
		}

		public static void SpeedTest()
		{
			const int iter = 50000000;
			var publisher = new TestSTPublisher();
			var listener = new TestSubscriber(publisher.testEvent);
			listener.Subscribe();

			var sw = new Stopwatch();
			var process = Process.GetCurrentProcess();

			var startCpuTime = process.TotalProcessorTime;
			sw.Start();
			for(int i = 0; i < iter; ++i)
				publisher.Raise();
			sw.Stop();
			var stopCpuTime = process.TotalProcessorTime;

			var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
			var diff = sw.Elapsed.TotalSeconds;
			var speed = iter / diff;

			Console.WriteLine(string.Format("SafeEvents : {0} events in {1:##.##} S. Speed=={2:##.###} ev/S.", listener.counter, diff, speed));
			Console.WriteLine(string.Format("SafeEvents : total processor time {0:##.###} S", cpuDiff));
		}

		public static void SpeedTest_DefaultEvents()
		{
			const int iter = 50000000;
			var publisher = new TestDefaultPublisher();
			var listener = new TestSubscriber(publisher);
			listener.Subscribe();

			var sw = new Stopwatch();
			var process = Process.GetCurrentProcess();

			var startCpuTime = process.TotalProcessorTime;
			sw.Start();
			for(int i = 0; i < iter; ++i)
				publisher.Raise();
			sw.Stop();
			var stopCpuTime = process.TotalProcessorTime;

			var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
			var diff = sw.Elapsed.TotalSeconds;
			var speed = iter / diff;

			Console.WriteLine(string.Format("Default events : {0} events in {1:##.##} S. Speed=={2:##.###} ev/S.", listener.counter, diff, speed));
			Console.WriteLine(string.Format("Default events : total processor time {0:##.###} S", cpuDiff));
		}
	}
}
