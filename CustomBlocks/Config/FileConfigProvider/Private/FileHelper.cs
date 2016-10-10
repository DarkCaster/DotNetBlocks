// FileHelper.cs
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
using System.Net;

namespace DarkCaster.Config.File.Private
{
	/// <summary>
	/// Internal helper class for use with FileConfigProvider.
	/// It performs platform dependent service operations with config files such as ACL management,
	/// path management, transform path to uri and vice versa, path locking, provide alternative config paths (depends on platform).
	/// </summary>
	internal class FileHelper
	{
		private readonly PlatformID platform = Environment.OSVersion.Platform;		
		
		private FileHelper() {}
		
		public FileHelper(string dirName, string configName)
		{
		}
		
		public FileHelper(string configURI)
		{
			//We assume that prefixes like file://, http://, https://, etc means that URI is urlencoded.
			if(configURI.StartsWith("file://",StringComparison.OrdinalIgnoreCase))
				configURI=(WebUtility.UrlDecode(configURI)).Substring(7);
			else if(configURI.StartsWith("http://",StringComparison.OrdinalIgnoreCase) ||
			        configURI.StartsWith("https://",StringComparison.OrdinalIgnoreCase) ||
			        configURI.StartsWith("ftp://",StringComparison.OrdinalIgnoreCase))
				throw new Exception("Current URI prefix is not supported: "+configURI);
			
		}
		
	}
}
