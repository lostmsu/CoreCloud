using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace CoreCloud
{
	[ServiceContract]
	public interface IParallelEngine
	{
		[OperationContract]
		Uri Create(string typeName, string assemblyName, Uri imageSource);

		[OperationContract]
		bool Has(string assemblyName);

		[OperationContract]
		void Upload(byte[] assembly);
	}
}
