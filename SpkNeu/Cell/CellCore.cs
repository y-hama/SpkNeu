using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu.Cell
{
    public abstract class CellCore
    {
        protected static int idseed { get; set; } = 0;
        public int ID { get; protected set; }

        protected static Random random { get; set; } = new Random();
        protected static bool Probabirity(double expect)
        {
            return random.NextDouble() < expect;
        }

        public Location Location { get; set; }

        protected double localSignal { get; set; }
        public double LocalSignal { get { return localSignal; } }
        public double Signal { get; protected set; }

        protected const int TermQueueCount = 2500;
        protected Queue<bool> LongSignalDeviation { get; set; } = new Queue<bool>(new bool[TermQueueCount]);
        public double LongTermIgnitionRatio { get; protected set; } = 0;
        public double MiddleTermIgnitionRatio { get; protected set; } = 0;
        public double ShortTermIgnitionRatio { get; protected set; } = 0;

        public double SignalUpdateAmount { get; set; } = 0;

        protected void CellBaseTermIgnitionRatio(bool IsIgnition)
        {
            LongSignalDeviation.Enqueue(IsIgnition);
            if (LongSignalDeviation.Count > TermQueueCount)
            {
                LongSignalDeviation.Dequeue();
            }
            double lcnt = 0;
            double mcnt = 0;
            double scnt = 0;
            var items = LongSignalDeviation.ToArray();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i])
                {
                    lcnt++;
                    if (i - (TermQueueCount * 4 / 5) >= 0)
                    {
                        mcnt++;
                    }
                    if (i - (TermQueueCount * 24 / 25) >= 0)
                    {
                        scnt++;
                    }
                }
            }
            LongTermIgnitionRatio = lcnt / LongSignalDeviation.Count;
            MiddleTermIgnitionRatio = mcnt / (TermQueueCount / 5);
            ShortTermIgnitionRatio = scnt / (TermQueueCount / 25);
            if (scnt > 0)
            { scnt++; }
        }

        protected void ReceptorTermIgnitionRatio(double signal, double update)
        {
            double diff = (signal - update) / 2;
            SignalUpdateAmount = 0.5 * SignalUpdateAmount + (1 - 0.5) * diff * diff;
        }
    }
}
