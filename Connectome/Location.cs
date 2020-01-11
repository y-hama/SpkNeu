using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome
{
    public class Location
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public new string ToString()
        {
            return string.Format("X:{0}, Y:{1}, Z:{2}, D:{3}", X, Y, Z, DistanceTo(new Location()));
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

        public Location Normalize()
        {
            double norm = Norm;
            if (norm != 0)
            {
                X /= norm; Y /= norm; Z /= norm;
            }
            return this;
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

        private static double[,] ViewMatrix { get; set; } = new double[4, 4];
        private static double[,] ProjectionMatrix { get; set; } = new double[4, 4];
        private static double[,] WorldMatrix { get; set; } = new double[4, 4];
        public static void SetWorldMatrix(Location camera, Location view, double angle = Math.PI / 2, double near = 0.9, double far = 1)
        {
            Location res = new Location();
            Location cam_z = (view - camera);
            cam_z.Normalize();
            Location cam_x = Cross(new Location() { X = 0, Y = 1, Z = 0 }, cam_z);
            cam_x.Normalize();
            Location cam_y = Cross(cam_z, cam_x);

            ViewMatrix[0, 0] = cam_x.X;
            ViewMatrix[0, 1] = cam_y.X;
            ViewMatrix[0, 2] = cam_z.X;
            ViewMatrix[0, 3] = -Dot(camera, cam_x);
            ViewMatrix[1, 0] = cam_x.Y;
            ViewMatrix[1, 1] = cam_y.Y;
            ViewMatrix[1, 2] = cam_z.Y;
            ViewMatrix[1, 3] = -Dot(camera, cam_y);
            ViewMatrix[2, 0] = cam_x.Z;
            ViewMatrix[2, 1] = cam_y.Z;
            ViewMatrix[2, 2] = cam_z.Z;
            ViewMatrix[2, 3] = -Dot(camera, cam_z);
            ViewMatrix[3, 0] = 0;
            ViewMatrix[3, 1] = 0;
            ViewMatrix[3, 2] = 0;
            ViewMatrix[3, 3] = 1;

            ProjectionMatrix[0, 0] = 1;
            ProjectionMatrix[0, 1] = 0;
            ProjectionMatrix[0, 2] = 0;
            ProjectionMatrix[0, 3] = 0;
            ProjectionMatrix[1, 0] = 0;
            ProjectionMatrix[1, 1] = 1;
            ProjectionMatrix[1, 2] = 0;
            ProjectionMatrix[1, 3] = 0;
            ProjectionMatrix[2, 0] = 0;
            ProjectionMatrix[2, 1] = 0;
            ProjectionMatrix[2, 2] = 1;
            ProjectionMatrix[2, 3] = 0;
            ProjectionMatrix[3, 0] = 0;
            ProjectionMatrix[3, 1] = 0;
            ProjectionMatrix[3, 2] = 0;
            ProjectionMatrix[3, 3] = 1;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    WorldMatrix[i, j] = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        WorldMatrix[i, j] += ProjectionMatrix[i, k] * ViewMatrix[k, j];
                    }
                }
            }
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
            return new Location(res[0] / vec[3], res[1] / vec[3], res[2] / vec[3]);
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
