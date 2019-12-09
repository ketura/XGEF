///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
////                                                                               ////
////    Copyright 2019-2020 Christian 'ketura' McCarty                             ////
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace XGEF
{
	public class ModRegistry<T> : NamedRegistry<T>
		where T : INameInfo, IOwningModInfo
	{

	}

	public class NamedRegistry<T> : Registry<T>
		where T : INameInfo
	{

	}

	public interface IRegistry<T> : IEnumerable<KeyValuePair<string, T>>
	{
		IEnumerable<string> Keys { get; }
		IEnumerable<T> Values { get; }
		IReadOnlyDictionary<string, T> ReadOnlyDictionary { get; }

		bool Contains(string name);
		bool Contains(T t);
		int Count();

		T Lookup(string name);

		bool Add(string name, T t);
		bool AddRange(IEnumerable<KeyValuePair<string, T>> list);
		bool Replace(T t);
		void AddOrReplace(T t);
		bool Delete(T t);
	}

	public class Registry<T> : Dictionary<string, T>
	{

	}

	//public class Registry<T> : IEnumerable<KeyValuePair<string, T>>
	//{
	//	public IEnumerable<string> Keys { get { return _catalogue.Keys.ToList(); } }
	//	public IEnumerable<T> Values { get { return _catalogue.Values.ToList(); } }
	//	public IReadOnlyDictionary<string, T> ReadOnlyDictionary { get { return _catalogue; } }
	//	protected Dictionary<string, T> _catalogue { get; }

	//	public IEnumerator<KeyValuePair<string, T>> GetEnumerator() { return _catalogue.GetEnumerator(); }
	//	IEnumerator IEnumerable.GetEnumerator() { return _catalogue.GetEnumerator(); }

	//	public virtual bool Contains(string name)
	//	{
	//		return _catalogue.ContainsKey(name);
	//	}

	//	public virtual bool Contains(T t)
	//	{
	//		return _catalogue.ContainsValue(t);
	//	}

	//	public virtual int Count()
	//	{
	//		return _catalogue.Count;
	//	}

	//	public virtual T Lookup(string name)
	//	{
	//		if (this.Contains(name))
	//			return _catalogue[name];
	//		return default;
	//	}

	//	public virtual bool Add(T t)
	//	{
	//		if (!this.Contains(t.Name))
	//		{
	//			_catalogue.Add(t.Name, t);
	//			return true;
	//		}
	//		else
	//			return false;
	//	}

	//	public virtual bool AddRange(IEnumerable<T> list)
	//	{
	//		bool allSuccess = true;
	//		foreach (T t in list)
	//		{
	//			allSuccess = Add(t);
	//		}

	//		return allSuccess;
	//	}

	//	public virtual bool Replace(T t)
	//	{
	//		if (_catalogue.ContainsKey(t.Name))
	//		{
	//			_catalogue[t.Name] = t;
	//			return true;
	//		}
	//		else
	//			return false;
	//	}

	//	public virtual void AddOrReplace(T t)
	//	{
	//		_catalogue[t.Name] = t;
	//	}

	//	public virtual bool Delete(T t)
	//	{
	//		if (_catalogue.ContainsKey(t.Name))
	//		{
	//			_catalogue.Remove(t.Name);
	//			return true;
	//		}
	//		else
	//			return false;
	//	}

	//	public virtual Registry()
	//	{
	//		_catalogue = new Dictionary<string, T>();
	//	}
	//}
}
