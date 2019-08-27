using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Resource = System.String;

namespace Lost.Diagnostics
{
	public class BackgroundMonitor: MarshalByRefObject, IDisposable
	{
		readonly Timer checkTimer;
		// readonly Dictionary<Resource, 

		private void CheckResources(Object dummy)
		{
			// достать из очереди требования текущие требования

			// для каждого ресурса вычислить текущее использование

			// если превышение - сгенерировать сообщение

			// вычислить для каждого ресурса новые требования

			// записать их в очередь
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
