using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.Diagnostics.Win32
{
	[Flags]
	enum ThreadAccessRights : int
	{
		SuspendResume = 0x0002,
		Terminate = 0x0001,
	}
}
