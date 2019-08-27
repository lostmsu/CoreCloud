using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.Geometry1D
{
	public struct Size1D
	{
		internal double value;

		public double Value
		{
			get { return value; }
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException("X");
				this.value = value;
			}
		}

		public static Size1D operator +(Size1D a, Size1D b)
		{
			return new Size1D {
				value = a.value + b.value,
			};
		}

		public void Add(Size1D value)
		{
			this.value += value.value;
		}

		public static implicit operator double(Size1D size){
			return size.value;
		}

		public static explicit operator Size1D(double size)
		{
			return new Size1D{
				Value = size,
			};
		}

		public static readonly Size1D Infinite = new Size1D { value = double.PositiveInfinity };
	}
}
