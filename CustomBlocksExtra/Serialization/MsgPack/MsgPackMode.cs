// MsgPackMode.cs
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

namespace DarkCaster.Serialization.MsgPack
{
	/// <summary>
	/// Mode of operation for MsgPackSerializationHelper.
	/// MsgPack-Cli could not ensure integrity of data and does not throw error when trying to deserialize broken data,
	/// sometimes producing incorrectly deserialized object instead.
	/// So, there are 2 modes that use additional checksum protection,
	/// to deny deserialization of damaged data and throw MsgPackDeserializationException in such cases.
	/// </summary>
	public enum MsgPackMode
	{
		/// <summary>
		/// Optimize for on-disk storage.
		/// Maximize compatibility with past and future MsgPack-Cli library versions, and possible serialized class changes.
		/// Produced data can be deserialized manually without use of MsgPackSerializationHelper.
		/// </summary>
		Storage=0,
		/// <summary>
		/// Optimize for on-disk storage. Also add checksum protection to ensure data integrity.
		/// Maximize compatibility with past and future MsgPack-Cli library versions, and possible serialized class changes.
		/// Produced data must be deserialized with MsgPackSerializationHelper.
		/// </summary>
		StorageCheckSum,
		/// <summary>
		/// Optimize for network data transfer. Maximize serialization performance, and minimize data size.
		/// Compatibility over different versions of MsgPack-Cli library is not guaranteed.
		/// Produced data can be deserialized manually without use of MsgPackSerializationHelper.
		/// </summary>
		Transfer,
		/// <summary>
		/// Optimize for network data transfer. Maximize serialization performance, and minimize data size.
		/// Also add checksum protection to ensure data integrity.
		/// Compatibility over different versions of MsgPack-Cli library is not guaranteed.
		/// Produced data must be deserialized with MsgPackSerializationHelper.
		/// </summary>
		TransferCheckSum
	}
}
