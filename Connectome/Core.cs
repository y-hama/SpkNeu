using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome
{
    public class Core
    {
        public static void Terminate() { CoreObject.IsTerminate = true; }

        public static double StepTime
        {
            get
            {
                return CoreObject.Field.StepTime;
            }
        }
        public static double TotalStepTime
        {
            get
            {
                return CoreObject.Field.TotalStepTime;
            }
        }
        public static long StepCount
        {
            get
            {
                return CoreObject.Field.StepCount;
            }
        }

        public static int Interval
        {
            set { CoreObject.Interval = value; }
        }


        public static void Initialize()
        {
            Components.State.SetAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            Components.State.AddSharedSourceGroup("Connectome.Gpgpu.GpuSource.Shared");
            Components.State.AddSourceGroup("Connectome.Gpgpu.Function");
            Components.State.Initialize();

            CoreObject.Field = new Connectome.Field(new Calculation.InitializeNeuron(CoreObject.NeuronCount, CoreObject.FieldArea, CoreObject.AxsonLengthDefault));
            foreach (var item in CoreObject.Receptor)
            {
                CoreObject.Field.AddReceptor(item);
            }

            CoreObject.Field.Confirm();
        }
    }
}
