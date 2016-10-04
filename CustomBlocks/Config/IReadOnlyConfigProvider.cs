// IReadOnlyConfigProvider.cs
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
using DarkCaster.Events;

namespace DarkCaster.Config
{
	/// <summary>
	/// Client side interface for system that perform config management, i.e. config provider.
	/// This system perform storage operations with serialized data coming from config classes.
	/// Storage methods may vary (on disk files, database, network storage), also provider must be thread safe.
	/// This is stripped-down, read only version of IConfigProvider interface that may be created from it by using corresponding method.
	/// This interface should be provided for that parts of your program, that needs only to read config.
	/// </summary>
	public interface IReadOnlyConfigProvider<CFG> where CFG: class, new()
	{
		/// <summary>
		/// Gets the current state of ConfigProvider.
		/// Must be thread safe, and to be interlocked with state changes,
		/// so, any thread that want to request current state will get proper value.
		/// No exceptions should be thrown when accessing this property.
		/// </summary>
		/// <value>Current state</value>
		ConfigProviderState State { get; }
		
		/// <summary>
		/// Event for config state change notifications.
		/// </summary>
		ISafeEvent<ConfigProviderStateEventArgs> StateChangeEvent { get; }
		
		/// <summary>
		/// Perform config read, when State == Online.
		/// Deserialize and create new config-class object on return.
		/// </summary>
		/// <returns>Newly created config class instance with current config values.</returns>
		CFG ReadConfig();
	}
}
