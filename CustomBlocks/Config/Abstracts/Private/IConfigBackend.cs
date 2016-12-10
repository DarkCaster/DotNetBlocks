// IConfigBackend.cs
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
using System.Threading.Tasks;

namespace DarkCaster.Config.Private
{
	/// <summary>
	/// Internal interface, for use with config provider class.
	/// Perform direct access to shared storage media: read and write of raw config data.
	/// Object of this interface should resolve access conflicts and may be shared across different instances of config providers.
	/// May be used (but it is not required) to simplify and unify internal design of config provider class.
	/// May be also used to simplify unit testing of config provider class.
	/// Not for general and direct use: initialization, deinitialization, usage pattern and realization may differ with various config providers types.
	/// </summary>
	public interface IConfigBackend
	{
		/// <summary>
		/// Is write of config data allowed by storage media ? This is an informational property:
		/// It will try to detect on start, if all nessecary conditions are met to perform data write.
		/// However, even if it is true on start, it may switch back to false after write error. 
		/// </summary>
		bool IsWriteAllowed { get; }
		/// <summary>
		/// Fetch latest snapshot of raw config data. Initial data snapshot must be read and cached from storage media,
		/// when creating object, and it must be updated on Commit. This method must be thread safe, and also it should not block
		/// while performing commit operations.
		/// </summary>
		/// <returns>Copy of current in-memory raw config data snapshot</returns>
		byte[] Fetch();		
		/// <summary>
		/// Commit raw config data to storage media. May fail if write is not allowed, or error happened.
		/// This method must be thread safe, and also resolve all conflicts to storage media.
		/// </summary>
		/// <param name="data">Serialized raw config data that will replace original config file</param>
		void Commit(byte[] data);
		/// <summary>
		/// Commit raw config data to storage media.
		/// Async implementation, that will not block on IO operations.
		/// May fail if write is not allowed, or error happened.
		/// This method must be thread safe, and also resolve all conflicts to storage media.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		Task CommitAsync(byte[] data);
		/// <summary>
		/// Mark config data to be deleted on storage media resource close.
		/// When sharing the same instance of ConfigStorageBackend between multiple providers to avoid resource access conflicts,
		/// data deletion will be performed only after last client release this instance.
		/// </summary>
		void MarkForDelete();
	}
}
