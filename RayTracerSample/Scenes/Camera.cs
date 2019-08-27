using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayTracer.Geometry;

namespace RayTracer.Scenes
{
	public class Camera
	{
		public Vector Pos;
		public Vector Forward;
		public Vector Up;
		public Vector Right;

		public static Camera Create(Vector pos, Vector lookAt)
		{
			Vector forward = (lookAt - pos).Normalize();
			Vector down = new Vector(0, -1, 0);
			Vector right = 1.5 * forward.Cross(down).Normalize();
			Vector up = 1.5 * forward.Cross(right).Normalize();

			return new Camera{ Pos = pos, Forward = forward, Up = up, Right = right };
		}
	}
}
