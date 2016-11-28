﻿// FileConfigStorageBackendManager.cs
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
using DarkCaster.Config.Private;
	
namespace DarkCaster.Config.Files.Private
{
	/// <summary>
	/// Internal helper class to perform management of FileConfigStorageBackend instances.
	/// </summary>
	internal static class FileConfigStorageBackendManager
	{
		public static IConfigStorageBackend GetBackend(string folder, string name)
		{
			throw new NotImplementedException("TODO");
		}
		
		public static IConfigStorageBackend GetBackend(string filename)
		{
			throw new NotImplementedException("TODO");
		}
		
		public static async Task<IConfigStorageBackend> GetBackendAsync(string folder, string name)
		{
			throw new NotImplementedException("TODO");
		}
		
		public static async Task<IConfigStorageBackend> GetBackendAsync(string filename)
		{
			throw new NotImplementedException("TODO");
		}
		
		public static void FlushBackend(IConfigStorageBackend target)
		{
			throw new NotImplementedException("TODO");
		}
		
		public static async Task FlushBackendAsync(IConfigStorageBackend target)
		{
			throw new NotImplementedException("TODO");
		}
	}
}