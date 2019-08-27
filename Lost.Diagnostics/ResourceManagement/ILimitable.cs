using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;

namespace Lost.Diagnostics.ResourceManagement
{
	/// <summary>
	/// This interface is to control resource limitations
	/// </summary>
	[ServiceContract]
	public interface ILimitable
	{
		/// <summary>
		/// Tries to set a limit for a named resource
		/// </summary>
		/// <param name="resourceName">Resource name</param>
		/// <param name="limit">New limit value for that resource</param>
		/// <param name="completeIn">The time that method have to complete operation</param>
		/// <returns>true in case object will try to set new limit, otherwise false</returns>
		[OperationContract]
		bool SetLimit(string resourceName, float limit, TimeSpan completeIn);
	}

	public static class ResourceLimitsDefaultEndpoints
	{
		/// <summary>
		/// Opens a default interface to control process's limits
		/// </summary>
		/// <param name="process">A process to open a control for</param>
		/// <returns>An interface which allows to control process's limits</returns>
		public static ILimitable AsLimitable(this Process process)
		{
			if (process == null) throw new ArgumentNullException("process");

			var binding = new NetNamedPipeBinding();

			var address = string.Format("net.pipe://{1}/{0}.ProcessLimits",
				process.Id, process.MachineName);
			var endpoint = new EndpointAddress(address);

			return ChannelFactory<ILimitable>.CreateChannel(binding, endpoint);
		}
	}
}
