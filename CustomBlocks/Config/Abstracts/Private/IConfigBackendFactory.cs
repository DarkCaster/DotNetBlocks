// IConfigBackendFactory.cs
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

namespace DarkCaster.Config.Private
{
	/// <summary>
	/// Interface for "factory" class, that can create and dispose IConfigBackend on demand.
	/// This is an internal interface, for use with config provider classes.
	/// You should not use this interface directly in most cases.
	/// Mainly used in unit tests to create "mock" backend classes tuned for various test scenarios.
	/// Class of this interface must be fully threadsafe, and may generate
	/// same shared instance of IConfigBackend on each invocation
	/// (IConfigBackend instances may be shared by design for some config provider tests).
	/// Configuration for backend to be created by factory should be stored inside IConfigBackendFactory.
	/// </summary>
	public interface IConfigBackendFactory
	{
		/// <summary>
		/// Get some string resource id, which is uniquely describe resource address for witch created backend will operate
		/// </summary>
		/// <returns></returns>
		string GetId();
		/// <summary>
		/// Create new IConfigBackend, with params preconfigured in factory code
		/// </summary>
		/// <returns>Initialized IConfigBackend object ready for work. May be a single instance.</returns>
		IConfigBackend Create();
		/// <summary>
		/// Perform deinitialize of IConfigBackend, previously created by Create method.
		/// </summary>
		/// <param name="target">IConfigBackend object to be disposed</param>
		void Destroy(IConfigBackend target);
	}
}
