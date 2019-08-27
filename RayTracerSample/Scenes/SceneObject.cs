using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayTracer.Material;
using RayTracer.Geometry;

namespace RayTracer.Scenes
{
	public interface ISceneObject
	{
		Surface Surface { get; set; }
		Intersection Intersect(Ray ray);
		Vector Normal(Vector pos);
	}
}
