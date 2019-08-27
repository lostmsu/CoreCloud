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

using Color = RayTracer.Material.Color;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RayTracer.GUI
{
	/// <summary>
	/// Interaction logic for RayTracerWindow.xaml
	/// </summary>
	public partial class RayTracerWindow : Window
	{
		public RayTracerWindow()
		{
			InitializeComponent();
		}

		const int width = 1024;
		const int height = 1024;

		WriteableBitmap buffer = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
		Int32Rect rect = new Int32Rect { X = 0, Y = 0, Width = width, Height = height };
		Color[,] renderBuffer = new Color[width * 2, height * 2];

		public static void Run()
		{
			var window = new RayTracerWindow();
			window.Show();
			System.Windows.Threading.Dispatcher.Run();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			System.Windows.Threading.Dispatcher.ExitAllFrames();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var timer = Stopwatch.StartNew();
			if (Debugger.IsAttached)
				Tracer.Render(Tracer.DefaultScene, renderBuffer, (Action<int>)(progress => Title = progress * 50 / height + "%" ));
			else
				Parallel.For(0, height * 2, (int y) => Tracer.Render(Tracer.DefaultScene, renderBuffer, y, 6));
			this.Title = string.Format("{0}ms", timer.ElapsedMilliseconds);
			timer.Stop();

			var pixels = new byte[height, width * 4];
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++) {
					var color = 0.25*(renderBuffer[y * 2, x * 2] + renderBuffer[y * 2, x * 2 + 1]
						+ renderBuffer[y * 2 + 1, x * 2] + renderBuffer[y * 2 + 1, x * 2 + 1]);
					pixels[y, x * 4] = (byte)(255 * color.B);
					pixels[y, x * 4 + 1] = (byte)(255 * color.G);
					pixels[y, x * 4 + 2] = (byte)(255 * color.R);
				}

			buffer.WritePixels(rect, pixels, width * 4, 0, 0);

			image.Source = buffer;
		}
	}
}
