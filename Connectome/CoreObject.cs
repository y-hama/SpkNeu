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
        public static double FieldArea { get; set; } = 1;
        public static double AxsonLengthDefault { get; set; } = 0.12;

        public static int Interval { get; set; } = 0;

        public static Field Field { get; set; }

        public static List<Receptor.Receptor> Receptor { get; set; } = new List<Receptor.Receptor>()
        {
            new Receptor.Pulsar(59, new Location(0.9, 0, 0), 0.1, 1, 1),
            new Receptor.Pulsar(50, new Location(0, 0.9, 0), 0.1, 1, 1),
            new Receptor.NegativeContingency(50, new Location(0.7, 0.7, 0), 0.1, 2) { Contingency = 0, DutyMax = 1, SignalTerm = 1, },
            new Receptor.PositiveContingency(50, new Location(0.7, 0.7, 0), 0.1, 3) { Contingency = 0, DutyMax = 100, SignalTerm = 100, },

        };

        public static List<Location> Signal { get; set; } = new List<Location>()
        {
            new Location(-0.95, 0, 0),
            new Location(0, -0.95, 0),
            new Location(0.65, 0.65, 0),
            new Location(-0.65, -0.65, 0),
        };

        public static bool PulseON { get; set; } = true;

        public static bool GiveContingency(double val)
        {
            Field.Energy += val;
            if (Field.Energy < 0) { Field.Energy = 0; return false; }
            if (Field.Energy > CoreObject.NeuronCount) { Field.Energy = CoreObject.NeuronCount; return false; }
            return true;
        }

        public static void SetReceptorContingency(int idx, double contingency)
        {
            Receptor[idx].Contingency = contingency;
        }
    }
}
