using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu.Cell
{
    public class SpikeNeuron : CellBase
    {
        private int IDStub { get { return ID; } }
        private enum State
        {
            Stable,
            Ignition,
            OverShoot,
            Cooling,
        }
        private State state { get; set; } = State.Stable;

        private double OverShootLimit { get; set; }

        private List<double> prevAxonSignal { get; set; } = new List<double>();

        public override void Connection()
        {
            OverShootLimit = random.NextDouble() * 0.2 + 0.05;
            prevAxonSignal = new List<double>(new double[AxsonCount]);
        }

        protected override void Disconnection(int index)
        {
        }

        protected override double calculateUpdateQuantity(List<double> signals)
        {
            double res = 0;
            for (int i = 0; i < signals.Count; i++)
            {
                res += prevAxonSignal[i] - signals[i];
                prevAxonSignal[i] = signals[i];
            }

            return Math.Max(0, res);
        }

        protected override double ignition(double updateQuantity, double localSignal)
        {
            double ret = localSignal;
            switch (state)
            {
                case State.Stable:
                    {
                        double rho = 0.55;
                        ret = (rho * localSignal + (1 - rho) * updateQuantity) + (random.NextDouble() * 2 - 1) / 100;
                        if (ret > 1)
                        {
                            state = State.Ignition;
                        }
                    }
                    break;
                case State.Ignition:
                    {
                        ret = (localSignal) * 1.25;
                        if (ret > 2)
                        {
                            state = State.OverShoot;
                        }
                    }
                    break;
                case State.OverShoot:
                    {
                        ret = (localSignal - 0.1) * 0.75;
                        if (ret < -OverShootLimit)
                        {
                            state = State.Cooling;
                        }
                    }
                    break;
                case State.Cooling:
                    {
                        ret = 1E-3 + (localSignal) * 0.95;
                        if (ret > 0)
                        {
                            state = State.Stable;
                        }
                    }
                    break;
                default:
                    break;
            }
            return ret;
        }
    }
}
