///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
////                                                                               ////
////    Copyright 2017 Christian 'ketura' McCarty                                  ////
////                                                                               ////
////    Licensed under the Apache License, Version 2.0 (the "License");            ////
////    you may not use this file except in compliance with the License.           ////
////    You may obtain a copy of the License at                                    ////
////                                                                               ////
////                http://www.apache.org/licenses/LICENSE-2.0                     ////
////                                                                               ////
////    Unless required by applicable law or agreed to in writing, software        ////
////    distributed under the License is distributed on an "AS IS" BASIS,          ////
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   ////
////    See the License for the specific language governing permissions and        ////
////    limitations under the License.                                             ////
////                                                                               ////
///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XGEF
{
	public static class Extensions
	{
		public static bool CanConvertTo(this object obj, Type toType)
		{
			try
			{
				Expression.Convert(Expression.Parameter(obj.GetType(), null), toType);
				return true;
			}
			catch (InvalidOperationException)
			{
				return false;
			}
		}
	}

	public static class ListExtensions
	{
		public static void Swap<T>(this IList<T> list, int indexA, int indexB)
		{
			T tmp = list[indexA];
			list[indexA] = list[indexB];
			list[indexB] = tmp;
		}

		public static void Swap<T>(this IList<T> list, T A, T B)
		{
			list.Swap(list.IndexOf(A), list.IndexOf(B));
		}

		public static void ReplaceAt<T>(this IList<T> list, int index, T item)
		{
			list.RemoveAt(index);
			list.Insert(index, item);
		}

		public static void Replace<T>(this IList<T> list, T A, T B)
		{
			list.ReplaceAt(list.IndexOf(A), B);
		}

		public static string ToCSVString<T>(this IEnumerable<T> me)
		{
			return string.Join(", ", me.ToArray());
		}

		public static string ToStringList<T>(this IEnumerable<T> me, string separator = ", ")
		{
			return string.Join(separator, me.ToArray());
		}
	}

	public static class EnumExtensions
	{
		public static bool HasProperty(this Type type, string name)
		{
			return type
					.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Any(p => p.Name == name);
		}

		public static string GetDescription<T>(this T value)
			where T : Enum
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());

			DescriptionAttribute[] attributes = null;

			if (fi != null)
			{
				attributes =
					(DescriptionAttribute[])fi.GetCustomAttributes(
					typeof(DescriptionAttribute),
					false);
			}

			if (attributes != null && attributes.Length > 0)
				return attributes[0].Description;
			else
				return value.ToString();
		}

		public static IEnumerable<string> GetDescriptions<T>(this T value)
			where T : Enum
		{
			List<string> desc = new List<string>();
			foreach (T t in value.GetValues())
			{
				desc.Add(t.GetDescription());
			}
			return desc;
		}

		public static IEnumerable<T> GetValues<T>(this T value)
			where T : Enum
		{
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static IEnumerable<T> GetValues<T>(this Enum value)
			where T : Enum
		{
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static T Parse<T>(this T me, string str)
			where T : Enum
		{
			if (string.IsNullOrEmpty(str))
				return me.GetValues().First();

			return (T)Enum.Parse(typeof(T), str);
		}

		public static T LowestFlagBit<T>(this Enum flags)
			where T : Enum
		{
			int x = (int)Convert.ChangeType(flags, typeof(int));
			return (T)(object)(~(x & x - 1) & x);
		}

		public static T HighestFlagBit<T>(this Enum flags)
			where T : Enum
		{
			int x = (int)Convert.ChangeType(flags, typeof(int));
			int last = x;
			while (x != 0)
			{
				last = x;
				x &= x - 1;
			}
			return (T)(object)last;
		}
	}
}