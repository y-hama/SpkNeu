using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome.Receptor
{
    class Pulsar : Receptor
    {
        private int DutyMax { get; set; }
        private int SignalTerm { get; set; }

        private int Counter { get; set; }

        public Pulsar(int count, Location center, double dispersion, int dutymax, int signal) : base(count, center, dispersion)
        {
            DutyMax = dutymax;
            SignalTerm = signal;
            Counter = DutyMax;
        }

        public override void StepStart()
        {
        }

        public override Real GetSignel(int idx)
        {
            if (Counter > SignalTerm)
            {
                return 0;
            }
            else if (CoreObject.PulseON)
            {
                return Contingency;
            }
            return 0;
        }

        public override void StepEnd()
        {
            Counter--;
            if (Counter < 0)
            {
                Counter = DutyMax;
            }
        }

    }
}
