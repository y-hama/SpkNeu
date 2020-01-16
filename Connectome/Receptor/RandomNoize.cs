using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome.Receptor
{
    class RandomNoize : Receptor
    {
        private Random random { get; set; } = new Random();
        public RandomNoize(int Count, Location Center, double dispersion) : base(Count, Center, dispersion)
        {

        }

        public override void StepStart()
        {
        }

        public override Real GetSignel(int idx)
        {
            return Contingency * random.NextDouble();
        }

        public override void StepEnd()
        {
        }
    }
}
