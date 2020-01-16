using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome
{
    public class CellState
    {

        public Location SourceLocation { get; private set; }
        public Location ConvertedLocation { get; private set; } = new Location();

        public double Value { get; set; }
        public CellCore.IgnitionState State { get; set; }

        public double Energy { get; private set; }

        public new string ToString()
        {
            return string.Format("st:{0}, v:{1}, {2}, {3}", State, Value, SourceLocation.ToString(), ConvertedLocation.ToString());
        }

        public CellState(Location location, double value, double energy, CellCore.IgnitionState state)
        {
            SourceLocation = location;
            Value = value;
            State = state;
            Energy = energy;
        }

        public void Convert()
        {
            ConvertedLocation = Connectome.Location.GetConvertedLocation(SourceLocation);
        }
    }
}
