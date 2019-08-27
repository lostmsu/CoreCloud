using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayTracer
{
	class RayTracerSampleProgram
	{
		[STAThread]
		static void Main()
		{
			RayTracer.GUI.RayTracerWindow.Run();
		}
	}
}
