using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lost.Geometry1D;

namespace Lost.Geometry3D
{
	public struct AABox3D
	{
		public Vector3D Position;
		public Size3D Size;
		public Vector3D End
		{
			get { return Position + Size; }
			set
			{
				var pos = new Vector3D {
					X = Math.Min(Position.X, value.X),
					Y = Math.Min(Position.Y, value.Y),
					Z = Math.Min(Position.Z, value.Z),
				};
				var end = this.End;
				var size = new Size3D {
					X = new Size1D { value = Math.Max(end.X, value.X) - pos.X },
					Y = new Size1D { value = Math.Max(end.Y, value.Y) - pos.Y },
					Z = new Size1D { value = Math.Max(end.Z, value.Z) - pos.Z },
				};
				Position = pos;
				Size = size;
			}
		}

		internal void uncheckedEnd(Vector3D value)
		{
			Size.uncheckedSet(value - Position);
		}

		public void Add(AABox3D other)
		{
			var pos = Position.MinByAxis(other.Position);
			var end = End.MaxByAxis(other.End);
			Position = pos;
			uncheckedEnd(end);
		}

		public void Add(Vector3D point)
		{
			var pos = Position.MinByAxis(point);
			var end = End.MaxByAxis(point);
			Position = pos;
			uncheckedEnd(end);
		}
	}
}
