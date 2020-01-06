using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu.Cell
{
    public class VirtualNeuron : CellBase
    {
        private List<double> weight { get; set; } = new List<double>();
        private List<double> prevAxonSignal { get; set; } = new List<double>();

        private void WeightNormalize()
        {
            double sum = weight.Sum() * AxsonCount;
            for (int i = 0; i < weight.Count; i++)
            {
                weight[i] /= sum;
            }
        }

        public override void Connection()
        {
            foreach (var item in Axson)
            {
                weight.Add(random.NextDouble());
                prevAxonSignal.Add(0);
            }
            WeightNormalize();
        }

        protected override void Disconnection(int index)
        {

        }

        protected override double calculateUpdateQuantity(List<double> signals)
        {
            double res = 0;
            for (int i = 0; i < AxsonCount; i++)
            {
                res += signals[i] * weight[i];
            }

            for (int i = 0; i < AxsonCount; i++)
            {
                double da = prevAxonSignal[i] - signals[i];
                if (da < 0)
                {
                    if (!IsIgnition)
                    {
                        weight[i] *= 0.999;
                    }
                }
                else if (da > 0)
                {
                    weight[i] *= 1.001;
                }
                prevAxonSignal[i] = signals[i];
            }
            WeightNormalize();
            return res;
        }

        protected override double ignition(double updateQuantity, double localSignal)
        {
            if (localSignal > 1)
            { return 0; }
            else
            { return localSignal + updateQuantity; }
        }
    }
}
