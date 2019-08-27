using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.DirectoryServices.AccountManagement;
using CoreCloud.HostWPF.Properties;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;

namespace CoreCloud.HostWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		#region Settings
		readonly PrincipalContext machine = new PrincipalContext(ContextType.Machine);

		private void login_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (createUser == null) return;
			
			UserPrincipal principal = UserPrincipal.FindByIdentity(machine, login.Text);
			bool exists = !string.IsNullOrEmpty(login.Text) && principal != null;

			login.Background = exists ? null : (Brush)this.Resources["errorBrush"];

			if (exists) password_PasswordChanged(this, e);

			createUser.IsEnabled = !string.IsNullOrEmpty(login.Text) && !exists
				&& !string.IsNullOrEmpty(password.Password);
		}

		private void password_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (createUser == null) return;

			createUser.IsEnabled = !string.IsNullOrEmpty(login.Text) && login.Background != null
				&& !string.IsNullOrEmpty(password.Password);
		}
		#endregion Settings

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Settings.Default.Save();
		}

		private void enableParallelEngine_Checked(object sender, RoutedEventArgs e)
		{
			bool enable = true.Equals(enableParallelEngine.IsChecked);
			parallelEngineControl.IsEnabled = false;
			enableParallelEngine.IsChecked = false;

			if (enable) StartParallelEngine();
			else StopParallelEngine();
		}

		private void StopParallelEngine()
		{
			if (!true.Equals(enableParallelEngine.IsChecked)) return;

			throw new NotImplementedException();
		}

		private void StartParallelEngine()
		{
			if (true.Equals(useCredentials.IsChecked) && login.Text != UserPrincipal.Current.Name) {
				if (MessageBox.Show(this, "CoreCloud is running under " + UserPrincipal.Current.Name + ". Restart as " + login.Text + "?",
					"Restart as another user?", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
					RestartAs();
				}
				parallelEngineControl.IsEnabled = true;
				return;
			}
		}

		private void RestartAs()
		{
			var startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location){
				LoadUserProfile = true,
				Password = password.SecurePassword,
				UserName = login.Text,
				UseShellExecute = false,
				Verb = "runas",
			};

			try {
				var proc = Process.Start(startInfo);
				Close();
			} catch (Win32Exception error) {
				MessageBox.Show(this, error.Message, "Can't start CoreCloud under " + login.Text, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			password.Password = Settings.Default.Password;
		}

		private void password_LostFocus(object sender, RoutedEventArgs e)
		{
			Settings.Default.Password = password.Password;
		}
	}
}
