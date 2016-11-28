// IConfigProviderController.cs
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

namespace DarkCaster.Config
{
	/// <summary>
	/// Controller instance for config provider.
	/// May be used with IOC\DI scenarios, at such point in your code that control application
	/// init, shutdown, and lifetime of it's main logical blocks.
	/// Must be used to manually perform config provider startup after other dependend logic initialization is complete and all references is set.
	/// Should be used to shutdown config provider and release all handles or other resources on media where config is stored.
	/// Dispose should be used after all config provider's users was stop using config provider instance.
	/// </summary>
	public interface IConfigProviderController<CFG> : IDisposable where CFG: class, new()
	{
		/// <summary>
		/// Perform config provider init, that may include different tasks - it depends on config provider's type.
		/// Operation may block while performing IO operations, establishing database connection, etc.
		/// In case of error - exception will be thrown.
		/// After init is complete - actual config data must be cached to guarantee successful config-read,
		/// also config provider must switch to Online state before return from this method.
		/// </summary>
		void Init();
		
		/// <summary>
		/// Async variant of init.
		/// </summary>
		Task InitAsync();
		
		/// <summary>
		/// Perform config provider shutdown, releasing all connections to storage backend\media.
		/// Operation may block while waiting for pending config-write tasks to complete.
		/// In case of error - exception will be thrown.
		/// Config provider must switch to Offline state before return from this method, even in case of error.
		/// </summary>
		void Shutdown();
		
		/// <summary>
		/// Get config provider instance managed by this controller.
		/// It may share the same object-reference as controller (or may be not).
		/// Should be used with IOC\DI scenarios - provide this instance to your logic instead of concrete config provider's object.
		/// </summary>
		/// <returns>Config provider instance managed by this controller.</returns>
		IConfigProvider<CFG> GetProvider();
		
		/// <summary>
		/// Return read only instance for current config provider managed by this controller.
		/// May be used to provide config read feature and explicitly deny config write operations.
		/// Should be the same object-reference as returned by GetProvider,
		/// state change events and stored raw config data must be synchronized
		/// if it is not.
		/// </summary>
		/// <returns>Instance of IReadOnlyConfigProvider. Can only perform config read (deserialization)</returns>
		IReadOnlyConfigProvider<CFG> GetReadOnlyProvider();
	}
}
