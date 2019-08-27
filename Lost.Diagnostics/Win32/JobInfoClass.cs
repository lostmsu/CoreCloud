using System.Collections.Generic;
using System.Linq;

namespace Lost.Diagnostics.Win32
{
	enum JobInfoClass
	{
		BasicAccounting = 1,
		BasicLimits,
		ProcessIDs,
		UserInterfaceRestrictions,
		SecurityLimits,
		EndOfJobTime,
		AssocCompletionPort,
		BasicAndIO,
		ExtendedLimits,
	}
}
