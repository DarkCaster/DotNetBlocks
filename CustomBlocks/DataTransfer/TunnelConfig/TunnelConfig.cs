﻿// TunnelConfig.cs
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
using System.Collections.Concurrent;
namespace DarkCaster.DataTransfer.Config
{
	[Serializable]
	public class TunnelConfig : ITunnelConfig
	{
		private readonly ConcurrentDictionary<string, object> storage;
		public ConcurrentDictionary<string, object> Storage { get { return storage; } }
		public TunnelConfig() { storage = new ConcurrentDictionary<string, object>(); }
		public TunnelConfig(ConcurrentDictionary<string, object> storage) { this.storage = storage; }

		public void Set(string key, object val)
		{
			storage.AddOrUpdate(key, val, (k, old_v) => val);
		}
		public object Get(string key)
		{
			object result = null;
			storage.TryGetValue(key,out result);
			return result;
		}
	}
}