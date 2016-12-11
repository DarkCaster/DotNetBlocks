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
		#pragma warning disable 618
		/// <summary>
		/// Create new FileConfigProvider instance.
		/// </summary>
		/// <param name="serializer">Serializer that will encode and decode config data into classes</param>
		/// <param name="dirName">Directory name for config files storage. Directory location is platform dependend</param>
		/// <param name="id">Config ID. Will be used when generating real config file name</param>
		public FileConfigProvider(ISerializationHelper<CFG> serializer, string dirName, string id)
			: this(serializer, new FileConfigBackendFactory(dirName,id)) {}
		
		/// <summary>
		/// Create new FileConfigProvider instance.
		/// </summary>
		/// <param name="serializer">Serializer that will encode and decode config data into classes</param>
		/// <param name="filename">Config file filename</param>
		public FileConfigProvider(ISerializationHelper<CFG> serializer, string filename)
			: this(serializer, new FileConfigBackendFactory(filename)) {}
		#pragma warning restore 618
		
		[Obsolete("This constructor is not recommended for direct use. Dedicated for unit testing.")]
		public FileConfigProvider(ISerializationHelper<CFG> serializer, IConfigBackendFactory backendFactory)
			: base(serializer,backendFactory) {}
		
		public override void Init()
		{
			try { base.Init(); }
			catch(ConfigProviderException ex) { throw new FileConfigProviderInitException(ex); }
		}
		
		public override void Shutdown()
		{
			try { base.Shutdown(); }
			catch(ConfigProviderException ex) { throw new FileConfigProviderDeinitException(ex); }
		}
				
		public override void WriteConfig(CFG config)
		{
			try { base.WriteConfig(config); }
			catch(ConfigProviderException ex) { throw new FileConfigProviderWriteException(ex); }
		}
		
		public override async Task WriteConfigAsync(CFG config)
		{
			try { await base.WriteConfigAsync(config); }
			catch(ConfigProviderException ex) { throw new FileConfigProviderWriteException(ex); }
		}
		
		public override CFG ReadConfig()
		{
			try { return base.ReadConfig(); }
			catch(ConfigProviderException ex) { throw new FileConfigProviderReadException(ex); }
		}
		
		public override void MarkConfigForDelete()
		{
			try { base.MarkConfigForDelete(); }
			catch(ConfigProviderException ex) { throw new FileConfigProviderWriteException(ex); }
		}
	}
}
