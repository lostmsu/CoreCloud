using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Net.PeerToPeer;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CoreCloud.Service
{
	class CloudHost
	{
		static PeerNameRegistration registration;
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				HelloWorld();
				return;
			}

			switch (args[0])
			{
			case "demo":
				Demo();
				return;

			case "parallel":
				Host<ParallelEngine, IParallelEngine>(ParallelEngine.DefaultPort);
				return;

			case "host":
				Host<ExecutionEngine, IExecutionEngine>(ExecutionEngine.DefaultPort);
				return;

			default:
				Console.Error.WriteLine("unknown parameter: " + args[0]);
				if (Debugger.IsAttached)
					Console.ReadKey();
				break;
			}
		}

		static ServiceHost host;

		private static void Host<T, I>(int port)
			where T: I, new()
		{
			string target = "http://localhost" +":" + port + "/" + typeof(I).FullName;
			Console.Write("hosting " + typeof(T).FullName +" at " + target + "...");
			var ee = new T();
			host = new ServiceHost(ee);
			//foreach (var ip in GetAllIPAddresses()) {
			//    string hostname = ip.ToString();
			//    if (ip.AddressFamily == AddressFamily.InterNetworkV6)
			//        hostname = "[" + ip + "]";
			//    host.AddServiceEndpoint(typeof(I), new BasicHttpBinding(), "http://" + hostname + ":" + port + "/" + typeof(I).FullName);
			//}
			host.AddServiceEndpoint(typeof(I), new BasicHttpBinding(), target);
			//host.AddServiceEndpoint(typeof(I), new NetTcpBinding(), "net.tcp://localhost:" + port + "/" + typeof(I).FullName);
			host.Open();
			Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
			Console.WriteLine("OK");

			Publish<I>(port);
			// Publish<I>(port + 1);

			Console.Write("press any key...");
			Console.ReadKey();
			host.Close();
			host = null;
			var del = ee as IDisposable;
			if (del != null) {
				Console.Write("stopping...");
				del.Dispose();
				Console.WriteLine("OK");
			}
		}

		private static IPAddress[] GetAllIPAddresses()
		{
			string host = Dns.GetHostName();
			return Dns.GetHostEntry(host).AddressList.ToArray();
		}

		private static void Publish<I>(int port)
		{
			Console.Write("publishing " + typeof(I) + " via PNRP...");
			var name = new PeerName(typeof(I).FullName, PeerNameType.Unsecured);
			registration = new PeerNameRegistration(name, port);
			try {
				registration.Start();
				Console.WriteLine("published {0}:{1}", name, port);
			} catch (Exception e) {
				Console.WriteLine("FAILED");
				Console.Error.WriteLine(e.Message);
			}
		}

		static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			if (registration != null)
				registration.Stop();

			if (host != null)
				host.Close();
		}

		private static void Demo()
		{
			var assembly = Assembly.GetEntryAssembly();
			var name = assembly.GetName().ToString();

			var engine = new ExecutionEngine();
			engine.Start(name, new Uri(Environment.GetCommandLineArgs()[0]));
		}

		private static void HelloWorld()
		{
			Console.WriteLine("Hello world!");
			Console.ReadKey();
		}
	}
}
