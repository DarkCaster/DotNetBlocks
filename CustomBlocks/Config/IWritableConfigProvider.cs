// IConfigProvider.cs
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

namespace DarkCaster.Config
{
	/// <summary>
	/// Client side interface for system that perform config management, i.e. config provider.
	/// This system perform storage operations with serialized data coming from config classes.
	/// Storage methods may vary (on disk files, database, network storage), also provider should perform checks
	/// for resource conflicts (accesing the same storage location from multiple instances), and it must be thread safe.
	/// </summary>
	public interface IWritableConfigProvider<CFG> : IReadOnlyConfigProvider<CFG> where CFG: class, new()
	{
		/// <summary>
		/// Is config write operation is allowed ?
		/// Config access may be performed in read-only mode, this can be used as simple security mechanism.
		/// This value may be changed only on state change, it must be thread safe and interlocked with state changes.
		/// False also should is returned in situations when it is not possible to determine access mode - in Init and Offline states.
		/// </summary>
		bool IsWriteEnabled { get; }
	}
}
