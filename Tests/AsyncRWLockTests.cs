using System;
using NUnit.Framework;
using DarkCaster.Async;

namespace Tests
{
	[TestFixture]
	public class AsyncRWLock
	{
		[Test]
		public void Init()
		{
			var test=new AsyncRWLock();
		}
	}
}
