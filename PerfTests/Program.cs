﻿// Program.cs
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

namespace PerfTests
{
	class Program
	{
		public static void Main(string[] args)
		{
			FastLZSpeedTests.CompressionSpeed();
			FastLZSpeedTests.DecompressionSpeed();
			AsyncRunnerSpeedTests.DirectCalls();
			AsyncRunnerSpeedTests.AsyncRunnerCalls();
			MMHashSpeedTest.Test();
			MsgPackSerHelperSpeedTest.TestSerialize(DarkCaster.Serialization.MsgPack.MsgPackMode.Storage);
			MsgPackSerHelperSpeedTest.TestDeserialize(DarkCaster.Serialization.MsgPack.MsgPackMode.Storage);
			MsgPackSerHelperSpeedTest.TestSerialize(DarkCaster.Serialization.MsgPack.MsgPackMode.StorageCheckSum);
			MsgPackSerHelperSpeedTest.TestDeserialize(DarkCaster.Serialization.MsgPack.MsgPackMode.StorageCheckSum);
			MsgPackSerHelperSpeedTest.TestSerialize(DarkCaster.Serialization.MsgPack.MsgPackMode.Transfer);
			MsgPackSerHelperSpeedTest.TestDeserialize(DarkCaster.Serialization.MsgPack.MsgPackMode.Transfer);
			MsgPackSerHelperSpeedTest.TestSerialize(DarkCaster.Serialization.MsgPack.MsgPackMode.TransferCheckSum);
			MsgPackSerHelperSpeedTest.TestDeserialize(DarkCaster.Serialization.MsgPack.MsgPackMode.TransferCheckSum);
			JsonSerHelperSpeedTest.TestSerialize();
			JsonSerHelperSpeedTest.TestDeserialize();
			JsonSerHelperSpeedTest.TestSerializeText();
			JsonSerHelperSpeedTest.TestDeserializeText();
			BinarySerHelperSpeedTest.TestSerialize();
			BinarySerHelperSpeedTest.TestDeserialize();

			const int spass1 = 1000;
			const int spass2 = 2000;
			const int spass3 = 3000;

			const int srpass1 = 3000;
			const int srpass2 = 5000;
			const int srpass3 = 10000000;

			var subs=SafeEventSpeedTests.SafeEvent_Subscribe(spass1);
			SafeEventSpeedTests.SafeEvent_Unsubscribe(subs);
			subs = SafeEventSpeedTests.SafeEvent_Subscribe(spass2);
			SafeEventSpeedTests.SafeEvent_Unsubscribe(subs);
			subs = SafeEventSpeedTests.SafeEvent_Subscribe(spass3);
			SafeEventSpeedTests.SafeEvent_Unsubscribe(subs);
			subs=SafeEventSpeedTests.SafeEvent_SubscribeRaise(srpass1);
			SafeEventSpeedTests.SafeEvent_Raise(subs,srpass2);
			SafeEventSpeedTests.SafeEvent_UnsubscribeRaise(subs);
			SafeEventSpeedTests.SafeEvent_RaiseSingle(srpass3);
			SafeEventSpeedTests.SafeEvent_RaiseMulti(5,srpass3);

			subs = SafeEventSpeedTests.SafeEventDbg_Subscribe(spass1);
			SafeEventSpeedTests.SafeEventDbg_Unsubscribe(subs);
			subs = SafeEventSpeedTests.SafeEventDbg_Subscribe(spass2);
			SafeEventSpeedTests.SafeEventDbg_Unsubscribe(subs);
			subs = SafeEventSpeedTests.SafeEventDbg_Subscribe(spass3);
			SafeEventSpeedTests.SafeEventDbg_Unsubscribe(subs);
			subs = SafeEventSpeedTests.SafeEventDbg_SubscribeRaise(srpass1);
			SafeEventSpeedTests.SafeEventDbg_Raise(subs, srpass2);
			SafeEventSpeedTests.SafeEventDbg_UnsubscribeRaise(subs);
			SafeEventSpeedTests.SafeEventDbg_RaiseSingle(srpass3);
			SafeEventSpeedTests.SafeEventDbg_RaiseMulti(5, srpass3);

			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}