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

            new System.Threading.Thread(() =>
            {
                while (!CoreObject.IsTerminate)
                {
                    StepStart();
                    for (int i = 0; i < count; i++)
                    {
                        Cells[i].Signal = GetSignel(i);
                    }
                }
            }).Start();
        }

        public abstract void StepStart();
        public abstract Real GetSignel(int idx);
    }
}
