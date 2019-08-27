using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayTracer.Geometry;
using RayTracer.Material;

namespace RayTracer.Scenes
{
	public sealed class Sphere : ISceneObject
	{
		public Surface Surface { get; set; }
		public Vector Center;
		public double Radius;

		public Intersection Intersect(Ray ray)
		{
			Vector eo = Center - ray.Start;
			double ldpc = eo.DotProduct(ray.Dir);
			double dist;
			if (ldpc < 0) return Intersection.Empty;

			double disc = Radius * Radius - eo.DotProduct(eo) + ldpc * ldpc;
			if (disc < 0) return Intersection.Empty;
			dist = ldpc - Math.Sqrt(disc);
			return new Intersection {
				Thing = this,
				Ray = ray,
				Distance = dist
			};
		}

		public Vector Normal(Vector pos)
		{
			return (pos - Center).Normalize();
		}
	}
}
