using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu
{
    public class Field
    {
        protected static Random random { get; set; } = new Random();

        public delegate void IgnitionEventHandler(List<Cell.CellInfomation> infomation);
        private IgnitionEventHandler IgnitionHandler = null;
        public event IgnitionEventHandler Ignition
        {
            add { IgnitionHandler += value; }
            remove { IgnitionHandler -= value; }
        }

        private double AxsonLength { get; set; } = 0.2;

        public List<Cell.CellBase> Neurons { get; set; } = new List<Cell.CellBase>();

        public Field(Cell.CellBase typebase, int count, int timing = 5)
        {
            Type type = typebase.GetType();

            for (int i = 0; i < count; i++)
            {
                Neurons.Add((Cell.CellBase)Activator.CreateInstance(type));
            }
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (i == j) { continue; }
                    else
                    {
                        if (Neurons[i].Location.DistanceTo(Neurons[j].Location) < AxsonLength)
                        {
                            Neurons[i].Add(Neurons[j]);
                        }
                    }
                }
            }
            //List<int> rem = new List<int>();
            //for (int i = 0; i < count; i++)
            //{
            //    if (Neurons[i].AxsonCount == 0)
            //    {
            //        rem.Add(i);
            //    }
            //}
            //var tmp = new List<Cell.CellBase>(Neurons);
            //for (int i = 0; i < rem.Count; i++)
            //{
            //    Neurons.Remove(tmp[rem[i]]);
            //}
            foreach (var item in Neurons)
            {
                item.Connection();
            }

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
                    Neurons[i].Ignition();
                }
                for (int i = 0; i < Neurons.Count; i++)
                { Neurons[i].Update();
                }

                if (IgnitionHandler != null)
                {
                    IgnitionHandler?.Invoke(new List<Cell.CellInfomation>(Neurons.Select(x => new Cell.CellInfomation(x))));
                }

                double sleep = timing - (DateTime.Now - start).TotalMilliseconds;
                System.Threading.Thread.Sleep((int)Math.Max(0, sleep));
            }
        }
    }
}
