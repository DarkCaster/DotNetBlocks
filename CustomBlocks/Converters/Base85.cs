﻿// Base85.cs
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
using System.Text;

namespace DarkCaster.Converters
{
	/// <summary>
	/// Basic Base85 data converter. Inpired by this publication\project:
	/// https://blog.codinghorror.com/c-implementation-of-ascii85/
	/// Main goal of this version - is to make more lightweight and robust converter:
	/// - There are 2 modes of operation:
	///   ignoreErrors = false - act similar to Convert.ToBase64String\FromBase64String, trow similiar exceptions
	///   ignoreErrors = true - do not throw anything, try to salvage and convert all data that is possible
	/// - Avoid possible unicode string problems on decode by converting string to 7-bit-ascii byte array
	/// - Avoid to use complex stuff like memory stream or string builder in order to
	///   maximize performance on decode\encode
	/// - Minimize memory allocations - calculate required arrays size and allocate it only once
	/// 
	/// Converter is fully working now but some features require more work to be complete.
	/// Any contribution is welcomed.
	/// </summary>

	public static class Base85
	{
		//start mark sequence. BStartMark - version of StartMark. used in encode
		private const char BStartMark_1 = '<';
		private const char BStartMark_2 = '~';

		//end mark sequence. BEndMark - version of EndMark, used in encode
		private const char BEndMark_1 = '~';
		private const char BEndMark_2 = '>';
		
		//empty string with marks, used when encoding null or empty data byte arrays with error check is disabled
		private const string EmptyStringWithMarks = "<~~>";

		//using in decode to fast select proper "power of 85" value
		private static readonly uint[] pow85 = { 52200625U, 614125U, 7225U, 85U, 1U };

		public static byte[] Decode(string dataString, bool useMarks = true, bool ignoreErrors = false)
		{
			if(dataString == null)
			{
				if(ignoreErrors)
					return new byte[0];
				throw new ArgumentNullException("dataString", "Encoded data string in null!");
			}
			if(dataString.Length == 0)
				return new byte[0];
			if(string.IsNullOrWhiteSpace(dataString))
			{
				if(ignoreErrors)
					return new byte[0];
				throw new FormatException("Encoded data string does not contain correct encoded characters!");
			}
			//convert source data string to ascii encoding, replacing all non compatible characters with space
			//so it will not interfere with base85's characters, will not contain "graphemes" or stuff like that
			var encoding = Encoding.GetEncoding("us-ascii", new EncoderReplacementFallback(" "), new DecoderExceptionFallback());
			var source = encoding.GetBytes(dataString);
			//counters
			int sourceLen = source.Length;
			int startIdx = 0;
			int endIdx = sourceLen;
			//If using marks, get substring position
			if(useMarks)
			{
				//find start marker
				for(int i=0;i<sourceLen-2;++i)
					if(source[i]==(byte)BStartMark_1 && source[i+1]==(byte)BStartMark_2)
					{
						startIdx=i+2;
						break;
					}
				if(startIdx==0||startIdx>sourceLen-2)
				{
					if(ignoreErrors)
						return new byte[0];
					throw new FormatException("Failed to find correct start marker in encoded data string!");
				}
				//find end marker
				for(int i=sourceLen-1;i>=startIdx;--i)
					if(source[i]==(byte)BEndMark_2 && source[i-1]==(byte)BEndMark_1)
					{
						endIdx=i-1;
						break;
					}
				if(endIdx==sourceLen||endIdx<startIdx)
				{
					if(ignoreErrors)
						return new byte[0];
					throw new FormatException("Failed to find correct end marker in encoded data string!");
				}
			}
			//count real decoded string len
			int decPredictedLen = 0;
			int encPendingLen = 0;
			//count result lenth composed from source 5 symbols blocks
			for(int i = startIdx; i < endIdx; ++i)
			{
				if(source[i] == 122)
				{
					if(encPendingLen == 0)
						encPendingLen = 5;
					else
					{
						if(!ignoreErrors)
							throw new FormatException("Incorrect z symbol at string's input position: " + i.ToString());
						continue;
					}
				}
				else if(source[i] >= 33 && source[i] <= 117)
					encPendingLen++;
				else if(!ignoreErrors && source[i]!=32 && source[i]!=10 && source[i]!=13 && source[i]!=9)
					throw new FormatException("Incorrect symbol at string's input position: " + i.ToString());
				if(encPendingLen == 5)
				{
					decPredictedLen += 4;
					encPendingLen = 0;
				}
			}
			if(encPendingLen == 1 && !ignoreErrors)
				throw new FormatException("Wrong remainder symbols count at string's end");
			//append lenth from remainder not multiple to 5 symbols
			if(encPendingLen > 1)
				decPredictedLen += (encPendingLen - 1);
			//create result byte array
			var result = new byte[decPredictedLen];
			uint tuple = 0U;
			int decPos = 0;
			int encBlkPos = 0;
			for(int i = startIdx; i < endIdx; ++i)
			{
				if(source[i] == 122)
				{
					if(encBlkPos == 0)
					{
						result[decPos] = result[decPos + 1] = result[decPos + 2] = result[decPos + 3] = 0;
						decPos += 4;
					}
					continue;
				}
				if(source[i] < 33 || source[i] > 117)
					continue;
				unchecked
				{
					tuple += (uint)(source[i] - 33) * pow85[encBlkPos];
				}
				encBlkPos++;
				if(encBlkPos == 5)
				{
					result[decPos] = (byte)((tuple >> 24) & 0xFF);
					result[decPos + 1] = (byte)((tuple >> 24 - 8) & 0xFF);
					result[decPos + 2] = (byte)((tuple >> 24 - 16) & 0xFF);
					result[decPos + 3] = (byte)((tuple >> 24 - 24) & 0xFF);
					decPos += 4;
					tuple = 0U;
					encBlkPos = 0;
				}
			}
			//append bytes from remainder block >1 symbols
			if(encBlkPos > 1) //cannot be single byte
			{
				encBlkPos--;
				tuple += pow85[encBlkPos];
				for(int j = 0; j < encBlkPos; ++j)
					result[decPos + j] = (byte)((tuple >> 24 - (j * 8)) & 0xFF);
			}
			return result;
		}

		public static string Encode(byte[] data, bool useMarks = true, bool ignoreErrors = false)
		{
			if(data == null && !ignoreErrors)
				throw new ArgumentNullException("data", "Input byte array cannot be null");
			if(data == null || data.Length == 0)
				return useMarks ? EmptyStringWithMarks : string.Empty;
			//pos
			int rPos = 0;
			//calculate how much space we need
			var fullchunks = data.Length / 4;
			var remainder = data.Length % 4;
			var result = new char[fullchunks * 5 + (remainder > 0 ? 5 : 0) + (useMarks ? 4 /*start+end marks len*/ : 0)];
			//append start mark
			if(useMarks)
			{
				result[rPos] = BStartMark_1;
				result[rPos + 1] = BStartMark_2;
				rPos+=2;
			}
			//encode full chunks
			uint tuple = 0U;
			//process full chunks
			for(int i = 0; i < fullchunks; ++i)
			{
				tuple |= (uint)data[4 * i] << 24;
				tuple |= (uint)data[4 * i + 1] << 16;
				tuple |= (uint)data[4 * i + 2] << 8;
				tuple |= (uint)data[4 * i + 3];
				if(tuple == 0U)
				{
					result[rPos] = 'z';
					rPos++;
				}
				else
				{
					result[rPos + 4] = (char)((tuple % 85U) + 33U);
					result[rPos + 3] = (char)((tuple / 85U % 85U) + 33U);
					result[rPos + 2] = (char)((tuple / 7225U % 85U) + 33U);
					result[rPos + 1] = (char)((tuple / 614125U % 85U) + 33U);
					result[rPos] = (char)((tuple / 52200625U % 85U) + 33U);
					rPos += 5;
					tuple = 0U;
				}
			}
			//process remainder
			if(remainder > 0)
			{
				for(int i = 0; i < remainder; ++i)
					tuple |= (uint)data[4 * fullchunks + i] << (24 - i * 8);
				result[rPos + 4] = (char)((tuple % 85U) + 33U);
				result[rPos + 3] = (char)((tuple / 85U % 85U) + 33U);
				result[rPos + 2] = (char)((tuple / 7225U % 85U) + 33U);
				result[rPos + 1] = (char)((tuple / 614125U % 85U) + 33U);
				result[rPos] = (char)((tuple / 52200625U % 85U) + 33U);
				rPos += (remainder + 1);
			}
			if(useMarks)
			{
				result[rPos] = BEndMark_1;
				result[rPos + 1] = BEndMark_2;
				rPos+=2;
			}
			//7-bit-ascii strings will be generated, no need to use additional Encoding
			return new string(result, 0, rPos);
		}
	}
}
