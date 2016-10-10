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
using System.IO;
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
		
		public readonly bool error;
		public readonly Exception exception;
		
		public readonly string actualFilename;
		public readonly string uid;
		public readonly string[] backupFilenames;
			
		private FileHelper() {}
		
		private string GenerateUid(string origFilename)
		{
			if( platform == PlatformID.Unix || platform == PlatformID.MacOSX )
				return origFilename;
			return origFilename.ToLower();
		}
		
		private string GetBaseDir()
		{
			return AppDomain.CurrentDomain.BaseDirectory;
		}
		
		public FileHelper(string dirName, string configName)
		{
		}
		
		public FileHelper(string configURI)
		{
			string decodedUrl=configURI;
			
			//We assume that prefixes like file://, http://, https://, etc means that URI is urlencoded.
			if(configURI.StartsWith("http://",StringComparison.OrdinalIgnoreCase) ||
			   configURI.StartsWith("https://",StringComparison.OrdinalIgnoreCase) ||
			   configURI.StartsWith("ftp://",StringComparison.OrdinalIgnoreCase))
			{
				error=true;
				exception=null;
			}
			else 
			{
				try
				{
					if(configURI.StartsWith("file://",StringComparison.OrdinalIgnoreCase))
						decodedUrl=Path.GetFullPath((WebUtility.UrlDecode(configURI)).Substring(7));
					else
						decodedUrl=Path.GetFullPath(configURI);
				}
				catch(Exception ex)
				{
					error=true;
					exception=ex;
				}
			}
			
			//Set what we got so far.
			//Even in case of access error or URI format error, we still can get default config (but cannot write to it).
			actualFilename=decodedUrl;
			//File URI processed in a such way that it may be used to uniquely identify file requested by URI specified by configURI at input.
			//So, it may be used as caching entry to reuse already created config provider to avoid access conflicts
			uid=GenerateUid(decodedUrl);
			//No backup readonly locations is avilable when we trying config URI directly
			backupFilenames=new string[0];
		}
	}
}
