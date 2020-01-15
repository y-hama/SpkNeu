using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome
{
    static class CoreObject
    {
        public static bool IsTerminate { get; set; } = false;

        public static int NeuronCount { get; set; } = 5000;
        public static double FieldArea { get; set; } = 0.75;
        public static double AxsonLengthDefault { get; set; } = 0.15;

        public static int Interval { get; set; } = 0;

        public static Field Field { get; set; }

        public static List<Receptor.Receptor> Receptor { get; set; } = new List<Receptor.Receptor>()
        {
            new Receptor.RandomNoize(10, new Location(0.65, 0, 0), 0.001), 
        };
    }
}
