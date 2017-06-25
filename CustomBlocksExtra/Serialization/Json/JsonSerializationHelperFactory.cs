// JsonSerializationHelperFactory.cs
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
using System.Runtime.Serialization;

namespace DarkCaster.Serialization.Json
{
	/// <summary>
	/// SerializationHelperFactory for JsonSerializationHelper
	/// </summary>
	public sealed class JsonSerializationHelperFactory : ISerializationHelperFactory
	{
		public ISerializationHelper<T> GetHelper<T>()
		{
			try
			{
				if (typeof(ISerializable).IsAssignableFrom(typeof(T)))
					throw new NotSupportedException("TODO");
				return new JsonSerializationHelper<T>();
			}
			catch(Exception ex)
			{
				throw new JsonSerializationFactoryException(typeof(T), true, ex);
			}
		}

		public ISerializationHelper GetHelper(Type type)
		{
			try
			{
				if (typeof(ISerializable).IsAssignableFrom(type))
					throw new NotSupportedException("TODO");
				return (ISerializationHelper)Activator.CreateInstance(typeof(JsonSerializationHelper<>).MakeGenericType(new Type[] { type }));
			}
			catch(Exception ex)
			{
				throw new JsonSerializationFactoryException(type, false, ex);
			}
		}

		public ISerializationHelper<ExtType> GetHelper<ExtType>(Type intType)
		{
			return new SerializationHelperProxy<ExtType>(GetHelper(intType));
		}
	}
}
