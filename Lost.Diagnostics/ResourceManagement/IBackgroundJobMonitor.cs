using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.Diagnostics.ResourceManagement
{
	public interface IBackgroundJobMonitor: ICollection<ResourceUsagePolicy>
	{
		Job Job { get; }
	}
}
