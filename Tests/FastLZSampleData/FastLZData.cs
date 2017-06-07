// FastLZData.cs
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
using System.IO;

namespace Tests
{
	public static class FastLZData
	{
		private static byte[] input=null;
		public static byte[] GetSampleInput
		{
			get
			{
				if(input==null)
					input=File.ReadAllBytes("input.data");
				return input;
			}
		}

		private static byte[] outputDataLV1 = null;
		public static byte[] GetSampleOutputLV1
		{
			get
			{
				if (outputDataLV1 == null)
					outputDataLV1 = File.ReadAllBytes("output.data.lv1");
				return outputDataLV1;
			}
		}

		private static byte[] outputDataLV2 = null;
		public static byte[] GetSampleOutputLV2
		{
			get
			{
				if (outputDataLV2 == null)
					outputDataLV2 = File.ReadAllBytes("output.data.lv2");
				return outputDataLV2;
			}
		}
	}
}
