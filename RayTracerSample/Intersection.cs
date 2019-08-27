using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayTracer.Scenes;
using RayTracer.Geometry;

namespace RayTracer
{
	public struct Intersection: IComparable<Intersection>
	{
		public ISceneObject Thing;
		public Ray Ray;
		public double Distance;

		public bool Intersects
		{
			get { return Thing != null && !double.IsPositiveInfinity(Distance); }
		}

		public static readonly Intersection Empty = new Intersection { Thing = null, Ray = new Ray { }, Distance = double.PositiveInfinity };

		public int CompareTo(Intersection other)
		{
			return Distance.CompareTo(other.Distance);
		}
	}
}
