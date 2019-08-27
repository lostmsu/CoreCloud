using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreCloud
{
	class DummyService: IDummyService
	{
		public void Dummy() { }
	}

	interface IDummyService
	{
		void Dummy();
	}
}
