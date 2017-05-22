// ITunnelConfig.cs
//
// The MIT License (MIT)
//
// Copyright (c) 2017 DarkCaster <dark.caster@outlook.com>
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
namespace DarkCaster.DataTransfer.Config
{
	/// <summary>
	/// Interface for tunnel configuration.
	/// May be used to pass some configuration parameters to internal INode\ITunnel components that used in data transfer process,
	/// when creating new IEntryTunnel object.
	/// All methods must be thread safe. Internal storage must be also thread safe and serializable.
	/// Key name is not case sensitive.
	/// </summary>
	public interface ITunnelConfig
	{
		/// <summary>
		/// Set config option value specified by key.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="val">Value.</param>
		void Set<T>(string key, T val);

		/// <summary>
		/// Get config option value specified by key.
		/// </summary>
		/// <returns>Config value. Return null if there is no such key.</returns>
		/// <param name="key">Key.</param>
		T Get<T>(string key);
	}
}
