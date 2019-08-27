using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lost.Diagnostics.Win32
{
	struct JobBasicAccounting
	{
		public long TotalUserTime, TotalKernelTime;
		public long ThisPeriodUserTime, ThisPeriodKernelTime;
		public uint PageFaults;
		public uint ProcessCount;
		public uint ActiveProcessCount;
		public uint TerminatedProcessCount;

		internal static readonly int Size = Marshal.SizeOf(typeof(JobBasicAccounting));
	}
}
