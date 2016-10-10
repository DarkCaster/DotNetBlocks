﻿// FileConfigProvider.cs
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
using DarkCaster.Events;
using DarkCaster.Serialization;

namespace DarkCaster.Config.File
{
	/// <summary>
	/// Config provider, that store config data as files at user's home directory.
	/// Data serialization is provided by ISerializationHelper class,
	/// instance of particular serializer should be provided as parameter at constructor.
	/// </summary>
	public sealed class FileConfigProvider<CFG> : IConfigProviderController<CFG>, IConfigProvider<CFG> where CFG: class, new()
	{
		private FileConfigProvider() {} 
		
		public FileConfigProvider(ISerializationHelper<CFG> serializer, string domain, string id = null)
		{
			if(string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
				id=typeof(CFG).Name.ToLower();
			throw new NotImplementedException("TODO:");
		}
		
		public void Init()
		{
			throw new NotImplementedException("TODO:");
		}
		
		public void Shutdown()
		{
			throw new NotImplementedException("TODO:");
		}
		
		public IConfigProvider<CFG> GetProvider()
		{
			return (IConfigProvider<CFG>)this;
		}
		
		public void Dispose()
		{
			throw new NotImplementedException("TODO:");
		}
		
		public bool IsWriteEnabled
		{
			get
			{
				throw new NotImplementedException("TODO:");
			}
		}
		
		public void WriteConfig(CFG config)
		{
			throw new NotImplementedException("TODO:");
		}
		
		public async Task WriteConfigAsync(CFG config)
		{
			throw new NotImplementedException("TODO:");
		}
		
		public IReadOnlyConfigProvider<CFG> GetReadOnlyProvider()
		{
			return (IReadOnlyConfigProvider<CFG>)this;
		}
		
		public ConfigProviderState State 
		{
			get
			{
				throw new NotImplementedException("TODO:");
			}
		}
		
		public ISafeEvent<ConfigProviderStateEventArgs> StateChangeEvent
		{
			get
			{
				throw new NotImplementedException("TODO:");
			}
		}
		
		public CFG ReadConfig()
		{
			throw new NotImplementedException("TODO:");
		}
	}
}
