using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lost.Geometry1D;

namespace Lost.Geometry3D
{
	public struct Size3D
	{
		public Size1D X, Y, Z;

		internal void uncheckedAdd(Vector3D vector)
		{
			X.value += vector.X;
			Y.value += vector.Y;
			Z.value += vector.Z;
		}

		internal void uncheckedSet(Vector3D vector)
		{
			X.value = vector.X;
			Y.value = vector.Y;
			Z.value = vector.Z;
		}

		static internal Size3D uncheckedMake(Vector3D vector)
		{
			return new Size3D {
				X = new Size1D { value = vector.X },
				Y = new Size1D { value = vector.Y },
				Z = new Size1D { value = vector.Z },
			};
		}

		public static implicit operator Vector3D(Size3D size){
			return new Vector3D{
				X = size.X,
				Y = size.Y,
				Z = size.Z,
			};
		}

		public static explicit operator Size3D(Vector3D vector)
		{
			return new Size3D {
				X = (Size1D)vector.X,
				Y = (Size1D)vector.Y,
				Z = (Size1D)vector.Z,
			};
		}
	}
}
