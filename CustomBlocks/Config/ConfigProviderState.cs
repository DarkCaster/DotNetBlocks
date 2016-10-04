// ConfigProviderState.cs
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
	/// Different states of ConfigProvider.
	/// States can only be changed in one direction: from Init -> to Offline.
	/// When reached Offline state config provider cannot be used anymore, it must be created again.
	/// </summary>
	public enum ConfigProviderState
	{
		/// <summary>
		/// Config provider currently performing it's initialization.
		/// Eventually it will switch to Online, or Offline state in case of error.
		/// If we try to read or write config in this state, exception will be thrown.
		/// </summary>
		Init=0,
		/// <summary>
		/// Config provider is ready to perform config processing requests.
		/// </summary>
		Online=1,
		/// <summary>
		/// Config provider goes offline. It may be a normal part of it operation, or it happened because of error.
		/// Anyway, provider's object cannot be used anymore, and must be recreated.
		/// </summary>
		Offline=-1
	}
}
