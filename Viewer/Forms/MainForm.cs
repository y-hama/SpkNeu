using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Viewer.Forms
{
    public partial class MainForm : Form
    {
        private object ___lockobj = new object();
        private Queue<List<SpkNeu.Cell.CellInfomation>> IgnitionSequence { get; set; } = new Queue<List<SpkNeu.Cell.CellInfomation>>();
        private Queue<double> HeadCellSignal { get; set; } = new Queue<double>();

        private int NCount { get; set; } = 2000;

        private double R { get; set; } = 2;
        private double Cx { get; set; } = 0;
        private double Cy { get; set; } = 0;
        private double Cz { get; set; } = 2;

        private int IgnitionEventLocalCount { get; set; } = 0;
        private int IgnitionEventCount { get; set; } = 0;
        private double fps { get; set; } = 0;

        public MainForm()
        {
            InitializeComponent();
            pictureBox1.MouseWheel += pictureBox1_MouseWheel;

            SpkNeu.Core.FieldTest(new SpkNeu.Cell.SpikeNeuron(), NCount, 1, IgnitionEventHandler);
            pictureBox1_MouseMove(null, new MouseEventArgs(MouseButtons.Right, 5, pictureBox1.Width / 2, pictureBox1.Height / 2, 0));

        }

        private void IgnitionEventHandler(List<SpkNeu.Cell.CellInfomation> infomation)
        {
            lock (___lockobj)
            {
                IgnitionSequence.Enqueue(infomation);
                HeadCellSignal.Enqueue(SpkNeu.Core.HeadCell.LocalSignal);
                if (IgnitionSequence.Count > 2)
                {
                    IgnitionSequence.Dequeue();
                    HeadCellSignal.Dequeue();
                }
            }
            IgnitionEventCount++;
            IgnitionEventLocalCount++;
        }

        private DateTime start { get; set; } = DateTime.Now;
        private void timer1_Tick(object sender, EventArgs e)
        {
            double rho = 0.95;
            fps = rho * fps + (1 - rho) * (Math.Max(1, IgnitionEventLocalCount) / ((DateTime.Now - start).TotalMilliseconds / 1000));
            this.Text = IgnitionEventCount.ToString() + " " + Math.Round(fps, 2).ToString() + " " + Math.Round((IgnitionEventCount / (fps)), 0).ToString();
            IgnitionEventLocalCount = 0;
            start = DateTime.Now;
            List<SpkNeu.Cell.CellInfomation> latest = null;
            lock (___lockobj)
            {
                if (IgnitionSequence.Count > 0)
                {
                    latest = IgnitionSequence.Peek();
                }
            }
            if (latest != null)
            {
                Bitmap map = DrawMap(Math.Min(pictureBox1.Width, pictureBox1.Height), latest);
                if (map != null) { pictureBox1.Image = map; }
                GC.Collect();
            }
        }

        private Bitmap DrawMap(int size, List<SpkNeu.Cell.CellInfomation> infomation)
        {
            if (size <= 0) { return null; }
            Bitmap bitmap = new Bitmap(size, size);
            Graphics g = Graphics.FromImage(bitmap);
            g.FillRectangle(Brushes.Black, new Rectangle(new Point(), bitmap.Size));

            var info = new List<SpkNeu.Cell.CellInfomation>();
            var camera = new SpkNeu.Location(Cx, Cy, Cz);
            var view = new SpkNeu.Location();
            SpkNeu.Location.SetWorldMatrix(camera, view);

            List<double> z_order = new List<double>();
            foreach (var item in infomation)
            {
                var loc = SpkNeu.Location.GetConvertedLocation(item.Location);
                info.Add(new SpkNeu.Cell.CellInfomation(loc, item));
                z_order.Add(loc.Z);
            }
            double near = z_order.Min();
            double far = z_order.Max();
            double areasize = far == near ? 1 : far - near;
            info.Sort();
            info.Reverse();
            double sizeoder = Math.Max(20, size / 15);
            foreach (var item in info)
            {
                if (item.Location.Z >= 0)
                {
                    int x = (int)(((item.Location.X + 1) / 2) * size);
                    int y = (int)(((item.Location.Y + 1) / 2) * size);
                    double zodr = (1 - (item.Location.Z - near) / (areasize)) * 0.5 + 0.5;
                    byte b = (byte)(byte.MaxValue * zodr * (Math.Max(Math.Min(1, (item.LocalSignal)), 0)));
                    Color c = Color.FromArgb((byte)(byte.MaxValue * zodr * 1 / 2), 0, b, b);
                    if (item.IsIgnition)
                    {
                        c = Color.FromArgb(byte.MaxValue, 0, b, b);
                    }
                    int es = (int)(sizeoder * zodr);
                    g.FillEllipse(new SolidBrush(c), new RectangleF(new PointF(x - es / 2, y - es / 2), new SizeF(es, es)));
                    Pen p = new Pen(Color.FromArgb(100, Color.Gray), 1);
                    if (item.IsIgnition)
                    {
                        p = new Pen(Color.FromArgb((byte)(byte.MaxValue * zodr), Color.Red), 1.5f);
                    }
                    g.DrawEllipse(p, new RectangleF(new PointF(x - es / 2 + 0.5f, y - es / 2 + 0.5f), new SizeF(es - 1, es - 1)));
                    Font font = new Font(DefaultFont.Name, (float)(DefaultFont.Size * zodr));
                    g.DrawString(item.AxsonCount.ToString(), font, new SolidBrush(p.Color), new PointF((float)x - DefaultFont.Size / 2, (float)y - DefaultFont.Size / 2));
                }
            }

            var axis = SpkNeu.Location.GetAxis(new SpkNeu.Location(20, 20, 0), 20);
            g.DrawLine(Pens.Red, new PointF((float)axis[0].X, (float)axis[0].Y), new PointF((float)axis[1].X, (float)axis[1].Y));
            g.DrawLine(Pens.Green, new PointF((float)axis[0].X, (float)axis[0].Y), new PointF((float)axis[2].X, (float)axis[2].Y));
            g.DrawLine(Pens.Blue, new PointF((float)axis[0].X, (float)axis[0].Y), new PointF((float)axis[3].X, (float)axis[3].Y));
            g.DrawString("x", DefaultFont, Brushes.White, new PointF((float)axis[1].X, (float)axis[1].Y));
            g.DrawString("y", DefaultFont, Brushes.White, new PointF((float)axis[2].X, (float)axis[2].Y));
            g.DrawString("z", DefaultFont, Brushes.White, new PointF((float)axis[3].X, (float)axis[3].Y));

            return bitmap;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            double th = 0, ph = 0;
            if (e.Button == MouseButtons.Left)
            {
                double width = 0.75;
                double ex = (double)e.X / pictureBox1.Width, ey = (double)e.Y / pictureBox1.Height;
                th = Math.Max(-width, Math.Min(width, (ex - 0.5) * 2));
                ph = Math.Max(-width, Math.Min(width, (ey - 0.5) * 2));
                th = 2 * Math.PI * th;
                ph = 2 * Math.PI * ph;
            }
            if (e.Button != MouseButtons.None)
            {
                Cx = R * Math.Sin(th) * Math.Cos(-ph);
                Cy = R * Math.Sin(th) * Math.Sin(-ph);
                Cz = R * Math.Cos(th);
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                R += 0.1;
            }
            else
            {
                R -= 0.1;
                if (R < 0.1) { R = 0.1; }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SpkNeu.Core.Pause();
        }
    }
}
