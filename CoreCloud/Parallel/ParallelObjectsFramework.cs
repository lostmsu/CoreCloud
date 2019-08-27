using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.PeerToPeer;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CoreCloud
{
	partial class ParallelObjects
	{
		static PeerNameResolver resolver = new PeerNameResolver();

		static IEnumerable<IPEndPoint> Resolve(string name)
		{
			Console.Write("resolving " + name + "...");
			var peername = new PeerName(name, PeerNameType.Unsecured);
			try {
				IPEndPoint[] result = resolver.Resolve(peername)
					.SelectMany(record => record.EndPointCollection)
					// .Where(ep => ep.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
					.ToArray();
				Console.WriteLine(result.Length + " entries");
				return result;
			} catch (PeerToPeerException e) {
				Console.WriteLine("resolve failed: " + e.Message);
				return new IPEndPoint[] { new IPEndPoint(IPAddress.Loopback, 4079) };
			}
			
		}
	}
}
