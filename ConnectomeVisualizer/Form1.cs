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
        private void CalcCameraPos()
        {
            Cx = R * Math.Sin(-Th) * Math.Cos(-Ph);
            Cy = R * Math.Sin(-Th) * Math.Sin(-Ph);
            Cz = R * Math.Cos(-Th);
        }

        private object ___qlock = new object();
        private Queue<Connectome.FieldState> FieldStateHistoryTemporary { get; set; } = new Queue<Connectome.FieldState>();
        private Queue<Connectome.FieldState> FieldStateHistory { get; set; } = new Queue<Connectome.FieldState>();

        public Form1()
        {
            InitializeComponent();
            Connectome.Core.SetReceptorContingency(0, 0.5);
            Connectome.Core.SetReceptorContingency(1, 0.5);
            Connectome.Core.SignalUpdate += Core_SignalUpdate;
        }

        private void Core_SignalUpdate(Connectome.FieldState state)
        {
            lock (___qlock)
            {
                FieldStateHistoryTemporary.Enqueue(state);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Connectome.Location camera = new Connectome.Location(Cx, Cy, Cz);
            Connectome.Location view = new Connectome.Location();

            DateTime start = DateTime.Now;
            double timespan = 0;
            if (checkBox2.Checked)
            {
                var image = Connectome.Imaging.ViewImage(Math.Min(pictureBox1.Width, pictureBox1.Height), camera, view);

                Graphics g = Graphics.FromImage(image);
                var axis = Connectome.Location.GetAxis(new Connectome.Location(20, 20, 0), 20);
                g.DrawLine(Pens.Red, new PointF((float)axis[0].X, (float)axis[0].Y), new PointF((float)axis[1].X, (float)axis[1].Y));
                g.DrawLine(Pens.Green, new PointF((float)axis[0].X, (float)axis[0].Y), new PointF((float)axis[2].X, (float)axis[2].Y));
                g.DrawLine(Pens.Blue, new PointF((float)axis[0].X, (float)axis[0].Y), new PointF((float)axis[3].X, (float)axis[3].Y));
                g.DrawString("x", DefaultFont, Brushes.White, new PointF((float)axis[1].X, (float)axis[1].Y));
                g.DrawString("y", DefaultFont, Brushes.White, new PointF((float)axis[2].X, (float)axis[2].Y));
                g.DrawString("z", DefaultFont, Brushes.White, new PointF((float)axis[3].X, (float)axis[3].Y));
                pictureBox1.Image = image;
            }

            Queue<Connectome.FieldState> temporary = new Queue<Connectome.FieldState>();
            lock (___qlock)
            {
                temporary = new Queue<Connectome.FieldState>(FieldStateHistoryTemporary.ToArray());
                FieldStateHistoryTemporary.Clear();
            }
            foreach (var item in temporary)
            {
                FieldStateHistory.Enqueue(item);
            }
            if (FieldStateHistory.Count > 1000)
            {
                int cnt = FieldStateHistory.Count - 1000;
                for (int i = 0; i < cnt; i++)
                {
                    FieldStateHistory.Dequeue();
                }
            }
            if (checkBox1.Checked)
            {
                var ccnt = FieldStateHistory.ToArray()[0].Signals.Count;
                if (ccnt != chart1.Series.Count)
                {
                    chart1.Series.Clear();
                    for (int i = 0; i < ccnt; i++)
                    {
                        System.Windows.Forms.DataVisualization.Charting.Series series
                            = new System.Windows.Forms.DataVisualization.Charting.Series(FieldStateHistory.ToArray()[0].Locations[i].ToString());
                        series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                        chart1.Series.Add(series);
                    }
                }
                for (int i = 0; i < ccnt; i++)
                {
                    chart1.Series[i].Points.Clear();
                }
                chart2.Series[0].Points.Clear();
                foreach (var item in FieldStateHistory)
                {
                    for (int i = 0; i < item.Signals.Count; i++)
                    {
                        chart1.Series[i].Points.AddY(item.Signals[i]);
                    }
                    chart2.Series[0].Points.AddY(item.Energy);
                }
            }

            timespan = (DateTime.Now - start).TotalMilliseconds;
            this.Text = Connectome.Core.StepCount.ToString() + " " + Math.Round(timespan, 0).ToString();
            label1.Text = Math.Round(Connectome.Core.StepTime, 10).ToString();
            label2.Text = Math.Round(Connectome.Core.TotalStepTime, 10).ToString();

            GC.Collect();
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
                CalcCameraPos();
            }
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            R = 1 - ((double)trackBar4.Value / trackBar4.Maximum);
            CalcCameraPos();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            Th = 2 * Math.PI * ((double)trackBar2.Value - trackBar2.Maximum / 2) / (trackBar2.Maximum / 2);
            CalcCameraPos();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            Ph = 2 * Math.PI * ((double)trackBar3.Value - trackBar3.Maximum / 2) / (trackBar3.Maximum / 2);
            CalcCameraPos();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connectome.Core.PulseON = !Connectome.Core.PulseON;
            button1.Text = "Pulse:" + (Connectome.Core.PulseON ? "ON" : "OFF");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Connectome.Core.GiveContingency();
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            var x = (((double)trackBar5.Value - trackBar5.Maximum / 2) / (trackBar5.Maximum / 2)) / 2;
            Connectome.Core.SetReceptorContingency(0, 0.5 - x);
            Connectome.Core.SetReceptorContingency(1, 0.5 + x);
        }
    }
}
