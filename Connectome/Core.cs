using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome
{
    public class Core
    {
        public static bool IsTerminate { get; private set; } = false;
        public void Terminate() { IsTerminate = true; }

        public static int NeuronCount { get; set; } = 1000;
        public static double FieldArea { get; set; } = 1;
        public static double AxsonLengthDefault { get; set; } = 0.25;

        private static Field Field { get; set; }

        public static void Initialize()
        {
            Components.State.SetAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            Components.State.AddSharedSourceGroup("Connectome.Gpgpu.GpuSource.Shared");
            Components.State.AddSourceGroup("Connectome.Gpgpu.Function");
            Components.State.Initialize();

            var noize = new Receptor.RandomNoize(100, new Location(), 0.1);

            Field = new Connectome.Field(new Calculation.InitializeNeuron(NeuronCount, FieldArea, AxsonLengthDefault));
            Field.AddReceptor(noize);

            Field.Confirm();
        }
    }
}
