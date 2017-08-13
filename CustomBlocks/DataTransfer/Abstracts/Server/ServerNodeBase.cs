// ServerNodeBase.cs
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
using System.Threading.Tasks;
using DarkCaster.DataTransfer.Config;

namespace DarkCaster.DataTransfer.Server
{
	/// <summary>
	/// Base class with common logic for server nodes.
	/// Should be used only with server nodes.
	/// </summary>
	public abstract class ServerNodeBase : INode
	{
		protected readonly INode upstreamNode;
		protected volatile INode downstreamNode;

		protected ServerNodeBase(INode upstream)
		{
			upstreamNode = upstream;
			upstreamNode.RegisterDownstream(this);
		}

		public virtual async Task InitAsync()
		{
			await upstreamNode.InitAsync();
		}

		public virtual void RegisterDownstream(INode downstream)
		{
			downstreamNode = downstream;
		}

		public virtual async Task NodeFailAsync(Exception ex)
		{
			await downstreamNode.NodeFailAsync(ex);
		}

		public virtual async Task ShutdownAsync()
		{
			await upstreamNode.ShutdownAsync();
		}

		public virtual void Dispose()
		{
			upstreamNode.Dispose();
		}

		public abstract Task OpenTunnelAsync(ITunnelConfig config, ITunnel upstream);
	}
}
