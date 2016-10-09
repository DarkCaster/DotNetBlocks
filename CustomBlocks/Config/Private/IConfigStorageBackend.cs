// IConfigStorageBackend.cs
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
using System.IO;
using System.Threading.Tasks;

namespace DarkCaster.Config.Private
{
	/// <summary>
	/// Internal interface, for use with config provider class.
	/// Storage backend that perform read and write of raw config data to storage media.
	/// May be used (but not required) to simplify and unify internal design of config provider class.
	/// (may be also used to simplify unit testing of config provider class).
	/// Dispose method may be used to drop resource handles, close network connections (for network stprage).
	/// </summary>
	public interface IConfigStorageBackend : IDisposable
	{
		/// <summary>
		/// Call this when performing config provider's init from controller interface.
		/// This method may block while performing access to media.
		/// If init process is failed (resource is unavailable, or read error happened),
		/// exception from failed routine will be thrown,
		/// it may be wrapped within ConfigProviderException by caller if needed.
		/// </summary>
		/// <param name="initData">Object with info needed to perform read or write config file.
		/// Type of data-object depends on particular implementation of storage backend.
		/// May contain single or multiple paths, uri, or other stuff needed to find or access config file.</param>
		/// <returns>Container with information about init.
		/// If response is received and no exception is thrown,
		/// this means that backend init process is complete even if rawConfigData field is null</returns>
		StorageBackendInitResponse Init(object initData);
		/// <summary>
		/// Commit raw config data to storage media. May fail if write is not allowed, or error happened.
		/// Only minimal checks performed here. Write access control, race conditions, and such stuff
		/// should be controlled from caller (config provider).
		/// </summary>
		/// <param name="data">Serialized raw config data that will replace original config file</param>
		void Commit(byte[] data);
		/// <summary>
		/// Commit raw config data to storage media.
		/// Async implementation, that will not block on IO operations.
		/// May fail if write is not allowed, or error happened.
		/// Only minimal checks performed here. Write access control, race conditions, and such stuff
		/// should be controlled from caller (config provider).
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		Task CommitAsync(byte[] data);
	}
}
