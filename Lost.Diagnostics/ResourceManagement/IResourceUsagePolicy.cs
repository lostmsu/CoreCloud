using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.Diagnostics.ResourceManagement
{
	public struct ResourceUsagePolicy
	{
		public string ResourceName;
		public Func<float> Limit;
		public TimeSpan ResponseTime;
		public Func<float> Usage;
		public Action Violated;
	}
}
