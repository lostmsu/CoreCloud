using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lost.MathExtensions;

namespace Lost.Geometry3D
{
	public struct Ray3D
	{
		public Vector3D Start;
		public Vector3D Direction;
		public Vector3D End
		{
			get
			{
				return Start + Direction;
			}
			set
			{
				Direction = value - Start;
			}
		}

		public double Intersect(Sphere3D sphere)
		{
			var l = Direction.Normalized;
			var ldpc = l.DotProduct(sphere.Position);
			var d = ldpc.Sqr() - l.Square * (sphere.Position.Square - sphere.RadiusSquare);
			if (d < 0)
				return double.PositiveInfinity;
			d = Math.Sqrt(d);

			var result = ldpc - d;
			if (result >= 0)
				return result;
			result = ldpc + d;
			if (result >= 0)
				return result;
			return double.PositiveInfinity;
		}
	}
}
