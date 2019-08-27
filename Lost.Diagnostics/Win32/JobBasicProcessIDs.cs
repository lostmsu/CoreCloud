using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lost.Diagnostics.Win32
{
	struct JobBasicProcessIDs
	{
		public int ListSize, RealCount;
		public int[] Identifiers;

		internal static readonly int Size = Marshal.SizeOf(typeof(JobBasicProcessIDs));
	}
}
