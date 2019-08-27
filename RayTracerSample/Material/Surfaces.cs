using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayTracer.Material
{
	public static class Surfaces
	{
		// Only works with X-Z plane.
		public static readonly Surface CheckerBoard =
			new Surface{
				Diffuse = pos => (((int)pos.Z + (int)pos.X) % 2 != 0)
									? Color.White
									: Color.Black,
				Specular = pos => Color.White,
				Reflect = pos => (((int)pos.Z + (int)pos.X) % 2 != 0)
									? .1
									: .7,
				RefractionIndex = pos => double.PositiveInfinity,
				RefractedPercent = pos => 0,
				Roughness = 150
			};


		public static readonly Surface Shiny =
			new Surface{
				Diffuse = pos => Color.Grey,
				Specular = pos => Color.Grey,
				Reflect = pos => .8,
				RefractionIndex = pos => 4,
				RefractedPercent = pos => 0.5,
				Roughness = 50
			};
	}
}
