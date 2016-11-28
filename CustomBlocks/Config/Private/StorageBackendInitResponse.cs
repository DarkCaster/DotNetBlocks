// StorageBackendInitResponse.cs
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

namespace DarkCaster.Config.Private
{
	//TODO: remove this class
	
	/// <summary>
	/// Container for response from Init method of IConfigStorageBackend.
	/// Response struct and all of it's data should be discarded as fast as possible after all checks is done,
	/// because raw config data read from media may leak some vital info,
	/// or it's size may be not optimal (comments?).
	/// </summary>
	/*public struct StorageBackendInitResponse
	{
		/// <summary>
		/// Raw data from config file (or other media), read by init routine.
		/// It is not checked for validity. May be null, if config file (or other resource) failed to read.
		/// </summary>
		public readonly byte[] rawConfigData;
		/// <summary>
		/// Write is allowed by ACL or other access control method. There is no guarantee that config write will not fail,
		/// this is only an information from storage backend that you have neccessary rights to write config data.
		/// Write access still can be revoked by config provider while it performing switch to "Online" state.
		/// </summary>
		public readonly bool writeAllowed;
		/// <summary>
		/// Any additional information from storage backend. Depends on backend type.
		/// May contain debug info, such as: exact file name that was read (if there are several search locations),
		/// selected storage location for config write, errors happened during media init, etc.
		/// </summary>
		public readonly object additionalInfo;
		
		public StorageBackendInitResponse(byte[] rawConfigData, bool writeAllowed, object additionalInfo)
		{
			this.rawConfigData=rawConfigData;
			this.writeAllowed=writeAllowed;
			this.additionalInfo=additionalInfo;
		}
	}*/
}
