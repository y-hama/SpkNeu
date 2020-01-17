using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Components;

namespace Connectome.Receptor
{
    abstract class Receptor
    {
        public double Contingency { get; set; } = 1;

        protected Location CenterLocation { get; private set; }

        public List<Cell> Cells { get; private set; } = new List<Cell>();

        public double Dispersion { get; private set; }

        protected Receptor(int count, Location center, double dispersion)
        {
            var random = new Random();
            CenterLocation = center;
            Dispersion = dispersion;

            for (int i = 0; i < count; i++)
            {
                Cells.Add(new Cell(new Location(random, dispersion) + center));
            }
        }

        public void Start()
        {
            new System.Threading.Thread(() =>
            {
                while (!CoreObject.IsTerminate)
                {
                    StepStart();
                    for (int i = 0; i < Cells.Count; i++)
                    {
                        Cells[i].Signal = Cells[i].Value= GetSignel(i);
                        Cells[i].Activity = 1;
                    }
                    StepEnd();
                    System.Threading.Thread.Sleep(1 + CoreObject.Interval);
                }
            }).Start();
        }

        public abstract void StepStart();
        public abstract void StepEnd();
        public abstract Real GetSignel(int idx);
    }
}
