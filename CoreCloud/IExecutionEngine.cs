using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace CoreCloud
{
	
	[ServiceContract]
	public interface IExecutionEngine
	{
		[OperationContract]
		void Start(string assemblyName, Uri imageSource);
	}
}
