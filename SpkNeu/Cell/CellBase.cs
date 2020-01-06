using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu.Cell
{
    public abstract class CellBase
    {
        private static int idseed { get; set; } = 0;
        public int ID { get; private set; }

        protected static Random random { get; set; } = new Random();

        public Location Location { get; set; } = new Location(random);

        private double localSignal { get; set; } = random.NextDouble();
        public double LocalSignal { get { return localSignal; } }
        public double Signal { get; private set; }

        private Queue<bool> LongSignalDeviation { get; set; } = new Queue<bool>();
        public double LongTermIgnitionRatio { get; private set; } = 0;
        public double MiddleTermIgnitionRatio { get; private set; } = 0;
        public double ShortTermIgnitionRatio { get; private set; } = 0;

        public bool IsIgnition { get; private set; } = false;

        protected List<CellBase> Glial { get; set; } = new List<CellBase>();
        protected List<CellBase> Axson { get; set; } = new List<CellBase>();
        public int AxsonCount { get { return Axson.Count; } }

        public CellBase()
        {
            ID = idseed;
            idseed++;
        }

        public void Add(CellBase cell)
        {
            Glial.Add(cell);
            if (random.NextDouble() > 0.5)
            {
                Axson.Add(cell);
            }
        }
        public void Remove(CellBase cell)
        {
            int index = Axson.IndexOf(cell);
            if (index >= 0)
            {
                Disconnection(index);
                Axson.Remove(cell);
            }
        }

        public bool Ignition()
        {
            localSignal = ignition(calculateUpdateQuantity(new List<double>(Axson.Select(x => x.Signal).ToArray())), localSignal);
            if (localSignal > 1)
            {
                IsIgnition = true;
            }
            else
            {
                IsIgnition = false;
            }
            return IsIgnition;
        }

        public bool Update()
        {
            Signal = Math.Max(0, localSignal);

            //LongSignalDeviation.Enqueue(IsIgnition);
            //if (LongSignalDeviation.Count > 500)
            //{
            //    LongSignalDeviation.Dequeue();
            //}
            //double lcnt = 0;
            //double mcnt = 0;
            //double scnt = 0;
            //var items = LongSignalDeviation.ToArray();
            //for (int i = 0; i < items.Length; i++)
            //{
            //    if (items[i])
            //    {
            //        lcnt++;
            //        if (i - (500 - 100) >= 0)
            //        {
            //            mcnt++;
            //        }
            //        if (i - (500 - 25) >= 0)
            //        {
            //            scnt++;
            //        }
            //    }
            //}
            //LongTermIgnitionRatio = lcnt / LongSignalDeviation.Count;
            //MiddleTermIgnitionRatio = mcnt / 100;
            //ShortTermIgnitionRatio = scnt / 25;

            if (AxsonCount == 0)
            {
                return false;
            }
            return true;
        }

        public abstract void Connection();
        protected abstract void Disconnection(int index);

        protected abstract double calculateUpdateQuantity(List<double> signals);
        protected abstract double ignition(double updateQuantity, double localSignal);
    }
}
