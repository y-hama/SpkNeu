using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Connectome
{
    public static class Imaging
    {

        public static System.Drawing.Bitmap
            ViewImage(int size, Location camera, Location view)
        {
            Location.SetWorldMatrix(camera, view);

            var cells = CoreObject.Field.GetState();
            Parallel.For(0, cells.Count, i =>
            {
                cells[i].Convert();
            });
            cells.Sort((c1, c2) =>
            {
                if (c1.ConvertedLocation.Z > c2.ConvertedLocation.Z)
                {
                    return -1;
                }
                else if (c1.ConvertedLocation.Z < c2.ConvertedLocation.Z)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            });
            List<double> z_order = new List<double>(cells.Select(x => x.ConvertedLocation.Z).ToArray());
            double near = z_order.Min();
            double far = z_order.Max();
            double areasize = far == near ? 1 : far - near;

            Bitmap bitmap = new Bitmap(size, size);
            Graphics g = Graphics.FromImage(bitmap);
            g.FillRectangle(Brushes.Black, new RectangleF(0, 0, size, size));
            double sizeoder = Math.Max(15, size / 30), sizemin = 0.75;
            double viewparticledistance = (view - camera).Norm;
            for (int i = 0; i < cells.Count; i++)
            {
                var item = cells[i];
                if (item.ConvertedLocation.Z > 0)
                {
                    double signal = -1;
                    Color fc = Color.Black;
                    Color ec = Color.Black;
                    RectangleF rect = new RectangleF();
                    int x = (int)(((item.ConvertedLocation.X + 1) / 2) * size);
                    int y = (int)(((item.ConvertedLocation.Y + 1) / 2) * size);
                    double itemorder = (item.ConvertedLocation.Z - near) / (areasize);
                    double zodr = (1 - itemorder);

                    float elemsize = (float)(sizeoder * ((1 - sizemin) * zodr + sizemin));
                    signal = (Math.Max(0, Math.Min(1, item.Value)));
                    byte sigv = (byte)(byte.MaxValue * signal);

                    fc = Color.FromArgb((byte)(byte.MaxValue * (sigv * 0.2 + 0.8) / 2), sigv / 4, sigv / 2, sigv);
                    rect = new RectangleF(x - elemsize / 2, y - elemsize / 2, elemsize, elemsize);

                    ec = Color.Gray;
                    switch (item.State)
                    {
                        case CellCore.IgnitionState.Stable:
                            ec = Color.FromArgb(100, Color.DimGray);
                            break;
                        case CellCore.IgnitionState.Ignition:
                            ec = Color.FromArgb(byte.MaxValue, Color.Red);
                            break;
                        case CellCore.IgnitionState.Overshoot:
                            ec = Color.FromArgb(128, Color.YellowGreen);
                            break;
                        case CellCore.IgnitionState.Cooling:
                            ec = Color.FromArgb(96, Color.Cyan);
                            break;
                        default:
                            break;
                    }
                    g.FillEllipse(new SolidBrush(fc), rect);
                    g.DrawEllipse(new Pen(ec), rect);
                }
            }

            return bitmap;
        }
    }
}
