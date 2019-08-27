using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace System.Windows
{
	public static class WindowExceptionHandlers
	{
		public static void Show(this Window win, Exception error)
		{
			MessageBox.Show(win, error.Message, error.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
