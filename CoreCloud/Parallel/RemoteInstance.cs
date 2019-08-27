using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreCloud
{
	public class RemoteInstance<I>
		where I: class
	{
		public RemoteInstance(I instance)
		{
			this.instance = instance;
		}

		readonly I instance;

		public I Instance
		{
			get { return instance; }
		}
	}
}
