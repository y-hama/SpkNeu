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

        private int NCount { get; set; } = 2500;
        private int timemax { get; set; } = 400;

        private double Cx { get; set; } = 0;
        private double Cy { get; set; } = 0;

        public MainForm()
        {
            InitializeComponent();

            chart1.ChartAreas[0].AxisY.Maximum = NCount;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = timemax;
            chart1.ChartAreas[0].AxisX.Minimum = 0;

            SpkNeu.Core.FieldTest(new SpkNeu.Cell.SpikeNeuron(), NCount, 1, IgnitionEventHandler);
        }

        private void IgnitionEventHandler(List<SpkNeu.Cell.CellInfomation> infomation)
        {
            lock (___lockobj)
            {
                IgnitionSequence.Enqueue(infomation);
                HeadCellSignal.Enqueue(SpkNeu.Core.HeadCell.LocalSignal);
                if (IgnitionSequence.Count > timemax)
                {
                    IgnitionSequence.Dequeue();
                    HeadCellSignal.Dequeue();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Queue<List<SpkNeu.Cell.CellInfomation>> igseq;
            List<SpkNeu.Cell.CellInfomation> latest;
            Queue<double> hdcl;
            lock (___lockobj)
            {
                igseq = new Queue<List<SpkNeu.Cell.CellInfomation>>(IgnitionSequence);
                hdcl = new Queue<double>(HeadCellSignal);
                latest = IgnitionSequence.Peek();
            }
            pictureBox1.Image = DrawMap(pictureBox1.Width, latest);

            GC.Collect();
        }

        private void DrawGraph(Queue<List<SpkNeu.Cell.CellInfomation>> igseq, Queue<double> hdcl)
        {
            int step = 0;
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            foreach (var inf in igseq)
            {
                foreach (var item in inf)
                {
                    if (item.IsIgnition)
                    {
                        chart1.Series[0].Points.AddXY(step, item.ID);
                    }
                }
                chart1.Series[0].Points.AddXY(step, chart1.ChartAreas[0].AxisY.Maximum);
                step++;
            }
            step = 0;
            foreach (var item in hdcl)
            {
                chart1.Series[1].Points.AddXY(step, item);
            }
        }

        private Bitmap DrawMap(int size, List<SpkNeu.Cell.CellInfomation> infomation)
        {
            Bitmap bitmap = new Bitmap(size, size);
            Graphics g = Graphics.FromImage(bitmap);
            g.FillRectangle(Brushes.Black, new Rectangle(new Point(), bitmap.Size));

            var info = new List<SpkNeu.Cell.CellInfomation>();
            var camera = new SpkNeu.Location() { X = Cx, Y = Cy, Z = 2 };
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
            info.Sort();
            info.Reverse();
            foreach (var item in info)
            {
                int x = (int)(((item.Location.X + 1) / 2) * size);
                int y = (int)(((item.Location.Y + 1) / 2) * size);
                double zodr = 1 - (item.Location.Z - near) / (far - near);
                byte b = (byte)(byte.MaxValue * zodr * (Math.Max(Math.Min(1, (item.LocalSignal)), 0)));
                Color c = Color.FromArgb(0, b, b);
                int es = (int)(10 * zodr);
                g.FillEllipse(new SolidBrush(c), new RectangleF(new PointF(x - es / 2, y - es / 2), new SizeF(es, es)));
                Pen p = new Pen(Color.FromArgb(100, Color.Gray), 1);
                if (item.IsIgnition)
                {
                    p = new Pen(Color.FromArgb((byte)(byte.MaxValue * zodr), Color.Red), 2);
                }
                g.DrawEllipse(p, new RectangleF(new PointF(x - es / 2, y - es / 2), new SizeF(es, es)));
            }

            return bitmap;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Cx = (double)(e.X - pictureBox1.Width / 2) / (pictureBox1.Width / 2);
                Cy = (double)(e.Y - pictureBox1.Height / 2) / (pictureBox1.Height / 2);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Cx = Cy = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SpkNeu.Core.Pause();
        }
    }
}
