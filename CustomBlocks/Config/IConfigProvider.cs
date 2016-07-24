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
	/// Different states of ConfigProvider
	/// </summary>
	public enum ConfigProviderState
	{
		/// <summary>
		/// Config provider is down, config cannot be read or written.
		/// If we try to read or write config, exception will be thrown.
		/// </summary>
		Disconnected=0,
		/// <summary>
		/// Config provider in readonly state.
		/// We can only perform config reading.
		/// </summary>
		ReadOnly,
		/// <summary>
		/// Config provider in read\write state.
		/// We can read and write config.
		/// </summary>
		ReadWrite
	}

	/// <summary>
	/// Interface for service class that perform storage operations with serialized data coming from config classes.
	/// Storage methods may vary (on disk files, database, network storage), also provider should perform checks
	/// for resource conflicts (accesing the same storage location from multiple instances), and it must be thread safe.
	/// </summary>
	public interface IConfigProvider : IDisposable
	{
		/// <summary>
		/// Gets the current state of ConfigProvider.
		/// Must be thread safe and volatile, so any thread that want to request current state will get proper value
		/// </summary>
		/// <value>Current state</value>
		ConfigProviderState State { get; }
	}
}
