using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu.Cell
{
    public class Receptor : CellCore, IComparable
    {
        protected double IgnitionLimit { get; set; } = 0.5;

        public void SetVirtualLocation(double area = 1)
        {
            Location = new Location(random, area);
        }

        public void Ignition(double value)
        {
            localSignal = value;
        }
        public bool Update()
        {
            ReceptorTermIgnitionRatio(Signal, localSignal);
            Signal = localSignal;

            return true;
        }

        public int CompareTo(object obj)
        {
            CellBase cell = obj as CellBase;
            double l1 = Location.DistanceTo(new Location());
            double l2 = cell.Location.DistanceTo(new Location());
            if (l1 > l2)
            {
                return 1;
            }
            else if (l2 > l1)
            {
                return -1;
            }
            return 0;
        }
    }
}
