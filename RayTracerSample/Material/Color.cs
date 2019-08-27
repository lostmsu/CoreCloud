using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayTracer.Material
{
	public struct Color
	{
		public double R;
		public double G;
		public double B;

		public Color(double r, double g, double b) { R = r; G = g; B = b; }
		public Color(double brightness)
		{
			R = G = B = brightness;
		}
		public Color(string str)
		{
			string[] nums = str.Split(',');
			if (nums.Length != 3) throw new ArgumentException();
			R = double.Parse(nums[0]);
			G = double.Parse(nums[1]);
			B = double.Parse(nums[2]);
		}

		public static Color Make(double r, double g, double b) { return new Color(r, g, b); }

		public readonly static Color Black = new Color { };
		public readonly static Color White = new Color(1, 1, 1);
		public readonly static Color Grey = new Color(0.5);

		public static Color operator*(double n, Color v)
		{
			return new Color { R = n * v.R, G = n * v.G, B = n * v.B };
		}
		public static Color operator*(Color v1, Color v2)
		{
			return new Color { R = v1.R * v2.R, G = v1.G * v2.G, B = v1.B * v2.B };
		}

		public static Color operator+(Color v1, Color v2)
		{
			return new Color { R = v1.R + v2.R, G = v1.G + v2.G, B = v1.B + v2.B };
		}
		public static Color operator-(Color v1, Color v2)
		{
			return new Color { R = v1.R - v2.R, G = v1.G - v2.G, B = v1.B - v2.B };
		}

		public static readonly Color Background = Make(0, 0, 0);
		public static readonly Color DefaultColor = Make(0, 0, 0);

		public static double Legalize(double d)
		{
			return d > 1 ? 1 : d;
		}

		internal System.Drawing.Color ToDrawingColor()
		{
			return System.Drawing.Color.FromArgb((int)(Legalize(R) * 255), (int)(Legalize(G) * 255), (int)(Legalize(B) * 255));
		}

	}
}
