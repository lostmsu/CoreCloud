using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.Geometry3D
{
	public class KDTreeNode3D
	{
		private Dimension3D splitDimension;
		private float min, max;
		private int lowIndex, highIndex;

		private KDTreeNode3D minChild, maxChild;

		public KDTreeNode3D(Dimension3D splitDimension)
		{
			this.splitDimension = splitDimension;
		}
	}
}
