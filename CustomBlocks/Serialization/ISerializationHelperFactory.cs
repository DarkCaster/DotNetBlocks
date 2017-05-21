// ISerializationHelperFactory.cs
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

namespace DarkCaster.Serialization
{
	/// <summary>
	/// Interface for factory object that used for on-demand generation of SerializationHelper objects for various types.
	/// Newly created SerializationHelper objects returned as ISerializationHelper instances.
	/// For use with IOC and such techniques.
	/// </summary>
	public interface ISerializationHelperFactory
	{
		/// <summary>
		/// Create new generic ISerializationHelper object for specific type.
		/// In case of error throw exception based on SerializationFactoryException:
		/// some SerializationHelpers might need additional work for custom classes to work
		/// (example - CBOR serialization may need to setup additional mapping classes). 
		/// </summary>
		/// <returns>ISerializationHelper object for use with specific type</returns>
		/// <typeparam name="T">Type that ISerializationHelper will serialize from\to</typeparam>
		ISerializationHelper<T> GetHelper<T>();

		/// <summary>
		/// Create new non-generic ISerializationHelper object for specific type.
		/// May be useful when you do not know exactly what type you need to serialize, and can only operate with "object".
		/// In case of error throw exception based on SerializationFactoryException:
		/// some SerializationHelpers might need additional work for custom classes to work
		/// (example - CBOR serialization may need additional mapping classes).
		/// </summary>
		/// <returns>non-generic ISerializationHelper object for use with specific type</returns>
		/// <param name="type">Type that ISerializationHelper will serialize from\to</param>
		ISerializationHelper GetHelper(Type type);

		/// <summary>
		/// Wrapper for non-generic <code>GetHelper(Type type)</code>.
		/// Return generic ISerializationHelper that use non generic serialization backend.
		/// For use as drop-in replacement to generic <code>GetHelper<T>()</code>.
		/// Serialization helper instance returned by this method perform additional type conversion
		/// from external type <code>ExtType</code> to internal type <code>intType</code>
		/// before performing serialization and vise versa when performing deserialization.
		/// May be also useful when you need to get generic ISerializationHelper for interface types.
		/// (you still need to provide concrete type).
		/// </summary>
		/// <returns>ISerializationHelper object for use with specific <code>ExtType</code> type.</returns>
		/// <param name="intType">Concrete type that ISerializationHelper will serialize from\to.</param>
		/// <typeparam name="ExtType">Type that ISerializationHelper will use externally.</typeparam>
		ISerializationHelper<ExtType> GetHelper<ExtType>(Type intType);
	}
}
