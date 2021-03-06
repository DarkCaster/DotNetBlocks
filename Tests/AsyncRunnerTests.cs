﻿// AsyncRunnerTests.cs
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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using DarkCaster.Async;
namespace Tests
{
	[TestFixture]
	public class AsyncRunnerTests
	{
		[Test]
		public void Init()
		{
			var runner = new AsyncRunner();
			Assert.DoesNotThrow(runner.Dispose);
		}

		public volatile int threadId = 0;

		public async Task<bool> ThreadTestAsync()
		{
			var result = Thread.CurrentThread.ManagedThreadId == threadId;
			await Task.Delay(1000);
			return result && Thread.CurrentThread.ManagedThreadId == threadId;
		}

		[Test]
		public void AddJob()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			bool result1 = false;
			bool result2 = false;
			var runner = new AsyncRunner();
			runner.AddTask(ThreadTestAsync, res => result1 = res);
			runner.AddTask(ThreadTestAsync, res => result2 = res);
			runner.RunPendingTasks();
			runner.Dispose();
			Assert.True(result1);
			Assert.True(result2);
		}

		public async Task ThreadActionTestAsync()
		{
			if (Thread.CurrentThread.ManagedThreadId != threadId)
				throw new Exception("Thread has been changed!");
			await Task.Delay(1000);
			if (Thread.CurrentThread.ManagedThreadId != threadId)
				throw new Exception("Thread has been changed!");
		}

		[Test]
		public void AddJob_Action()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			var runner = new AsyncRunner();
			runner.AddTask(ThreadActionTestAsync);
			runner.AddTask(ThreadActionTestAsync);
			Assert.DoesNotThrow(runner.RunPendingTasks);
		}

		public async Task Folded_AsyncRunner()
		{
			await Task.Delay(250);
			var runner = new AsyncRunner();
			runner.AddTask(ThreadTestAsync);
			runner.RunPendingTasks();
			runner.Dispose();
			await Task.Delay(250);
		}

		[Test]
		public void AddJob_FoldedAsyncRunner()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			bool result1 = false;
			var runner = new AsyncRunner();
			runner.AddTask(ThreadTestAsync, res => result1 = res);
			runner.AddTask(Folded_AsyncRunner);
			runner.RunPendingTasks();
			runner.Dispose();
			Assert.True(result1);
		}

		public async Task<bool> Folded_Job()
		{
			await Task.Delay(500);
			return await ThreadTestAsync();
		}

		[Test]
		public void AddJob_Folded()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			bool result1 = false;
			bool result2 = false;
			var runner = new AsyncRunner();
			runner.AddTask(ThreadTestAsync, res => result1 = res);
			runner.AddTask(Folded_Job, res => result2 = res);
			runner.RunPendingTasks();
			runner.Dispose();
			Assert.True(result1);
			Assert.True(result2);
		}

		public async Task<bool> ThreadTestAsync_Param(int value)
		{
			if (value < 0)
				throw new NotImplementedException("Expected");
			var result = Thread.CurrentThread.ManagedThreadId == threadId;
			await Task.Delay(1000);
			return result && Thread.CurrentThread.ManagedThreadId == threadId;
		}

		[Test]
		public void AddJob_WithParam()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			bool result1 = false;
			bool result2 = false;
			var runner = new AsyncRunner();
			runner.AddTask(() => ThreadTestAsync_Param(10), res => result1 = res);
			runner.AddTask(() => ThreadTestAsync_Param(10), res => result2 = res);
			runner.RunPendingTasks();
			Assert.True(result1);
			Assert.True(result2);
		}

		[Test]
		public void AddJob_Exception()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			bool result1 = false;
			bool result2 = false;
			bool result3 = false;
			bool result4 = false;
			var runner = new AsyncRunner();
			runner.AddTask(() => ThreadTestAsync_Param(10), res => result1 = res);
			runner.AddTask(() => ThreadTestAsync_Param(-1), res => result2 = res);
			runner.AddTask(() => ThreadTestAsync_Param(-2), res => result3 = res);
			runner.AddTask(() => ThreadTestAsync_Param(10), res => result4 = res);
			AggregateException rEx = null;
			try { runner.RunPendingTasks(); }
			catch (Exception ex)
			{
				Assert.True(ex is AggregateException);
				rEx = (AggregateException)ex;
			}
			runner.Dispose();
			Assert.NotNull(rEx);
			Assert.True(result1);
			Assert.False(result2);
			Assert.False(result3);
			Assert.True(result4);
		}

		public async Task ResucriveAddAsync(AsyncRunner runner)
		{
			runner.AddTask(ThreadActionTestAsync);
			await Task.Delay(1000);
		}

		public async Task ResucriveRunAsync(AsyncRunner runner)
		{
			runner.RunPendingTasks();
			await Task.Delay(1000);
		}

		[Test]
		public void RecursiveAddJob_Exception()
		{
			var runner = new AsyncRunner();
			runner.AddTask(() => ResucriveAddAsync(runner));
			runner.AddTask(() => ResucriveRunAsync(runner));
			AggregateException rEx = null;
			try { runner.RunPendingTasks(); }
			catch (Exception ex)
			{
				Assert.True(ex is AggregateException);
				rEx = (AggregateException)ex;
			}
			runner.Dispose();
			Assert.NotNull(rEx);
			Assert.NotNull(rEx.InnerException);
			Assert.AreEqual(2,rEx.InnerExceptions.Count);
			foreach(var ex in rEx.InnerExceptions)
				Assert.True(ex is NotSupportedException);
		}

		[Test]
		public void Execute()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			var runner = new AsyncRunner();
			var result = runner.ExecuteTask(ThreadTestAsync);
			runner.Dispose();
			Assert.True(result);
		}

		[Test]
		public void Execute_Action()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			var runner = new AsyncRunner();
			runner.ExecuteTask(ThreadActionTestAsync);
			runner.Dispose();
		}

		[Test]
		public void Execute_Exception()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			var runner = new AsyncRunner();
			NotImplementedException rEx = null;
			try{ runner.ExecuteTask(() => ThreadTestAsync_Param(-1)); }
			catch(Exception ex)
			{
				Assert.True(ex is NotImplementedException);
				rEx = (NotImplementedException)ex;
			}
			Assert.NotNull(rEx);
			runner.Dispose();
		}

		[Test]
		public void RecursiveExecute_Exception()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			var runner = new AsyncRunner();
			var result = runner.ExecuteTask(ThreadTestAsync);
			runner.Dispose();
			Assert.True(result);
		}

		[Test]
		public void Execute_Exception_Reuse()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			var runner = new AsyncRunner();
			NotImplementedException rEx = null;
			try { runner.ExecuteTask(() => ThreadTestAsync_Param(-1)); }
			catch (Exception ex)
			{
				Assert.True(ex is NotImplementedException);
				rEx = (NotImplementedException)ex;
			}
			Assert.NotNull(rEx);
			var result = runner.ExecuteTask(ThreadTestAsync);
			runner.Dispose();
			Assert.True(result);
		}

		[Test]
		public void AddJob_Exception_Reuse()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			bool result1 = false;
			bool result2 = false;
			var runner = new AsyncRunner();
			runner.AddTask(() => ThreadTestAsync_Param(-1), res => result1 = res);
			runner.AddTask(() => ThreadTestAsync_Param(-2), res => result2 = res);
			AggregateException rEx = null;
			try { runner.RunPendingTasks(); }
			catch (Exception ex)
			{
				Assert.True(ex is AggregateException);
				rEx = (AggregateException)ex;
			}
			Assert.NotNull(rEx);
			Assert.True(rEx.InnerException is NotImplementedException);
			Assert.False(result1);
			Assert.False(result2);
			runner.AddTask(() => ThreadTestAsync_Param(10), res => result1 = res);
			runner.AddTask(() => ThreadTestAsync_Param(10), res => result2 = res);
			runner.RunPendingTasks();
			runner.Dispose();
			Assert.True(result1);
			Assert.True(result2);
		}
	}
}