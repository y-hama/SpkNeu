using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConnectomeVisualizer
{
    public partial class Form1 : Form
    {
        private double R { get; set; } = 1.1;
        private double Th { get; set; } = 0;
        private double Ph { get; set; } = 0;
        private double Cx { get; set; } = 0;
        private double Cy { get; set; } = 0;
        private double Cz { get; set; } = 1.1;


        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Connectome.Location camera = new Connectome.Location(Cx, Cy, Cz);
            Connectome.Location view = new Connectome.Location();

            DateTime start = DateTime.Now;
            var image = Connectome.Imaging.ViewImage(Math.Min(pictureBox1.Width, pictureBox1.Height), camera, view);
            double timespan = (DateTime.Now - start).TotalMilliseconds;

            Graphics g = Graphics.FromImage(image);
            var axis = Connectome.Location.GetAxis(new Connectome.Location(20, 20, 0), 20);
            g.DrawLine(Pens.Red, new PointF((float)axis[0].X, (float)axis[0].Y), new PointF((float)axis[1].X, (float)axis[1].Y));
            g.DrawLine(Pens.Green, new PointF((float)axis[0].X, (float)axis[0].Y), new PointF((float)axis[2].X, (float)axis[2].Y));
            g.DrawLine(Pens.Blue, new PointF((float)axis[0].X, (float)axis[0].Y), new PointF((float)axis[3].X, (float)axis[3].Y));
            g.DrawString("x", DefaultFont, Brushes.White, new PointF((float)axis[1].X, (float)axis[1].Y));
            g.DrawString("y", DefaultFont, Brushes.White, new PointF((float)axis[2].X, (float)axis[2].Y));
            g.DrawString("z", DefaultFont, Brushes.White, new PointF((float)axis[3].X, (float)axis[3].Y));

            pictureBox1.Image = image;
            GC.Collect();


            this.Text = Connectome.Core.StepCount.ToString() + " " + Math.Round(timespan, 0).ToString();
            label1.Text = Math.Round(Connectome.Core.StepTime, 10).ToString();
            label2.Text = Math.Round(Connectome.Core.TotalStepTime, 10).ToString();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Connectome.Core.Interval = trackBar1.Value;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double width = 0.75;
                double ex = (double)e.X / pictureBox1.Width, ey = (double)e.Y / pictureBox1.Height;
                Th = Math.Max(-width, Math.Min(width, (ex - 0.5) * 2));
                Ph = Math.Max(-width, Math.Min(width, (ey - 0.5) * 2));
                Th = 2 * Math.PI * Th;
                Ph = 2 * Math.PI * Ph;
            }
            else if (e.Button != MouseButtons.None)
            {
                Th = Ph = 0;
            }
            if (e.Button != MouseButtons.None)
            {
                Cx = R * Math.Sin(-Th) * Math.Cos(-Ph);
                Cy = R * Math.Sin(-Th) * Math.Sin(-Ph);
                Cz = R * Math.Cos(-Th);
            }
        }
    }
}
