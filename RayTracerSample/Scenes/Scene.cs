using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayTracer.Geometry;

namespace RayTracer.Scenes
{
	public class Scene
	{
		public ISceneObject[] Things;
		public Light[] Lights;
		public Camera Camera;

		public IEnumerable<Intersection> Intersect(Ray r)
		{
			return	from thing in Things
					select thing.Intersect(r);
		}
	}
}
