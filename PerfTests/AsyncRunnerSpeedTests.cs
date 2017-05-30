// AsyncRunnerSpeedTests.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2017 DarkCaster <dark.caster@outlook.com>
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
using System.Threading.Tasks;
using System.Diagnostics;
using DarkCaster.Async;

namespace PerfTests
{
	public static class AsyncRunnerSpeedTests
	{
		private static volatile bool test_result = true;
#pragma warning disable 1998
		private static async Task<bool> TestAsync()
		{
			return test_result;
		}
#pragma warning restore 1998

		private static bool Test()
		{
			return test_result;
		}

		public static void DirectCalls()
		{
			const int iter = 1000000000;

			var sw = new Stopwatch();
			var process = Process.GetCurrentProcess();

			var startCpuTime = process.TotalProcessorTime;
			sw.Start();
			for (int i = 0; i < iter; ++i)
				Test();
			sw.Stop();
			var stopCpuTime = process.TotalProcessorTime;

			var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
			var diff = sw.Elapsed.TotalSeconds;
			var speed = iter / diff;

			Console.WriteLine(string.Format("DirectCalls: {0} method calls in {1:##.##} S. Speed=={2:##.###} calls/S.", iter, diff, speed));
			Console.WriteLine(string.Format("DirectCalls: total processor time {0:##.###} S", cpuDiff));
		}

		public static void AsyncRunnerCalls()
		{
			const int iter = 1000000;

			var sw = new Stopwatch();
			var process = Process.GetCurrentProcess();
			var runner = new AsyncRunner();
			var startCpuTime = process.TotalProcessorTime;
			sw.Start();
			for (int i = 0; i < iter; ++i)
				runner.ExecuteTask(TestAsync);
			sw.Stop();
			var stopCpuTime = process.TotalProcessorTime;

			var cpuDiff = (stopCpuTime - startCpuTime).TotalSeconds;
			var diff = sw.Elapsed.TotalSeconds;
			var speed = iter / diff;

			runner.Dispose();
			Console.WriteLine(string.Format("AsyncRunnerCalls: {0} method calls in {1:##.##} S. Speed=={2:##.###} calls/S.", iter, diff, speed));
			Console.WriteLine(string.Format("AsyncRunnerCalls: total processor time {0:##.###} S", cpuDiff));
		}
	}
}
