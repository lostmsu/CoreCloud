using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayTracer.Geometry
{
	public struct Vector
	{
		public double X;
		public double Y;
		public double Z;

		public static readonly Vector Empty = new Vector { };

		public Vector(double x, double y, double z) { X = x; Y = y; Z = z; }
		public Vector(string str)
		{
			string[] nums = str.Split(',');
			if (nums.Length != 3) throw new ArgumentException();
			X = double.Parse(nums[0]);
			Y = double.Parse(nums[1]);
			Z = double.Parse(nums[2]);
		}
		public static Vector Make(double x, double y, double z) { return new Vector(x, y, z); }
		public static Vector operator*(double n, Vector v)
		{
			return new Vector { X = v.X * n, Y = v.Y * n, Z = v.Z * n };
		}
		public static Vector operator-(Vector v1, Vector v2)
		{
			return new Vector { X = v1.X - v2.X, Y = v1.Y - v2.Y, Z = v1.Z - v2.Z };
		}
		public static Vector operator+(Vector v1, Vector v2)
		{
			return new Vector { X = v1.X + v2.X, Y = v1.Y + v2.Y, Z = v1.Z + v2.Z };
		}
		public double DotProduct(Vector other)
		{
			return (X * other.X) + (Y * other.Y) + (Z * other.Z);
		}
		public double GetLength()
		{
			return Math.Sqrt(DotProduct(this));
		}
		public Vector Normalize()
		{
			double len = GetLength();
			double div = len == 0 ? double.PositiveInfinity : 1 / len;
			return div * this;
		}
		
		public Vector Cross(Vector other)
		{
			return new Vector{	X = this.Y * other.Z - this.Z * other.Y,
								Y = this.Z * other.X - this.X * other.Z,
								Z = this.X * other.Y - this.Y * other.X };
		}
		public static bool Equals(Vector v1, Vector v2)
		{
			return (v1.X == v2.X) && (v1.Y == v2.Y) && (v1.Z == v2.Z);
		}
	}
}
