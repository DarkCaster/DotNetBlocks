// ISerializationHelper.cs
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

namespace DarkCaster.Serialization
{
	/// <summary>
	/// Generic interface for helper classes that performs serialization.
	/// For use with IOC.
	/// Support normal user-classes and classes that implements ISerializable interface (TODO).
	/// </summary>
	public interface ISerializationHelper<T>
	{
		/// <summary>
		/// Serialize the specified target object. Returns serialized data as byte array.
		/// </summary>
		/// <returns>Returns serialized data as byte array.</returns>
		/// <param name="target">Target object</param>
		byte[] Serialize(T target);

		/// <summary>
		/// Serialize the specified target object. Returns serialized data as string.
		/// Recomennded use - text transfer protocols and config files.
		/// </summary>
		/// <returns>Returns serialized data as string.</returns>
		/// <param name="target">Target object</param>
		string SerializeToString(T target);

		/// <summary>
		/// Serialize the specified target object. Insert serialized data into destination array at specified offset.
		/// Returns length of serialized data written.
		/// </summary>
		/// <returns>Returns length of serialized data written</returns>
		/// <param name="target">Target object</param>
		/// <param name="dest">Destination array where serialized data will be inserted. Array must have enough free size</param>
		/// <param name="offset">Offset where to start inserting data</param>
		int Serialize(T target, byte[] dest, int offset = 0);

		/// <summary>
		/// Deserialize the specified data array to new object
		/// </summary>
		/// <returns>New object, deserialized from byte array</returns>
		/// <param name="data">byte array with serialized data</param>
		/// <param name="offset">optional offset</param>
		/// <param name="len">optional length</param>
		T Deserialize(byte[] data, int offset = 0, int len = 0);

		/// <summary>
		/// Deserialize the specified string to new object
		/// </summary>
		/// <returns>New object, deserialized from string</returns>
		/// <param name="data">string with serialized data</param>
		T Deserialize(string data);
	}

	/// <summary>
	/// Interface for helper classes that performs serialization.
	/// For use with IOC.
	/// </summary>
	public interface ISerializationHelper
	{
		/// <summary>
		/// Serialize the specified target object. Returns serialized data as byte array.
		/// </summary>
		/// <returns>Returns serialized data as byte array.</returns>
		/// <param name="target">Target object</param>
		byte[] SerializeObj(object target);

		/// <summary>
		/// Serialize the specified target object. Returns serialized data as string.
		/// Recomennded use - text transfer protocols and config files.
		/// </summary>
		/// <returns>Returns serialized data as string.</returns>
		/// <param name="target">Target object</param>
		string SerializeObjToString(object target);

		/// <summary>
		/// Serialize the specified target object. Insert serialized data into destination array at specified offset.
		/// Returns length of serialized data written.
		/// </summary>
		/// <returns>Returns length of serialized data written</returns>
		/// <param name="target">Target object</param>
		/// <param name="dest">Destination array where serialized data will be inserted. Array must have enough free size</param>
		/// <param name="offset">Offset where to start inserting data</param>
		int SerializeObj(object target, byte[] dest, int offset = 0);

		/// <summary>
		/// Deserialize the specified data array to new object
		/// </summary>
		/// <returns>New object, deserialized from byte array</returns>
		/// <param name="data">byte array with serialized data</param>
		/// <param name="offset">optional offset</param>
		/// <param name="len">optional length</param>
		object DeserializeObj(byte[] data, int offset = 0, int len = 0);

		/// <summary>
		/// Deserialize the specified string to new object
		/// </summary>
		/// <returns>New object, deserialized from string</returns>
		/// <param name="data">string with serialized data</param>
		object DeserializeObj(string data);
	}
}
