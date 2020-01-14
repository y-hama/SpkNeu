using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome.Calculation
{
    class InitializeNeuron
    {
        public class NeuronSource
        {
            public Location Location { get; set; }
            public double AxsonLength { get; private set; }
            public new string ToString()
            {
                return Location.ToString() + ", " + string.Format("ax{0}", AxsonLength);
            }

            public NeuronSource(Location loc, double length)
            {
                Location = loc;
                AxsonLength = length;
            }
        }

        public List<NeuronSource> NeuronSources { get; private set; } = new List<NeuronSource>();

        public InitializeNeuron(int count, double area, double axsonLengthBase)
        {
            var random = new Random();
            var list = new List<Location>();
            Components.RNdArray px = new Components.RNdArray(count);
            Components.RNdArray py = new Components.RNdArray(count);
            Components.RNdArray pz = new Components.RNdArray(count);
            Components.RNdArray axson = new Components.RNdArray(count);
            for (int i = 0; i < count; i++)
            {
                var loc = new Location(random, area);
                list.Add(loc);
                px[i] = loc.X;
                py[i] = loc.Y;
                pz[i] = loc.Z;
                axson[i] = (axsonLengthBase) + (axsonLengthBase / 10.0) * (random.NextDouble() * 2 - 1);
            }

            Components.RNdArray hasRef = new Components.RNdArray(count);
            bool check = false;
            var function = new Connectome.Gpgpu.Function.LocationInitializeStep();
            function.FunctionConfiguration();
            int repetitioncount = 0;
            while (!check)
            {
                var variable = new Components.GPGPU.ComputeVariable();
                variable.Add("px", px, Components.State.MemoryModeSet.ReadOnly);
                variable.Add("py", py, Components.State.MemoryModeSet.ReadOnly);
                variable.Add("pz", pz, Components.State.MemoryModeSet.ReadOnly);
                variable.Add("paxson", axson, Components.State.MemoryModeSet.ReadOnly);
                variable.Add("phasRef", hasRef, Components.State.MemoryModeSet.WriteOnly);
                variable.Argument.Add(new Components.GPGPU.ComputeVariable.ValueSet("count") { Value = count });
                function.Do(false, variable);
                int containscount = hasRef.Data.Count(x => x.Value == 0);
                if (containscount != 0)
                {
                    repetitioncount++;
                    for (int i = 0; i < count; i++)
                    {
                        if (hasRef.Data[i] == 0)
                        {
                            var loc = new Location(random, area);
                            list[i] = loc;
                            px[i] = loc.X;
                            py[i] = loc.Y;
                            pz[i] = loc.Z;
                        }
                    }
                }
                else
                {
                    check = true;
                }
            }

            for (int i = 0; i < count; i++)
            {
                NeuronSources.Add(new NeuronSource(list[i], axson[i]));
            }

            NeuronSources.Sort((l1, l2) =>
            {
                var def = new Location();
                var dist1 = l1.Location.DistanceTo(def);
                var dist2 = l2.Location.DistanceTo(def);
                if (dist1 >= dist2)
                {
                    return 1;
                }
                else if (dist1 <= dist2)
                {
                    return -1;
                }
                else { return 0; }
            });
        }
    }
}
