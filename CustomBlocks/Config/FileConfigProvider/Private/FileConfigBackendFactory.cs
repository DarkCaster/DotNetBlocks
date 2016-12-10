// FileConfigBackendFactory.cs
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

using DarkCaster.Config.Private;

namespace DarkCaster.Config.Files.Private
{
	/// <summary>
	/// Factory for FileConfigStorageBackend
	/// </summary>
	public class FileConfigBackendFactory : IConfigBackendFactory
	{
		private readonly ConfigFileId fileId;
		
		private FileConfigBackendFactory() {}
		
		public FileConfigBackendFactory(string dirName, string id)
			: this(new ConfigFileId(dirName,id)) {}
		
		public FileConfigBackendFactory(string filename)
			: this(new ConfigFileId(filename)) {}
		
		internal FileConfigBackendFactory(ConfigFileId fileId) 
		{
			this.fileId = fileId;
		}
		
		public string GetId()
		{
			throw new NotImplementedException("TODO");
		}
		
		public IConfigBackend Create()
		{
			throw new NotImplementedException("TODO");
		}
		
		public void Destroy(IConfigBackend target)
		{
			throw new NotImplementedException("TODO");
		}
	}
}
