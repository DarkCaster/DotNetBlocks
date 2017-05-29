// AsyncRunner.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2017 DarkCaster <dark.caster@outlook.com>
// Based on concepts of AsyncBridge project by Tom Jacques.
// See https://github.com/tejacques/AsyncBridge for more info.
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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DarkCaster.Async
{
	public class AsyncRunner : IDisposable
	{
		private class SingleThreadedContext : SynchronizationContext, IDisposable
		{
			private struct TaskChunk
			{
				private readonly SendOrPostCallback callback;
				private readonly object state;
				public TaskChunk(SendOrPostCallback callback, object state)
				{
					this.callback = callback;
					this.state = state;
				}
				public void Execute()
				{
					callback(state);
				}
			}

			private int taskCount;
			private bool done;
			private readonly ConcurrentQueue<TaskChunk> items;
			private readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
			private readonly List<Exception> innerExceptions = new List<Exception>();

			public int InnerExceptionsCount { get { return innerExceptions.Count; } }
			public IEnumerable<Exception> InnerExceptions { get { return innerExceptions.ToArray(); } }
			public Exception InnerException { get { return InnerExceptionsCount > 0 ? innerExceptions[0] : null; } }

			public SingleThreadedContext()
			{
				this.items = new ConcurrentQueue<TaskChunk>();
				this.taskCount = 0;
			}

			public void AddTask<T>(Func<Task<T>> task, Action<T> callback = null)
			{
				if (taskCount > 0)
					throw new Exception("Cannot add new task while already running async task queue, recursive approach is not supported!");
				Post(async x =>
				{
					++taskCount;
					try
					{
						T result=await task();
						if (callback != null)
							callback(result);
					}
					catch (Exception ex)
					{
						innerExceptions.Add(ex);
					}
					finally
					{
						--taskCount;
						if (taskCount == 0)
							EndMessageLoop();
					}
				}, null);
			}

			public override void Send(SendOrPostCallback d, object state)
			{
				throw new NotSupportedException("We cannot send to our same thread");
			}

			public override void Post(SendOrPostCallback d, object state)
			{
				items.Enqueue(new TaskChunk(d, state));
				workItemsWaiting.Set();
			}

			private void EndMessageLoop()
			{
				Post(x => done = true, null);
			}

			public void BeginMessageLoop()
			{
				if (taskCount > 0)
					throw new Exception("Cannot start new task loop while already running async task queue, recursive approach is not supported!");
				innerExceptions.Clear();
				done = false;
				while (!done)
				{
					if (items.TryDequeue(out TaskChunk task))
						task.Execute();
					else
						workItemsWaiting.WaitOne();
				}
			}

			public override SynchronizationContext CreateCopy()
			{
				return this;
			}

			public void Dispose()
			{
				workItemsWaiting.Dispose();
			}
		}

		private readonly SingleThreadedContext context;

		public AsyncRunner()
		{
			context = new SingleThreadedContext();
		}

		public void AddTask<T>(Func<Task<T>> task, Action<T> callback = null)
		{
			context.AddTask(task, callback);
		}

		public void AddTask(Func<Task> task, Action callback = null)
		{
			if (callback != null)
				AddTask(async () => { await task(); return true; }, x => callback());
			else
				AddTask(async () => { await task(); return true; });
		}

		public void RunPendingTasks()
		{
			var previousContext = SynchronizationContext.Current;
			try
			{
				SynchronizationContext.SetSynchronizationContext(context);
				context.BeginMessageLoop();
				if (context.InnerExceptionsCount > 1)
					throw new AggregateException(context.InnerExceptions);
				if (context.InnerException != null)
					throw new AggregateException(context.InnerException);
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(previousContext);
			}
		}

		public void Dispose()
		{
			context.Dispose();
		}
	}
}
