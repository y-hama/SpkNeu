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

        private bool Initialized { get; set; } = false;
        #region PersonalProperty
        private const double StableDecayDefault = 0.5;
        private const double IgnitionInflationDefault = 1.1;
        private const double OverShootDecayStepDefault = 0.11;
        private const double OverShootDecayDefault = 0.8;
        private const double OverShootLimitDefault = 0.15;
        private const double CoolingDecayDefault = 0.9;
        private const double FluctuationDefault = 0;
        private const double AcceptanceDefault = 0.25;
        private double StableDecay { get; set; } = StableDecayDefault;
        private double IgnitionInflation { get; set; } = IgnitionInflationDefault;
        private double OverShootDecayStep { get; set; } = OverShootDecayStepDefault;
        private double OverShootDecay { get; set; } = OverShootDecayDefault;
        private double OverShootLimit { get; set; } = OverShootLimitDefault;
        private double CoolingDecay { get; set; } = CoolingDecayDefault;
        private double Fluctuation { get; set; } = FluctuationDefault;
        private double Acceptance { get; set; } = AcceptanceDefault;
        #endregion

        private List<double> prevAxonSignal { get; set; } = null;

        public override void Connection(double defaultvalue = 0)
        {
            if (!Initialized)
            {
                double fq = 0.005;
                StableDecay = Math.Max(0, StableDecayDefault + (random.NextDouble() * 2 - 1) * fq);
                IgnitionInflation = Math.Max(0, IgnitionInflationDefault + (random.NextDouble() * 2 - 1) * fq);
                OverShootDecayStep = Math.Max(0, OverShootDecayStepDefault + (random.NextDouble() * 2 - 1) * fq);
                OverShootDecay = Math.Max(0, OverShootDecayDefault + (random.NextDouble() * 2 - 1) * fq);
                OverShootLimit = Math.Max(0, OverShootLimitDefault + (random.NextDouble() * 2 - 1) * fq);
                CoolingDecay = Math.Max(0, CoolingDecayDefault + (random.NextDouble() * 2 - 1) * fq);
                Fluctuation = Math.Max(0, FluctuationDefault + (random.NextDouble() * 2 - 1) * fq);
                Acceptance = Math.Max(0, AcceptanceDefault + (random.NextDouble() * 2 - 1) * fq);

                state = State.Cooling;
                localSignal = -OverShootLimit;
                Initialized = true;
            }
            if (prevAxonSignal == null)
            {
                prevAxonSignal = new List<double>(new double[AxsonCount]);
            }
            else
            {
                prevAxonSignal.Add(defaultvalue);
            }
        }

        protected override void Disconnection(int index)
        {
            prevAxonSignal.Remove(prevAxonSignal[index]);
        }

        protected override double calculateUpdateQuantity(List<double> signals, List<bool> isreceotor)
        {
            double res = 0;
            int reccount = isreceotor.Count(x => x);
            for (int i = 0; i < signals.Count; i++)
            {
                if (isreceotor[i])
                {
                    res +=  signals[i];
                }
                else
                {
                    res += prevAxonSignal[i] - signals[i];
                    prevAxonSignal[i] = signals[i];
                }
            }
            if (signals.Count > 0)
            {
                res /= Math.Max(1, reccount);
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
                        ret = Math.Max(0, (localSignal + updateQuantity + (random.NextDouble()) * Fluctuation) * StableDecay);
                        if (ret > 1)
                        {
                            state = State.Ignition;
                        }
                        else
                        {
                            JoinUpdate();
                        }
                    }
                    break;
                case State.Ignition:
                    {
                        ret = (localSignal) * IgnitionInflation;
                        if (ret > 2)
                        {
                            state = State.OverShoot;
                        }
                    }
                    break;
                case State.OverShoot:
                    {
                        ret = (localSignal - OverShootDecayStep) * OverShootDecay;
                        if (ret < -OverShootLimit)
                        {
                            state = State.Cooling;
                        }
                    }
                    break;
                case State.Cooling:
                    {
                        ret = 1E-3 + (localSignal) * CoolingDecay;
                        if (ret > 0)
                        {
                            ret = 0;
                            state = State.Stable;
                        }
                    }
                    break;
                default:
                    break;
            }
            return ret;
        }

        private void JoinUpdate()
        {
            List<CellCore> rem = new List<CellCore>();
            foreach (var item in Axson)
            {
                if (item is CellBase)
                {
                    if (item.LongTermIgnitionRatio <= this.MiddleTermIgnitionRatio)
                    {
                        rem.Add(item);
                    }
                }
                else if (item is Receptor)
                {
                    Receptor receptor = item as Receptor;
                    if ((receptor.SignalUpdateAmount > 0) && (receptor.SignalUpdateAmount - this.ShortTermIgnitionRatio) < 1E-3)
                    {
                        rem.Add(item);
                    }
                }
            }
            foreach (var item in rem)
            {
                double probabirity = 1E-6;
                Remove(item, probabirity);
            }

            var target = Glial.FindAll(x => !Axson.Contains(x));
            foreach (var item in target)
            {
                double probabirity = 1;
                if (item is CellBase)
                {
                    if (item.LongTermIgnitionRatio > this.LongTermIgnitionRatio)
                    {
                        probabirity = 1E-2;
                        if (this.ShortTermIgnitionRatio > item.ShortTermIgnitionRatio)
                        {
                            probabirity = 1E-2;
                        }
                        Add(item, probabirity);
                    }
                }
                else if (item is Receptor)
                {
                    Receptor receptor = item as Receptor;
                    if ((receptor.SignalUpdateAmount) > Acceptance)
                    {
                        probabirity = 1E-2;
                        Add(item, probabirity);
                    }
                }
            }
        }
    }
}
