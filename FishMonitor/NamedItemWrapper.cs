using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishMonitor
{
	public class NamedItemWrapper<T>
	{
		public string Name { get; set; }
		public T Item { get; set; }
		public override string ToString()
		{
			return Name;
		}
	}
}
