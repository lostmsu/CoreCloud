using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost
{
	public class MathEx
	{
		public static double Sqr(double value)
		{
			return value * value;
		}
	}

	namespace MathExtensions
	{
		public static class MathExtensions
		{
			public static double Sqr(this double value)
			{
				return value * value;
			}
		}
	}
}
