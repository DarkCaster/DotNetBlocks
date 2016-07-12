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
	/// </summary>
	public interface ISerializationHelper<T>
	{
		byte[] Serialize(T target);
		T Deserialize(byte[] data);
		string SerializeToString(T target);
		T Deserialize(string data);
	}

	/// <summary>
	/// Interface for helper classes that performs serialization.
	/// For use with IOC.
	/// </summary>
	public interface ISerializationHelper
	{
		byte[] SerializeObj(object target);
		object DeserializeObj(byte[] data);
		string SerializeObjToString(object target);
		object DeserializeObj(string data);
	}
}
