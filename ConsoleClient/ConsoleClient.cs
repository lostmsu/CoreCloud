using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net.PeerToPeer;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace CoreCloud
{
	public class ConsoleClient
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
				ExecutionEngine();
			else
				switch (args[0])
				{
				case "parallel":
					ParallelClient();
					break;

				default:
					Console.Error.WriteLine("unknown parameter: {0}", args[0]);
					return;
				}
				
		}

		private static void ParallelClient()
		{
			var obj = ParallelObject<IFoo>.Create<Foo>();
			var stopwatch = new Stopwatch();
			obj.SetName("World");
			obj.SayHello();
			stopwatch.Start();
			const long sec = 10000000L;
			int counter = 0;
			string result = null;
			Console.Write("testing Hello...");
			while (stopwatch.ElapsedTicks < sec)
			{
				result = obj.SayHello();
				counter++;
			}
			stopwatch.Stop();

			Console.WriteLine("{0} cps", counter * sec / stopwatch.ElapsedTicks);
			Console.WriteLine(result);

			const int chunkSize = 4096;
			const int chunks = 64;
			const int baseNumber = 1024 * 1024 * 1024;

			var dummyTest = ParallelObject<IPrimeTest>.Create<DummyPrimeTest>();
			counter = 0;
			stopwatch.Reset();
			Console.Write("testing DummyPrimeTest...");
			dummyTest.Test(0, chunkSize);
			stopwatch.Start();
			while (stopwatch.ElapsedTicks < sec)
			{
				dummyTest.Test(0, chunkSize);
				counter++;
			}
			stopwatch.Stop();

			Console.WriteLine("{0} cps", counter * sec / stopwatch.ElapsedTicks);

			var primetest = new PrimeTest();
			stopwatch.Reset();
			stopwatch.Start();
			int primes = 0;
			for (int chunk = 0; chunk < chunks; chunk++)
			{
				var res = primetest.Test(baseNumber + chunk * chunkSize, chunkSize);
				primes += res.Count(i => i);
				Console.WriteLine(chunk);
			}
			stopwatch.Stop();

			var local = stopwatch.ElapsedTicks;
			stopwatch.Reset();

			var paralleltest = ParallelObject<IPrimeTest>.Create<PrimeTest>();
			int pprimes = 0;
			stopwatch.Start();
			for (int chunk = 0; chunk < chunks; chunk++)
			{
				var res = paralleltest.Test(baseNumber + chunk * chunkSize, chunkSize);
				pprimes += res.Count(i => i);
				Console.WriteLine(chunk);
			}
			stopwatch.Stop();

			var remote = stopwatch.ElapsedTicks;
			Console.WriteLine("comparison done.");
			Console.WriteLine("remote: answer = {0}; time = {1};", pprimes, remote);
			Console.WriteLine("parallel: answer = {0}; time = {1};", primes, local);
		}

		[ServiceContract]
		public interface IFoo
		{
			[OperationContract]
			void SetName(string name);

			[OperationContract]
			string SayHello();
		}

		[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
		public class Foo: IFoo
		{
			string name = null;

			public void SetName(string name)
			{
				this.name = name;
			}

			public string SayHello()
			{
				return "Hello " + this.name + "!";
			}
		}

		[ServiceContract]
		public interface IPrimeTest
		{
			[OperationContract]
			bool[] Test(int start, int count);
		}

		[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
		public class PrimeTest: IPrimeTest
		{
			internal bool IsPrime(int num)
			{
				if (num < 2) return false;

				for (int i = 2; i <= Math.Sqrt(num); i++)
				{
					if (num % i == 0) return false;
				}

				return true;
			}

			#region IPrimeTest Members

			public bool[] Test(int start, int count)
			{
				var nums = Enumerable.Range(start, count);
				return nums.Select(IsPrime).ToArray();
			}

			#endregion
		}

		[ServiceBehavior(InstanceContextMode= InstanceContextMode.Single)]
		public class DummyPrimeTest : IPrimeTest
		{
			#region IPrimeTest Members

			public bool[] Test(int start, int count)
			{
				return new bool[count];
			}

			#endregion
		}


		private static void ExecutionEngine()
		{
			Console.Write("resolving...");
			var endpoints = Resolve(typeof(IExecutionEngine).FullName);

			Console.Write("OK\nconnecting...");
			var ee = Connect(endpoints.First().EndPointCollection.First());

			Console.Write("OK\nsending a command...");
			ee.Start("dummy", new Uri("http://xxx"));
			Console.WriteLine("OK");
		}

		private static PeerNameRecordCollection Resolve(string serviceName)
		{
			var resolver = new PeerNameResolver();
			var peerName = new PeerName(serviceName, PeerNameType.Unsecured);
			return resolver.Resolve(peerName);
		}

		private static IExecutionEngine Connect(IPEndPoint hostport)
		{
			string hostname = hostport.ToString();
			if (hostport.AddressFamily == AddressFamily.InterNetworkV6)
				hostname = string.Format("[{0}]:{1}", hostport.Address, hostport.Port);

			var endpoint = new EndpointAddress("http://" + hostname + "/" + typeof(IExecutionEngine).FullName);

			var binding = new BasicHttpBinding();

			var channelFactory = new ChannelFactory<IExecutionEngine>(binding, endpoint);
			return channelFactory.CreateChannel();
		}
	}
}
