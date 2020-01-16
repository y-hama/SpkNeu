using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome.Receptor
{
    class PositiveContingency : Receptor
    {
        private int SignalIndex { get; set; }
        private bool NeedResponce { get; set; } = false;
        private double Signal { get; set; } = 0;

        public int DutyMax { get; set; } = 1000;
        public int SignalTerm { get; set; } = 1000;

        private int Counter { get; set; }
        private double signalCounter { get; set; }

        public PositiveContingency(int count, Location center, double dispersion, int signalIndex) : base(count, center, dispersion)
        {
            SignalIndex = signalIndex;
        }

        public override void StepStart()
        {
            double n = (signalCounter) / (signalCounter + 1);
            Signal = n * Signal + (1 - n) * CoreObject.Field.Signal(SignalIndex);
            signalCounter++; if (signalCounter > 10) { signalCounter = 0; }
            if (Signal > 0.25 && !NeedResponce)
            {
                NeedResponce = true;
                Counter = DutyMax;
            }
        }

        public override Real GetSignel(int idx)
        {
            if (NeedResponce)
            {
                if (Counter > SignalTerm)
                {
                    return 0;
                }
                return Contingency;
            }
            else { return 0; }
        }

        public override void StepEnd()
        {
            if (Counter < 0)
            {
                NeedResponce = false;
            }
            else
            {
                CoreObject.GiveContingency(2 * (1 - CoreObject.Field.Energy / CoreObject.NeuronCount));
                Counter--;
            }
        }
    }
}
