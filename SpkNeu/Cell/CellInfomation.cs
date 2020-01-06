using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu.Cell
{
    public class CellInfomation : IComparable
    {
        public int ID { get; private set; }
        public Location Location { get; private set; }
        public double Signal { get; private set; }
        public double LocalSignal { get; private set; }

        public bool IsIgnition { get; private set; }

        public CellInfomation(CellBase cell)
        {
            ID = cell.ID;
            Location = cell.Location;
            Signal = cell.Signal;
            LocalSignal = cell.LocalSignal;
            IsIgnition = cell.IsIgnition;
        }

        public CellInfomation(Location loc, CellInfomation cellinfo)
        {
            ID = cellinfo.ID;
            Location = loc;
            Signal = cellinfo.Signal;
            LocalSignal = cellinfo.LocalSignal;
            IsIgnition = cellinfo.IsIgnition;
        }

        public int CompareTo(object obj)
        {
            CellInfomation ci = obj as CellInfomation;
            if (ci.Location.Z > Location.Z)
            {
                return -1;
            }
            else if (ci.Location.Z < Location.Z)
            {
                return 1;
            }
            return 0;
        }
    }
}
