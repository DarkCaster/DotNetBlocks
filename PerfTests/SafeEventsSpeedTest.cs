using System;
using System.Diagnostics;
using DarkCaster.Events;
//using PerfTests.External;

namespace PerfTests
{
	public static class SafeEventsSpeedTest
	{
		private class TestEventArgs : EventArgs
		{
			public int Val { get; set; }
		}

		private class TestSubscriber
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

		private interface ISpeedTestPublisher
		{
			IEventPublisher<TestEventArgs> GetEvent();
			void Raise();
		}

		private class TestSafeEventPublisher : ISpeedTestPublisher
		{
			private SafeEventPublisher<TestEventArgs> testEvent = new SafeEventPublisher<TestEventArgs>();
			private int counter;

			public IEventPublisher<TestEventArgs> GetEvent()
			{
				return testEvent;
			}

			public TestSafeEventPublisher()
			{
				counter = 0;
			}

			public void Raise()
			{
				++counter;
				testEvent.Raise(this, new TestEventArgs() { Val = counter });
			}
		}

		private class TestDefaultPublisher : ISpeedTestPublisher, IEventPublisher<TestEventArgs>
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

			public IEventPublisher<TestEventArgs> GetEvent()
			{
				return this;
			}

			private int counter;

			public TestDefaultPublisher()
			{
				counter = 0;
			}

			public void Raise()
			{
				++counter;
				var testEventCopy = testEvent;
				if(testEventCopy != null)
					testEventCopy(this, new TestEventArgs() { Val = counter });
			}
		}

		/*private class TestFastPublisher : ISpeedTestPublisher, IEventPublisher<TestEventArgs>
		{
			private FastSmartWeakEvent<EventHandler<TestEventArgs>> _testEvent = new FastSmartWeakEvent<EventHandler<TestEventArgs>>();
			private event EventHandler<TestEventArgs> testEvent
			{
				add { _testEvent.Add(value); }
				remove { _testEvent.Remove(value); }
			}

			public void Subscribe(EventHandler<TestEventArgs> subscriber)
			{
				testEvent += subscriber;
			}

			public void Unsubscribe(EventHandler<TestEventArgs> subscriber)
			{
				testEvent -= subscriber;
			}

			public IEventPublisher<TestEventArgs> GetEvent()
			{
				return this;
			}

			private int counter;

			public TestFastPublisher()
			{
				counter = 0;
			}

			public void Raise()
			{
				++counter;
				_testEvent.Raise(this, new TestEventArgs() { Val = counter });
			}
		}

		public static void SpeedTest_FastEvents()
		{
			SpeedTest("FastEvents", new TestFastPublisher());
		}*/

		public static void SpeedTest_SafeEvents()
		{
			SpeedTest("SafeEvents", new TestSafeEventPublisher());
		}

		public static void SpeedTest_DefaultEvents()
		{
			SpeedTest("DefaultEvents", new TestDefaultPublisher());
		}

		private static void SpeedTest( string name, ISpeedTestPublisher publisher )
		{
			const int iter = 50000000;
			var listener = new TestSubscriber(publisher.GetEvent());
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

			Console.WriteLine(string.Format("{0} : {1} events in {2:##.##} S. Speed=={3:##.###} ev/S.", name, listener.counter, diff, speed));
			Console.WriteLine(string.Format("{0} : total processor time {1:##.###} S", name, cpuDiff));
		}
	}
}
