// FileConfigProvider.cs
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
using DarkCaster.Serialization;
using DarkCaster.Config.Private;
using DarkCaster.Config.Files.Private;

namespace DarkCaster.Config.Files
{
	/// <summary>
	/// Config provider, that store config data as files at user's home directory.
	/// Data serialization is provided by ISerializationHelper class,
	/// instance of particular serializer should be provided as parameter at constructor.
	/// </summary>
	public sealed class FileConfigProvider<CFG> : BasicConfigProvider<CFG>
		where CFG: class, new()
	{
		/// <summary>
		/// Create new FileConfigProvider instance.
		/// </summary>
		/// <param name="serializer">Serializer that will encode and decode config data into classes</param>
		/// <param name="dirName">Directory name for config files storage. Directory location is platform dependend.</param>
		/// <param name="id"></param>
		public FileConfigProvider(ISerializationHelper<CFG> serializer, string dirName, string id)
			: this(serializer, new FileConfigBackendFactory(dirName,id)) {}
		
		public FileConfigProvider(ISerializationHelper<CFG> serializer, string filename)
			: this(serializer, new FileConfigBackendFactory(filename)) {}
		
		public FileConfigProvider(ISerializationHelper<CFG> serializer, IConfigBackendFactory backendFactory)
			: base(serializer,backendFactory) {}
		
		public override void Init()
		{
			throw new NotImplementedException("TODO");
		}
		
		public override void Shutdown()
		{
			throw new NotImplementedException("TODO");
		}
				
		public override void WriteConfig(CFG config)
		{
			throw new NotImplementedException("TODO");
		}
		
		public override async Task WriteConfigAsync(CFG config)
		{
			throw new NotImplementedException("TODO");
		}
		
		public override CFG ReadConfig()
		{
			throw new NotImplementedException("TODO");
		}
		
		public override void MarkConfigForDelete()
		{
			throw new NotImplementedException("TODO");
		}
	}
}
