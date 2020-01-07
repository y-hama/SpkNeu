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

        private double FieldSize { get; set; } = 0.75;
        private double AxsonLength { get; set; } = 0.15;

        public List<Cell.CellBase> Neurons { get; set; } = new List<Cell.CellBase>();
        private List<int> SortID_Nearest_to_Center { get; set; } = new List<int>();
        private int ConnectedID { get; set; } = 0;

        public Field(Cell.CellBase typebase, int count)
        {
            Type type = typebase.GetType();

            for (int i = 0; i < count; i++)
            {
                var cell = (Cell.CellBase)Activator.CreateInstance(type);
                cell.SetLocation(FieldSize);
                Neurons.Add(cell);
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
            var tmp = new Cell.CellBase[Neurons.Count];
            Neurons.CopyTo(tmp);
            var ntmp = new List<Cell.CellBase>(tmp);
            ntmp.Sort();
            //ntmp.Reverse();
            for (int i = 0; i < ntmp.Count; i++)
            {
                SortID_Nearest_to_Center.Add(ntmp[i].ID);
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
                    Neurons[i].Ignition();
                }
                for (int i = 0; i < Neurons.Count; i++)
                {
                    Neurons[i].Update();
                }

                if (IgnitionHandler != null)
                {
                    IgnitionHandler?.Invoke(new List<Cell.CellInfomation>(Neurons.Select(x => new Cell.CellInfomation(x))));
                }

                double sleep = timing - (DateTime.Now - start).TotalMilliseconds;
                System.Threading.Thread.Sleep((int)Math.Max(0, sleep));
            }
        }

        public void SetReceotor(SensoryField receptor)
        {
            for (int i = 0; i < receptor.Neurons.Count; i++)
            {
                var cell = Neurons.Find(x => x.ID == SortID_Nearest_to_Center[i + ConnectedID]);
                cell.Add(receptor.Neurons[i], 1);
            }
            ConnectedID += receptor.Neurons.Count;
        }
    }
}
