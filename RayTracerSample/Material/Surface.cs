using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayTracer.Geometry;

namespace RayTracer.Material
{
	public struct Surface
	{
		public Func<Vector, Color> Diffuse;
		public Func<Vector, Color> Specular;
		public Func<Vector, double> Reflect;
		public Func<Vector, double> RefractionIndex;
		public Func<Vector, double> RefractedPercent;
		public double Roughness;
	}
}
