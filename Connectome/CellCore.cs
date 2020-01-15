using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome
{
    public abstract class CellCore
    {
        private static int idseed { get; set; } = 0;
        public int ID { get; private set; } = -1;

        public enum IgnitionState
        {
            Stable,
            Ignition,
            Overshoot,
            Cooling,
        }

        public Real Signal { get; set; }

        public Location Location { get; protected set; }

        public new string ToString()
        {
            return string.Format("id:{0}, {1}, sg:{2}", ID, Location.ToString(), Signal);
        }

        public CellCore(Location loc)
        {
            ID = idseed;
            idseed++;
            Location = loc;
        }
    }
}
