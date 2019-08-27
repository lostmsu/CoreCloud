using System;
using System.Collections.Generic;
using System.Linq;

namespace Lost.Diagnostics.Win32
{
	struct ProcessInformation
	{
		public IntPtr ProcessHandle;
		public IntPtr MainThreadHandle;
		public uint ProcessID;
		public uint MainThreadID;
	}
}
