// MockConfigBackendFactory.cs
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
using DarkCaster.Config.Private;

namespace Tests.Mocks
{
	public class MockConfigBackendFactory : IConfigBackendFactory
	{
		private readonly MockConfigBackend backend;
		
		public MockConfigBackendFactory(bool writeAllowed, byte[] data=null, float failProb=0.0f)
		{
			backend=new MockConfigBackend(writeAllowed,data,failProb);
		}
		
		public string GetId()
		{
			return "MockConfigBackend for unit tests";
		}
		
		public IConfigBackend Create()
		{
			return backend;
		}
		
		public void Destroy(IConfigBackend target)
		{
			var result=target as MockConfigBackend;
			if(result==null)
				throw new Exception("Wrong backend type!");
			if(result!=backend)
				throw new Exception("Wrong backend instance!");
			result.Dispose();
		}
	}
}
