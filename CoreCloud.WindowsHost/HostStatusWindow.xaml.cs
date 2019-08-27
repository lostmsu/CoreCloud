using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using System.IO;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel;
using System.Diagnostics;

namespace CoreCloud.WindowsHost
{
	/// <summary>
	/// Interaction logic for HostStatusWindow.xaml
	/// </summary>
	public partial class HostStatusWindow : Window
	{
		public HostStatusWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//while (!File.Exists("cloudHost.config"))
			//    SelectStartDirectory();
			try {
				var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var localConfig = Path.Combine(localAppData, @"Виктор Милованов\CoreCloud\cloudHost.config");
				ExeConfigurationFileMap configFile;

				if (Debugger.IsAttached) {
					Debug.Print("warning: using local config file");
					var path = Path.Combine(Environment.CurrentDirectory, "cloudHost.config");
					configFile = new ExeConfigurationFileMap{ ExeConfigFilename = path };
				} else {
					configFile = new ExeConfigurationFileMap { ExeConfigFilename = localConfig };
				}
			
				var config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
				var serviceModelSection = (ServiceModelSectionGroup)config.GetSectionGroup("system.serviceModel");
				var endpoints = serviceModelSection.Services.Services
					.Cast<ServiceElement>()
					.SelectMany(service => service.Endpoints.Cast<ServiceEndpointElement>())
					.Where(endpoint => endpoint.Address.Scheme == "http" || endpoint.Address.Scheme == "https");

				foreach (var endpoint in endpoints) {
					try {
						TestHttpUrl(endpoint.Address);
					} catch (Exception error) {
						this.Show(error);
					}
				}
			} catch (Exception error) {
				this.Show(error);
			}
		}

		private static void TestHttpUrl(Uri uri)
		{
			using (var serviceHost = new ServiceHost(typeof(DummyService), uri)) {
				serviceHost.Open();

				serviceHost.Close();
			}
		}

		//private void SelectStartDirectory()
		//{
		//    throw new NotImplementedException();
		//}
	}
}
