﻿// IExitNode.cs
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
using DarkCaster.Events;

namespace DarkCaster.DataTransfer.Server
{
	public interface IExitNode : INode
	{
		ISafeEvent<NewTunnelEventArgs> IncomingConnectionEvent { get; }

		/// <summary>
		/// Raised when some of the internal INode components detects error condition,
		/// and failed to maintain it's normal operation.
		/// You should discard (call Disconnect and Dispose) any active and incoming connections (ITunnel objects) from this ExitNode
		/// when this event occurs, and Dispose failed ExitNode object - it should not be used anymore:
		/// new connections may not work (throw exceptions), current connections may fail.
		/// </summary>
		ISafeEvent<NodeFailEventArgs> NodeFailEvent { get; }

		/// <summary>
		/// Perform init. May block while processing.
		/// After init is complete it is ready to accept new connections and spawn new ITunnel instances.
		/// </summary>
		void Init();

		/// <summary>
		/// Perform shutdown of INode instance.
		/// Should be called by upstream INode or user code (in case of IExitTunnel)
		/// This method should be used to stop all running tasks that can spawn new tunnels,
		/// and prepare instance to be disposed.
		/// </summary>
		void Shutdown();
	}
}
