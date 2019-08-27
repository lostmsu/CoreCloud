using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Security;
using System.ServiceModel;

namespace CoreCloud
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class ExecutionEngine: IExecutionEngine
	{
		public ExecutionEngine() { }

		public const int DefaultPort = 3076;

		public void Start(string assemblyName, Uri imageSource)
		{
			var name = new AssemblyName(assemblyName);
			assemblyName = name.ToString();

			CheckAllowed(name);

			Assembly assembly = null; // AssemblyDownloader.Load(imageSource, name, true);
			throw new NotImplementedException();

			var entry = assembly.EntryPoint;
			if (entry == null)
				throw new InvalidOperationException("Assembly has no entry point");

			var exepath = entry.Module.FullyQualifiedName;
			var nobodyPassword = "N0bod!";
			var securePasswd = new SecureString();
			foreach(var chr in nobodyPassword)
				securePasswd.AppendChar(chr);
			var startInfo = new ProcessStartInfo(exepath)
			{
				UserName = "Nobody",
				Password = securePasswd,
				UseShellExecute = false,
				WorkingDirectory = Environment.CurrentDirectory,
			};
			Process.Start(startInfo);
		}

		//private static string SetExtension(string path, string extension)
		//{
		//    if (!extension.StartsWith("."))
		//        extension = '.' + extension;
		//    return path.EndsWith(extension)
		//        ? path
		//        : path + extension;
		//}

		private void CheckAllowed(AssemblyName name)
		{
			
		}
	}
}
