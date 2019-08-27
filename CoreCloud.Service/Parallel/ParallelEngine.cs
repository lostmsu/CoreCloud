using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Security;
using System.Net;
using System.Security.Permissions;

namespace CoreCloud
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class ParallelEngine: IParallelEngine, IDisposable
	{
		private Dictionary<string, byte[]> assemblyCache = new Dictionary<string, byte[]>();
		private MD5 hasher = MD5.Create();
		private Dictionary<Guid, AppDomain> started = new Dictionary<Guid, AppDomain>();
		private readonly string rootDirectory = Environment.CurrentDirectory;

		#region IParallelEngine Members

		string root = "/ParallelEngine";

		static AppDomain CreateSandbox(string name, string uri, string basedir) {
			var evidence = new Evidence();
			evidence.AddHostEvidence(new Zone(System.Security.SecurityZone.Internet));

			var permissions = SecurityManager.GetStandardSandbox(evidence);

			var uriperm = new WebPermission(NetworkAccess.Accept, uri);
			permissions.AddPermission(uriperm);
			
			var fileperm = new FileIOPermission(FileIOPermissionAccess.AllAccess, basedir);
			var serviceAssembly = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
			fileperm.AddPathList(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, serviceAssembly);
			permissions.AddPermission(fileperm);

			var setup = new AppDomainSetup();
			setup.PrivateBinPath = basedir;
			setup.ApplicationBase = Path.GetDirectoryName(serviceAssembly);

			var result = AppDomain.CreateDomain(name, null, setup, permissions);
			var aname = Assembly.GetExecutingAssembly().GetName();
			result.Load(aname);
			return result;
		}

		public const int DefaultPort = 4079;

		string RootUri(string scheme, string hostname) {
			return scheme + "://" + hostname +":" + DefaultPort + root + "/";
		}

		public Uri Create(string typeName, string assemblyName, Uri imageSource) {
			#region loading assembly
			if (!assemblyCache.ContainsKey(assemblyName)) {
				if (imageSource != null) throw new NotImplementedException("imageSource");
				throw new TypeLoadException();
			}
			#endregion loading assembly

			Guid guid = Guid.NewGuid();
			var dir = Path.Combine(rootDirectory, guid.ToString());
			Directory.CreateDirectory(dir);
			lock (started)
				started.Add(guid, null);

			var name = new AssemblyName(assemblyName);
			var path = Path.Combine(dir, name.Name + ".dll");
			File.WriteAllBytes(path, assemblyCache[assemblyName]);

			var scheme = OperationContext.Current.IncomingMessageHeaders.To.Scheme;
			var permUri = RootUri(scheme, "+") + typeName + "/" + guid;
			var uri = new Uri(RootUri(scheme, OperationContext.Current.IncomingMessageHeaders.To.Host) + typeName + "/" + guid);
			var domainName = path + "\n" + typeName + "\n" + uri;

			var sandbox = CreateSandbox(domainName, permUri, dir);
			lock (started)
				started[guid] = sandbox;

			sandbox.DoCallBack(new CrossAppDomainDelegate(Hoster.HostFromSandbox));

			return uri;
		}

		public static class Hoster{
			public static void HostFromSandbox() {
				var parameters = AppDomain.CurrentDomain.FriendlyName.Split('\n');
				string taskPath = parameters[0], taskTypeName = parameters[1];
				var taskUri = new Uri(parameters[2]);
				var assembly = Assembly.LoadFile(taskPath);
				var t = assembly.GetType(taskTypeName);
				var obj = Activator.CreateInstance(t);

				var host = new ServiceHost(obj, taskUri);
				//foreach (var endpoint in host.AddDefaultEndpoints())
				//    (endpoint.Binding as BasicHttpBinding).MessageEncoding = WSMessageEncoding.Mtom;
				host.Open();
			}
		}

		public bool Has(string assemblyName)
		{
			return assemblyCache.ContainsKey(assemblyName);
		}

		public void Upload(byte[] assembly)
		{
			byte[] localCopy = (byte[])assembly.Clone();
			string hash = Convert.ToBase64String(hasher.ComputeHash(localCopy));
			if (assemblyCache.ContainsKey(hash)) return;
			assemblyCache[hash] = localCopy;
			try
			{
				var refl = Assembly.ReflectionOnlyLoad(assembly);

				var name = refl.GetName();
				if (assemblyCache.ContainsKey(name.ToString()))
					return;

				assemblyCache[name.ToString()] = localCopy;
				return;
			} catch(Exception error)
			{
				
			}
		}
		#endregion

		public void Dispose()
		{
			if (rootDirectory == null) return;
			foreach (var pair in started) {
				AppDomain.Unload(pair.Value);
				Directory.Delete(Path.Combine(rootDirectory, pair.Key.ToString()), true);
			}
		}
	}
}
