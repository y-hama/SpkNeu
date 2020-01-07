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
                for (int i = 0; i < Neurons.Count; i++)
                {
                    Neurons[i].Ignition(random.NextDouble() * 2);
                }
                for (int i = 0; i < Neurons.Count; i++)
                {
                    Neurons[i].Update();
                }

                double sleep = timing - (DateTime.Now - start).TotalMilliseconds;
                System.Threading.Thread.Sleep((int)Math.Max(0, sleep));
            }
        }
    }
}
