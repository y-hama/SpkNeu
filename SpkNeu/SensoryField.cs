using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu
{
    public class SensoryField
    {
        protected static Random random { get; set; } = new Random();
        public List<Cell.Receptor> Neurons { get; set; } = new List<Cell.Receptor>();

        public SensoryField(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var cell = new Cell.Receptor();
                cell.SetVirtualLocation();
                Neurons.Add(cell);
            }
        }

        public void Start(int timing = 5)
        {
            new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(transmission)).Start(timing);
        }

        private void transmission(object arg)
        {
            int timing = (int)arg;
            while (!Core.IsTerminate)
            {
                if (Core.IsPause)
                {
                    System.Threading.Thread.Sleep(timing);
                    continue;
                }
                List<Cell.CellBase> IgnitionCells = new List<Cell.CellBase>();
                var start = DateTime.Now;
#if !DEBUG
                Parallel.For(0, Neurons.Count, i =>
                {
                    //double value = 1;
                    double value = 0.5 + random.NextDouble();
                    Neurons[i].Ignition(value);
                });
                Parallel.For(0, Neurons.Count, i => { Neurons[i].Update(); });
#else
                for (int i = 0; i < Neurons.Count; i++)
                {
                    //double value = 1;
                    double value = 0.5 + random.NextDouble();
                    Neurons[i].Ignition(value);
                }
                for (int i = 0; i < Neurons.Count; i++)
                {
                    Neurons[i].Update();
                }
#endif 

                double sleep = timing - (DateTime.Now - start).TotalMilliseconds;
                System.Threading.Thread.Sleep((int)Math.Max(0, sleep));
            }
        }
    }
}
