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

namespace DarkCaster.Config.Files.Private
{
	/// <summary>
	/// Internal helper class for use with FileConfigProvider.
	/// It's main purpose is to generate config file path (platform dependent) and backup paths.
	/// Equals and GetHashCode methods may be used to pick and reuse same instance of FileConfigProvider class for the same config file.
	/// </summary>
	internal sealed class ConfigFileId: IEquatable<ConfigFileId>
	{
		public readonly string actualFilename;
		public readonly string[] backupFilenames;
		
		private readonly PlatformID platform = Environment.OSVersion.Platform;
		private readonly string uid;
		
		public override bool Equals(object obj)
		{
			var other = obj as ConfigFileId;
			if (other == null)
				return false;
			return Equals(other);
		}
		
		public bool Equals(ConfigFileId other)
		{
			return uid == other.uid;
		}

		public override int GetHashCode()
		{
			return uid.GetHashCode();
		}
		
		private ConfigFileId() {}
		
		private string GetBaseDir()
		{
			return AppDomain.CurrentDomain.BaseDirectory;
		}
		
		private string GetConfigBasedir()
		{
			if( platform == PlatformID.Unix || platform == PlatformID.MacOSX )
				return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToLower();
		}
		
		private string GetAppDir()
		{
			return AppDomain.CurrentDomain.BaseDirectory;
		}
		
		public ConfigFileId(string dirName, string configName)
		{
			if( string.IsNullOrEmpty(dirName) || string.IsNullOrWhiteSpace(dirName) ||
			   dirName.IndexOfAny(Path.GetInvalidPathChars())>=0 || dirName.IndexOfAny(Path.GetInvalidFileNameChars())>=0 )
				throw new ArgumentException("dirName is null or contains invalid chars", "dirName");
			
			if( string.IsNullOrEmpty(configName) || string.IsNullOrWhiteSpace(configName) ||
			   configName.IndexOfAny(Path.GetInvalidPathChars())>=0 ||  configName.IndexOfAny(Path.GetInvalidFileNameChars())>=0 )
				throw new ArgumentException("configName is null or contains invalid chars", "configName");
			
			var addon = Path.Combine(dirName,configName);
			if( platform == PlatformID.Unix || platform == PlatformID.MacOSX )
				addon = addon.ToLower();
			
			actualFilename=Path.Combine(GetConfigBasedir(),addon);
			if( platform == PlatformID.Unix || platform == PlatformID.MacOSX )
			{
				uid=actualFilename;
				backupFilenames=new string[2] { Path.Combine("/etc",addon), Path.Combine(GetAppDir(),addon) };
			}
			else
			{
				uid=actualFilename.ToLower();
				backupFilenames=new string[1] { Path.Combine(GetAppDir(),addon) };
			}
		}
		
		private string GenerateUid(string origFilename)
		{
			if( platform == PlatformID.Unix || platform == PlatformID.MacOSX )
				return origFilename;
			return origFilename.ToLower();
		}
		
		public ConfigFileId(string configURI)
		{
			if(string.IsNullOrEmpty(configURI) || string.IsNullOrWhiteSpace(configURI))
				throw new ArgumentException("configURI is null, empty or whitespace","configURI");
			//We assume that prefixes like file://, http://, https://, etc means that URI is urlencoded.
			if(configURI.StartsWith("http://",StringComparison.OrdinalIgnoreCase) ||
			   configURI.StartsWith("https://",StringComparison.OrdinalIgnoreCase) ||
			   configURI.StartsWith("ftp://",StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException("configURI format is not supported","configURI");
			if(configURI.StartsWith("file://",StringComparison.OrdinalIgnoreCase))
				configURI=(WebUtility.UrlDecode(configURI)).Substring(7);
			if(configURI.IndexOfAny(Path.GetInvalidPathChars())>=0 || configURI.IndexOfAny(Path.GetInvalidFileNameChars())>=0 )
				throw new ArgumentException("configURI contains invalid chars", "configURI");
			try
			{
				configURI=Path.GetFullPath(configURI);
				actualFilename=configURI;
			}
			catch(Exception) { actualFilename=""; }
			//File URI processed in a such way that it may be used to uniquely identify file requested by URI specified by configURI at input.
			//So, it may be used as caching entry to reuse already created config provider to avoid access conflicts
			uid=GenerateUid(configURI);
			//No backup readonly locations is avilable when we trying config URI directly
			backupFilenames=new string[0];
		}
	}
}
