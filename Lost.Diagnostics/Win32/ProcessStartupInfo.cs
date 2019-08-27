using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lost.Diagnostics.Win32
{
	struct ProcessStartupInfo
	{
		public uint ThisSize;
#pragma warning disable 169
		string reserved;
		public string Desktop;
		public string Title;
		public uint X, Y, Width, Height, RowChars, ColumnChars;
		public ConsoleColors ConsoleColors;
		public StartupInfoFlags Flags;
		public StartupInfoWindowFlags ShowWindow;
		ushort reserved2;
		IntPtr reserved3;
#pragma warning restore 169
		public IntPtr InputHandle, OutputHandle, ErrorHandle;

		public void AdjustSize()
		{
			ThisSize = (uint)Marshal.SizeOf(typeof(ProcessStartupInfo));
		}
	}
}
