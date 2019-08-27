using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayTracer.Geometry;
using RayTracer.Material;

namespace RayTracer.Scenes
{
	public sealed class Plane : ISceneObject
	{
		public Surface Surface { get; set; }
		public Vector Norm;
		public double Offset;

		public Intersection Intersect(Ray ray)
		{
			double denom = Norm.DotProduct(ray.Dir);
			if (denom > 0) return Intersection.Empty;
			return new Intersection() {
				Thing = this,
				Ray = ray,
				Distance = (Norm.DotProduct(ray.Start) + Offset) / (-denom)
			};
		}

		public Vector Normal(Vector pos)
		{
			return Norm;
		}
	}
}
