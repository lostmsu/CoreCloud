using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.Geometry3D
{
	public interface IScene3D
	{
		int LastObject { get; }
		void Sphere(Sphere3D sphere);
		void Color(double r, double g, double b, double a);
		void Color();
	}
}
