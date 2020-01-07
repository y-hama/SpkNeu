using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu
{
    public class Location
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public new string ToString()
        {
            return string.Format("X:{0}, Y:{1}, Z:{2}, D:{3}", X, Y, Z, DistanceTo(new SpkNeu.Location()));
        }

        public void Copy(Location location)
        {
            X = location.X;
            Y = location.Y;
            Z = location.Z;
        }

        public Location(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }
        public Location(Random random = null, double area = 1)
        {
            if (random != null)
            {
                bool check = false;
                while (!check)
                {
                    X = area * (random.NextDouble() * 2 - 1);
                    Y = area * (random.NextDouble() * 2 - 1);
                    Z = area * (random.NextDouble() * 2 - 1);
                    if (DistanceTo(new Location()) < area)
                    {
                        check = true;
                    }
                }
            }
        }

        public double DistanceTo(Location loc, double? border = null)
        {
            double dx = X - loc.X;
            double dy = Y - loc.Y;
            double dz = Z - loc.Z;
            double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (border != null)
            {
                while (dist > border)
                {
                    dist -= border.Value;
                }
            }
            return dist;
        }

        public double Norm
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        private void Normalize()
        {
            double norm = Norm;
            if (norm != 0)
            {
                X /= norm; Y /= norm; Z /= norm;
            }
        }

        public static List<Location> GetAxis(Location offset, double size)
        {
            Location eo = new Location(0, 0, 0);
            Location ex = new Location(1, 0, 0);
            Location ey = new Location(0, 1, 0);
            Location ez = new Location(0, 0, 1);
            eo = size * GetConvertedLocation(eo) + offset;
            ex = size * GetConvertedLocation(ex) + offset;
            ey = size * GetConvertedLocation(ey) + offset;
            ez = size * GetConvertedLocation(ez) + offset;
            return new List<Location>(new Location[] { eo, ex, ey, ez });
        }

        private static Location Cross(Location l1, Location l2)
        {
            return new Location(l1.Y * l2.Z - l1.Z * l2.Y, l1.Z * l2.X - l1.X * l2.Z, l1.X * l2.Y - l1.Y * l2.X);
        }

        private static double Dot(Location l1, Location l2)
        {
            return l1.X * l2.X + l1.Y * l2.Y + l1.Z * l2.Z;
        }

        private static double[,] WorldMatrix { get; set; } = new double[4, 4];
        public static void SetWorldMatrix(Location camera, Location view)
        {
            Location res = new Location();
            Location cam_z = (view - camera);
            cam_z.Normalize();
            Location cam_x = Cross(new Location() { X = 0, Y = 1, Z = 0 }, cam_z);
            cam_x.Normalize();
            Location cam_y = Cross(cam_z, cam_x);

            WorldMatrix[0, 0] = cam_x.X;
            WorldMatrix[1, 0] = cam_x.Y;
            WorldMatrix[2, 0] = cam_x.Z;
            WorldMatrix[3, 0] = 0;
            WorldMatrix[0, 1] = cam_y.X;
            WorldMatrix[1, 1] = cam_y.Y;
            WorldMatrix[2, 1] = cam_y.Z;
            WorldMatrix[3, 1] = 0;
            WorldMatrix[0, 2] = cam_z.X;
            WorldMatrix[1, 2] = cam_z.Y;
            WorldMatrix[2, 2] = cam_z.Z;
            WorldMatrix[3, 2] = 0;
            WorldMatrix[0, 3] = -Dot(camera, cam_x);
            WorldMatrix[1, 3] = -Dot(camera, cam_y);
            WorldMatrix[2, 3] = -Dot(camera, cam_z);
            WorldMatrix[3, 3] = 1;
        }

        public static Location GetConvertedLocation(Location pt)
        {
            double[] vec = new double[] { pt.X, pt.Y, pt.Z, 1 };
            double[] res = new double[4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    res[i] += WorldMatrix[i, j] * vec[j];
                }
            }
            return new Location(res[0], res[1], res[2]);
        }

        public static Location operator +(Location l1, Location l2)
        {
            return new Location(l1.X + l2.X, l1.Y + l2.Y, l1.Z + l2.Z);
        }
        public static Location operator -(Location l1, Location l2)
        {
            return new Location(l1.X - l2.X, l1.Y - l2.Y, l1.Z - l2.Z);
        }
        public static Location operator *(double scale, Location l2)
        {
            return new Location(scale * l2.X, scale * l2.Y, scale * l2.Z);
        }
    }
}
