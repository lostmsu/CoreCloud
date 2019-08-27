using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.IO;

namespace CoreCloud
{
	public partial class ParallelObjects
	{
#warning useDefaultWebProxy must be configurable
		static bool useDefaultWebProxy = false;

		internal static readonly ModuleBuilder dynamicModule;

		static ParallelObjects()
		{
			var assemblyName = new AssemblyName("CoreCloud.ParallelObjects");
			var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			dynamicModule = assembly.DefineDynamicModule(assemblyName.Name);
		}

		#region remote engines
		//static readonly PeerNameResolver resolver = new PeerNameResolver();
		static readonly string name = typeof(IParallelEngine).FullName;
		//static PeerNameRecordCollection resolveCache;
		//static DateTime lastCacheFlush = DateTime.MinValue;
		//static readonly TimeSpan cacheTime = new TimeSpan(0, 5, 0);

		//static PeerNameRecordCollection GetResolveList()
		//{
		//    if (DateTime.Now - lastCacheFlush > cacheTime)
		//    {
		//        Console.Write("peer cache is outdated; refreshing...");
		//        resolveCache = resolver.Resolve(peername);
		//        if (resolveCache.Count == 0)
		//            Console.WriteLine("nothing found");
		//        else
		//            Console.WriteLine("ok");

		//        lastCacheFlush = DateTime.Now;
		//    }

		//    return resolveCache;
		//}

		internal static I CreateRemote<T, I>()
			where T: I, new()
		{
			var endpoints = Resolve(name);

			IParallelEngine engine = null;
			foreach (var endpoint in endpoints) {
				Console.Write("probing IParallelEngine at " + endpoint + "...");
				try {
					engine = Connect(endpoint);
					engine.Has("");
					Console.WriteLine("OK");
					break;
				} catch (Exception) {
					Console.WriteLine("FAIL");
					engine = null;
				}
			}
			if (engine == null) {
				Console.WriteLine("no valid endpoints found; using local object");
				return new T();
			}
			var assembly = typeof(T).Assembly;
			Console.Write("testing if remote side has required assembly...");
			if (!engine.Has(assembly.FullName))
			{
				Console.Write("no\nuploading assembly...");
				engine.Upload(File.ReadAllBytes(new Uri(assembly.CodeBase).LocalPath));
				Console.WriteLine("OK");
			} else
			{
				Console.WriteLine("yes");
			}

			var uri = typeof(T).Assembly.CodeBase;

			Console.Write("creating remote object...");
			var objUri = engine.Create(typeof(T).FullName, typeof(T).Assembly.FullName, new Uri(uri));
			Console.WriteLine("ok");

			return Connect<I>(objUri);
		}

		private static I Connect<I>(Uri objUri)
		{
			Console.Write("connecting to remote object...");
			// objUri = new Uri(objUri, typeof(I).Name);
			var endpoint = new EndpointAddress(objUri);

			var binding = new BasicHttpBinding();
			binding.UseDefaultWebProxy = useDefaultWebProxy;
			// binding.MessageEncoding = WSMessageEncoding.Mtom;
			binding.MaxReceivedMessageSize = 1024 * 1024;
			var channelFactory = new ChannelFactory<I>(binding, endpoint);
			var channel = channelFactory.CreateChannel();
			Console.WriteLine("ok");
			return channel;
		}

		private static IParallelEngine Connect(IPEndPoint hostport)
		{
#warning TODO: binding type probing
			string hostname = hostport.ToString();
			if (hostport.AddressFamily == AddressFamily.InterNetworkV6) {
				hostname = string.Format("[{0}]:{1}", hostport.Address, hostport.Port);
			}

			var endpoint = new EndpointAddress("http://" + hostname + "/" + typeof(IParallelEngine).FullName);

			var binding = new BasicHttpBinding();
			binding.UseDefaultWebProxy = useDefaultWebProxy;

			var channelFactory = new ChannelFactory<IParallelEngine>(binding, endpoint);
			return channelFactory.CreateChannel();
		}
		#endregion remote engines
	}
}
