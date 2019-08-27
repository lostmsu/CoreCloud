using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;

namespace Lost.Diagnostics
{
	public static class PerformanceCounters
	{
		public const string IdleTimePercent = "% Idle Time";
		public const string ProcessorTimePercent = "Current % Processor Time";
	}
}
