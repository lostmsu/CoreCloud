using System;
using System.Collections.Generic;
using System.Linq;

namespace Lost.Diagnostics.Win32
{
	[Flags]
	enum StartupInfoFlags : uint
	{
		SetConsoleSize = 0x00000008,
		SetConsoleColors = 0x00000010,
		SetWindowPosition = 0x00000004,
		UseShowWindow = 0x00000001,
		SetWindowSize = 0x00000002,
		OverrideStdIO = 0x00000100,
		None = 0,
	}
}
