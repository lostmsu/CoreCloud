using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.Geometry3D
{
	public struct Vector3D
	{
		public double X;
		public double Y;
		public double Z;

		public double Square { get { return X * X + Y * Y + Z * Z; } }
		public double Length { get { return Math.Sqrt(Square); } }

		public void Add(Vector3D other)
		{
			X += other.X;
			Y += other.Y;
			Z += other.Z;
		}

		public void Sub(Vector3D other)
		{
			X -= other.X;
			Y -= other.Y;
			Z -= other.Z;
		}

		public void Mul(double factor)
		{
			X *= factor;
			Y *= factor;
			Z *= factor;
		}

		public double DotProduct(Vector3D other)
		{
			return X * other.X + Y * other.Y + Z * other.Z;
		}

		public static Vector3D Make(double value){
			return new Vector3D {
				X = value,
				Y = value,
				Z = value,
			};
		}

		public void Normalize()
		{
			var length = Length;
			Mul(1 / (length + double.Epsilon));
		}

		public Vector3D Normalized
		{
			get
			{
				var factor = 1 / (Length + double.Epsilon);
				return this * factor;
			}
		}

		public Vector3D MinByAxis(Vector3D other)
		{
			return new Vector3D {
				X = Math.Min(other.X, this.X),
				Y = Math.Min(other.Y, this.Y),
				Z = Math.Min(other.Z, this.Z),
			};
		}

		public Vector3D MaxByAxis(Vector3D other)
		{
			return new Vector3D {
				X = Math.Max(other.X, this.X),
				Y = Math.Max(other.Y, this.Y),
				Z = Math.Max(other.Z, this.Z),
			};
		}

		public static Vector3D operator +(Vector3D a, Vector3D b)
		{
			return new Vector3D {
				X = a.X + b.X,
				Y = a.Y + b.Y,
				Z = a.Z + b.Z,
			};
		}

		public static Vector3D operator -(Vector3D a, Vector3D b)
		{
			return new Vector3D {
				X = a.X - b.X,
				Y = a.Y - b.Y,
				Z = a.Z - b.Z,
			};
		}

		public static Vector3D operator *(Vector3D v, double f)
		{
			return new Vector3D {
				X = v.X * f,
				Y = v.Y * f,
				Z = v.Z * f,
			};
		}

		public static Vector3D operator *(double f, Vector3D v)
		{
			return new Vector3D {
				X = v.X * f,
				Y = v.Y * f,
				Z = v.Z * f,
			};
		}
	}
}
