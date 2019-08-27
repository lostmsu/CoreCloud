using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace RayTracer.GUI
{
	public partial class RayTracerForm : Form
	{
		Bitmap bitmap;
		PictureBox pictureBox;
		const int width = 600;
		const int height = 600;

		public RayTracerForm()
		{
			bitmap = new Bitmap(width, height);

			pictureBox = new PictureBox();
			pictureBox.Dock = DockStyle.Fill;
			pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
			pictureBox.Image = bitmap;

			ClientSize = new System.Drawing.Size(width, height + 24);
			Controls.Add(pictureBox);
			Text = "Ray Tracer";
			Load += RayTracerForm_Load;

			Show();
		}

		private void RayTracerForm_Load(object sender, EventArgs e)
		{
			this.Show();
			Tracer rayTracer = new Tracer(width, height, (int x, int y, System.Drawing.Color color) => {
																   bitmap.SetPixel(x, y, color);
																   if (x == 0) pictureBox.Refresh();
															   });
			rayTracer.LineRendered += RayTracerForm_LineRendered;
			rayTracer.Render(Tracer.DefaultScene);
			pictureBox.Invalidate();
		}

		private void RayTracerForm_LineRendered(object sender, EventArgs e)
		{
			Application.DoEvents();
		}

		public static void Run()
		{
			Application.EnableVisualStyles();

			Application.Run(new RayTracerForm());
		}
	}
}
