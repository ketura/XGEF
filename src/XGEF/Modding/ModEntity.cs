using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XGEF;

namespace XGEF
{
	public interface IModEntity<T> : INameInfo, IOwningModInfo
		where T : INameInfo, IOwningModInfo
	{
		ModRegistry<T> ExtraValues { get; }
	}

	public interface IModEntity : IModEntity<StringInfo> { }
}
