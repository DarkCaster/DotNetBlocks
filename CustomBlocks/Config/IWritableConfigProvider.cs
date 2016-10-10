// IWritableConfigProvider.cs
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
	/// Client side interface for system that perform config management, i.e. config provider.
	/// This system perform storage operations with serialized data coming from config classes.
	/// Storage methods may vary (on disk files, database, network storage), also provider should perform checks
	/// for resource conflicts (accesing the same storage location from multiple instances), and it must be thread safe.
	/// This interface can be used to perform full read-write operation with config.
	/// It also and may be used to generate interface instance with read-only functionality
	/// of same provider for use with logic that may only perform config read.
	/// </summary>
	public interface IWritableConfigProvider<CFG> : IReadOnlyConfigProvider<CFG> where CFG: class, new()
	{
		/// <summary>
		/// Is config write operation allowed ?
		/// Config access may be performed in read-only mode only. ConfigProvider is responsible to select mode when changing to online state.
		/// This value of this property may be changed only on state change, it must be thread safe and interlocked with state changes.
		/// False also should is returned in situations when it is not possible to determine access mode - in Init and Offline states.
		/// </summary>
		bool IsWriteEnabled { get; }
		
		/// <summary>
		/// Perform config serialization and write to storage media.
		/// This method will block until config is not commited to media.
		/// If IsWriteEnabled == false, or if current state is not Online - exception will be thrown. 
		/// </summary>
		/// <param name="config">Object of class that used to manage config parameters.
		/// Must be supported by serialization backend used with config provider, or exception will be thrown</param>
		void WriteConfig(CFG config);
		
		/// <summary>
		/// Perform config serialization and data-write to storage media.
		/// This method is using async behavior, and should be used with async callers.
		/// Serialization performed as fast as possible,
		/// await and return to caller is performed on io operations
		/// or when avoiding race conditions between multiple write operations.
		/// </summary>
		/// <param name="config">Object of class that used to manage config parameters.
		/// Must be supported by serialization backend used with config provider, or exception will be thrown</param>
		/// <returns>Task for use with async caller</returns>
		Task WriteConfigAsync(CFG config);
	}
}
