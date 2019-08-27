using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lost.Geometry1D;
using Lost.MathExtensions;

namespace Lost.Geometry3D
{
	public struct Sphere3D
	{
		public Size1D Radius;
		public Size1D RadiusSquare
		{
			get { return new Size1D { value = Radius.value.Sqr() }; }
		}
		public Vector3D Position;

		public AABox3D AABox
		{
			get
			{
				return new AABox3D {
					Position = this.Position - Vector3D.Make(Radius),
					Size = Size3D.uncheckedMake(Vector3D.Make(Radius * 2)),
				};
			}
		}

		public bool IntersectsBall(Sphere3D ball)
		{
			return (this.Position - ball.Position).Square <= MathEx.Sqr(Radius + ball.Radius);
		}
	}

	public interface ISphereBounded3D
	{
		Sphere3D SphereBound();
		Sphere3D SphereBound(Sphere3D constraint);
	}
}
