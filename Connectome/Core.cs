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

        public static bool PulseON
        {
            get { return CoreObject.PulseON; }
            set { CoreObject.PulseON = value; }
        }

        public static void GiveContingency()
        {
            CoreObject.GiveContingency(CoreObject.Field.Energy);
        }

        public static void SetReceptorContingency(int idx, double contingency)
        {
            CoreObject.SetReceptorContingency(idx, contingency);
        }


        public static event SignalUpdateEventHandler SignalUpdate
        {
            add { CoreObject.Field.SignalUpdateEvent += value; }
            remove { CoreObject.Field.SignalUpdateEvent -= value; }
        }

        public static void Initialize()
        {
            Components.State.SetAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            Components.State.AddSharedSourceGroup("Connectome.Gpgpu.GpuSource.Shared");
            Components.State.AddSourceGroup("Connectome.Gpgpu.Function");
            Components.State.Initialize();

            CoreObject.Field = new Connectome.Field(new Calculation.InitializeNeuron(CoreObject.NeuronCount, CoreObject.FieldArea, CoreObject.AxsonLengthDefault));
            CoreObject.CreateReceptor();
            foreach (var item in CoreObject.Receptor)
            {
                CoreObject.Field.AddReceptor(item);
            }
            foreach (var item in CoreObject.Signal)
            {
                CoreObject.Field.SetSignalLocation(item, CoreObject.AxsonLengthDefault);
            }

            CoreObject.Field.Confirm();
        }
    }
}
