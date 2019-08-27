using System;
using System.Collections.Generic;
using System.Linq;

namespace Lost.Diagnostics.Win32
{
	[Flags]
	enum ProcessCreationFlags : uint
	{
		OutsideJob = 0x01000000,
		DefaultErrorMode = 0x04000000,
		NewConsole = 0x00000010,
		NewProcessGroup = 0x00000200,
		NoWindow = 0x08000000,
		Protected = 0x00040000,
		/// <summary>
		/// Allows the caller to execute a child process that bypasses the process restrictions that would normally be applied automatically to the process.
		/// Windows 2000:  This value is not supported.
		/// </summary>
		PreserveCodeAuthzLevel = 0x02000000,
		SeparateWowVDM = 0x00000800,
		SharedWowVDM = 0x00001000,
		Suspended = 0x00000004,
		UnicodeEnvironment = 0x00000400,
		DebugOnlyThisProcess = 0x00000002,
		Debug = 0x00000001,
		Detached = 0x00000008,
		UseStartupInfoEx = 0x00080000,
		InheritAffinity = 0x00010000,
	}
}
