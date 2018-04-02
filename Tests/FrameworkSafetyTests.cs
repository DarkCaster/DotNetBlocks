using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace Tests
{
	/// <summary>
	/// This tests are basically intended for my own clarification of some specific .NET aspects and test of some non-typical cases.
	/// Some tests may be intended for verificaition of some specific bugs in mono\dotnet\dotnet-core\etc for specific platform (for ARM).
	/// </summary>
	[TestFixture]
	public class FrameworkSafetyTests
	{
		// https://gist.github.com/huysentruitw/f6f10cc1e9a10f2ef9bd5ab18f0b4f47
		private static class ConcurrentDictionaryTestCase
		{
			private static ConcurrentDictionary<string, string> dict = new ConcurrentDictionary<string, string>();
			private static int count = 0;
			public static void Execute()
			{
				var t1 = new Thread(() =>
				dict.GetOrAdd("key1", x => {
					Interlocked.Increment(ref count);
					Thread.Sleep(TimeSpan.FromSeconds(1));
					return x;
				}));
				var t2 = new Thread(() =>
				dict.GetOrAdd("key2", x => {
					Interlocked.Increment(ref count);
					return x;
				}));

				var t3 = new Thread(() =>
				dict.GetOrAdd("key1", x => {
					Interlocked.Increment(ref count);
					return x;
				}));

				t1.Start();
				Thread.Sleep(500);
				t2.Start();
				t3.Start();
				t1.Join();
				t2.Join();
				t3.Join();
				if (count > 2)
					Assert.Fail(count.ToString());
			}
		}
		
		private static ReaderWriterLockSlim failingLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		
		//should fail
		private async Task<int> RWLockTestAsync()
		{
			for (int i = 0; i < 10; ++i) {
				failingLock.EnterReadLock();
				await Task.Delay(100);
				failingLock.ExitReadLock();
			}
			return 0;
		}
		
		[Test()]
		public void RWLockTest()
		{
			try
			{
				var task = Task.Run(async () => await RWLockTestAsync());
				var i = task.Result;
			}
			catch (AggregateException ex)
			{
				Assert.IsInstanceOf(typeof(SynchronizationLockException), ex.InnerException);
				return;
			}
			catch (Exception ex) { throw ex; }
		}

		private class ThreadSpawner
		{
			private int counter;
			public ThreadSpawner()
			{
				Task.Run(()=>Worker());
				for (int i = 0; i < 1250000000; ++i)
					if (Interlocked.CompareExchange(ref counter, i + 1, i) != i)
						throw new Exception("Thread write value while constructor is running!");
			}
			public void Worker()
			{
					Interlocked.Exchange(ref counter,-1);
			}
		}

		[Test]
		public void ThreadFromConstructor()
		{
			Assert.Throws(typeof(Exception),() => new ThreadSpawner());
		}

		private volatile bool asyncThreadIdTest_Stop = false;
		private int asyncThreadIdTest_WorkersConut = 0;
		private async Task<bool> ThreadIDTestWorker()
		{
			Interlocked.Increment(ref asyncThreadIdTest_WorkersConut);
			var random = new Random();
			var threadId = Thread.CurrentThread.ManagedThreadId;
			var result = false;
			do
			{
				await Task.Delay(random.Next(0, 50));
				var newThreadId = Thread.CurrentThread.ManagedThreadId;
				if (newThreadId != threadId)
				{
					result = true;
					threadId = newThreadId;
				}
			} while (!asyncThreadIdTest_Stop);
			return result;
		}

		[Test]
		public void AsyncThreadIdTest()
		{
			asyncThreadIdTest_Stop = false;
			asyncThreadIdTest_WorkersConut = 0;
			const int workers = 500;
			Task<bool>[] tasks = new Task<bool>[workers];
			for (int i = 0; i < workers; ++i)
				tasks[i] = Task.Run(async () => await ThreadIDTestWorker());
			while(Interlocked.CompareExchange(ref asyncThreadIdTest_WorkersConut,0,0)<workers) {}
			Thread.Sleep(500);
			asyncThreadIdTest_Stop = true;
			var result = false;
			for (int i = 0; i < workers; ++i)
				result |= tasks[i].Result;
			Assert.True(result);
		}

		//more info about this test case:
		//https://stackoverflow.com/questions/27701812/anonymous-function-and-local-variables

		private static int TestWorker(int result)
		{
			return result;
		}

		[Test]
		public void TaskRun_ParameterNotCaptured()
		{
			var cnt = 100;
			var tasks = new Task<int>[cnt];
			for (int i = 0; i < cnt; ++i)
				tasks[i] = Task.Run(() => TestWorker(i));
			var testPassed = false;
			for (int i = 0; i < cnt; ++i)
				//initial variable was not captured, result may not match initial value
				if (tasks[i].Result != i)
					testPassed = true;
			if (!testPassed)
				Assert.Fail("Coult not confirm that parameter sometimes is not captured");
		}

		[Test]
		public void TaskRun_ParameterCaptured()
		{
			var cnt = 100;
			var tasks = new Task<int>[cnt];
			for (int i = 0; i < cnt; ++i)
			{
				var j = i;
				tasks[i] = Task.Run(() => TestWorker(j));
			}
			for (int i = 0; i < cnt; ++i)
				//initial variable was captured, result will match initial value
				if (tasks[i].Result != i)
					throw new Exception(string.Format("tasks[{0}].Result == {1}", i, tasks[i].Result));
		}

		private static volatile bool start = false;
		private static readonly object sourceDataLock = new object();
		private static byte[] sourceData = null;

		private static int readWorkersActive = 0;
		private static int writeWorkersActive = 0;

		private static void Reset()
		{
			Interlocked.Exchange(ref readWorkersActive, 0);
			Interlocked.Exchange(ref writeWorkersActive, 0);
			start = false;
		}

		private static void Start()
		{
			start = true;
		}

		private static async Task ReadWorker(int iterations)
		{
			byte[] controlData = null;
			lock (sourceDataLock)
			{
				controlData = new byte[sourceData.Length];
				Buffer.BlockCopy(sourceData, 0, controlData, 0, sourceData.Length);
			}
			Interlocked.Increment(ref readWorkersActive);
			while (!start)
				await Task.Delay(10);

			var testData = new byte[controlData.Length];
			var random = new Random();
			var addr = IPAddress.Parse("127.0.0.1");//Dns.GetHostEntry("127.0.0.1").AddressList[0];
			var client = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			await Task.Factory.FromAsync(
				(callback, state) => client.BeginConnect(new IPEndPoint(addr, 49999), callback, state),
				client.EndConnect, null).ConfigureAwait(false);
			//apply some optional settings to socket
			client.NoDelay = false;
			client.ReceiveBufferSize = 262144;
			client.SendBufferSize = 262144;

			for (int iter = 0; iter < iterations; ++iter)
			{
				random.NextBytes(testData);
				int pos = 0;
				while (pos < testData.Length)
					pos += await Task.Factory.FromAsync(
						(callback, state) => client.BeginReceive(testData, pos, testData.Length - pos, SocketFlags.None, callback, state),
						client.EndReceive, null).ConfigureAwait(false);
				for (int i = 0; i < testData.Length; ++i)
					if (testData[i] != controlData[i])
						throw new Exception($"Data verification failed, iteration {iter}");
			}
		}

		private static async Task WriteWorker(int iterations)
		{
			byte[] data = null;
			lock (sourceDataLock)
			{
				data = new byte[sourceData.Length];
				Buffer.BlockCopy(sourceData, 0, data, 0, sourceData.Length);
			}
			IPAddress addr = IPAddress.Any;
			var ep = new IPEndPoint(addr, 49999);
			var socket = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(ep);
			socket.Listen(10);
			Interlocked.Increment(ref writeWorkersActive);
			while (!start)
				await Task.Delay(10);
			var tSocket = await Task.Factory.FromAsync(socket.BeginAccept, socket.EndAccept, null).ConfigureAwait(false);
			tSocket.NoDelay = false;
			tSocket.ReceiveBufferSize = 262144;
			tSocket.SendBufferSize = 262144;
			for (int iter = 0; iter < iterations; ++iter)
			{
				int pos = 0;
				while (pos < data.Length)
					pos += await Task.Factory.FromAsync(
					(callback, state) => tSocket.BeginSend(data, pos, data.Length - pos, SocketFlags.None, callback, state),
					tSocket.EndSend, null).ConfigureAwait(false);
			}
		}

		//Failing on Mono 5.2.0 Stable and 5.4.0 Stable
		//https://bugzilla.xamarin.com/show_bug.cgi?id=57918
		[Test]
		public void FromAsync_Socket_SendReceive()
		{
			//test params
			const int iterations = 1000;
			const int dataBlockSize = 1024 * 1024;
			Reset();
			//generate source data
			lock (sourceDataLock)
			{
				sourceData = new byte[dataBlockSize];
				new Random().NextBytes(sourceData);
			}
			Task clReaders = Task.Run(() => ReadWorker(iterations));
			Task clWriters = Task.Run(() => WriteWorker(iterations));
			while (Interlocked.CompareExchange(ref readWorkersActive, 0, 0) < 1) { Thread.Sleep(10); }
			while (Interlocked.CompareExchange(ref writeWorkersActive, 0, 0) < 1) { Thread.Sleep(10); }
			Start();
			clReaders.Wait();
			clWriters.Wait();
		}

		private const int TEST1 = 31;
		private const int TEST2 = 8191;
		private const int TEST3 = 2097151;
		private const int TEST4 = 536870911;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CalculateHeaderLength(int payloadLen)
		{
			if(payloadLen <= TEST1)
				return 1;
			if(payloadLen <= TEST2)
				return 2;
			if(payloadLen <= TEST3)
				return 3;
			if(payloadLen <= TEST4)
				return 4;
			throw new Exception(string.Format("Payload length > MAX_BLOCK_SZ: {0} > {1}", payloadLen, TEST4));
		}

		//Failing on Mono 5.2.0 and 5.4.0 with Release build + optimizations enabled
		//https://bugzilla.xamarin.com/show_bug.cgi?id=59608
		[Test]
		public void AggressiveInlining_Fail()
		{
			var testLen = (new Random()).Next(8192, 2097150);
			var result = CalculateHeaderLength(testLen);
			if(result != 3)
				throw new Exception("Triggered!");
		}

		//Failing on Mono 5.10.0.140-5.10.1.25 because of using new corefx sources that is less compatible with .NET 4.5
		//More info:
		//https://github.com/mono/mono/issues/7822
		//https://github.com/dotnet/corefx/issues/28137
		//https://github.com/dotnet/corefx/pull/19742
		[Test]
		public void ConcurrentDictionary_BinarySerialization_Fail()
		{
			var target = new ConcurrentDictionary<int, int>();
			using (var stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, target);  //exception will be thrown here
			}
		}
	}
}
