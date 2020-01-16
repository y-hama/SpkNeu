using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome
{
    public class FieldState
    {
        public double Energy { get; set; }
        public List<double> Signals { get; set; }
        public List<Location> Locations { get; set; }
    }
}
