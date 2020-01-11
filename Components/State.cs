using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    public static class State
    {
        #region EventMessage
        public enum EventState
        {
            State,
            Log,
            Option01,
        }

        public class DataEventArgs
        {
            public EventState Mode { get; set; }
            public int Step { get; set; }
            public object Object { get; set; }
        }

        public delegate void UpdateDataEvent(object sender, DataEventArgs e);
        private static UpdateDataEvent UpdateMessageDataEventHandler { get; set; }
        public static event UpdateDataEvent UpdateMessageData
        {
            add { UpdateMessageDataEventHandler += value; }
            remove { UpdateMessageDataEventHandler -= value; }
        }
        public static void SendMessage(EventState state, string message)
        {
            if (UpdateMessageDataEventHandler != null)
            {
                UpdateMessageDataEventHandler(null, new DataEventArgs() { Mode = state, Object = message });
            }
        }

        private static UpdateDataEvent UpdateObjectDataEventHandler { get; set; }
        public static event UpdateDataEvent UpdateObjectData
        {
            add { UpdateObjectDataEventHandler += value; }
            remove { UpdateObjectDataEventHandler -= value; }
        }
        public static void SendData(DataEventArgs e)
        {
            if (UpdateObjectDataEventHandler != null)
            {
                UpdateObjectDataEventHandler(null, e);
            }
        }
        #endregion

        #region GPGPU Define
        public enum MemoryModeSet
        {
            ReadOnly,
            WriteOnly,
            Parameter,
        }
        public enum ActionMode
        {
            Forward,
            Back,
        }
        #endregion

        #region Imaging Define
        public enum CaptureMode
        {
            Device_Camera,
        }
        #endregion

        public static void Initialize(bool testmode = false)
        {
            GPGPU_Startup();
            if (testmode)
            {
                GPGPU_TestStart();
            }
        }

        private static void GPGPU_Startup()
        {
            GPGPU.Core.Instance.UseGPU = true;
            Terminal.WriteLine(0, GPGPU.Core.Instance.ProcesserStatus);

            GPGPU.Core.Instance.BuildAllMethod();
        }

        private static void GPGPU_TestStart()
        {
            Components.Imaging.Core.Instance.Initialize(CaptureMode.Device_Camera);

            (new System.Threading.Tasks.Task(() =>
            {
                Random rand = new Random();
                RNdMatrix inmat = new RNdMatrix(1, 3, 320, 240);
                RNdMatrix outmat = new RNdMatrix(1, 3, 320, 240);

                var unit = new GPGPU.Layer.Unit.Convolution.Convolution()
                {
                    Activation = true,
                    KernelSize = 22,
                    OutputChannels = 3,
                    Rho = 0.001,
                };
                unit.Initialize(inmat.Clone());
                unit.Confirm();

                int counter = 0;
                double time = 0;
                while (true)
                {
                    time = 0;
                    var input = Imaging.Core.Instance.GetFrame(inmat.Shape);
                    unit.Input = input.Clone();
                    time += unit.Action(ActionMode.Forward);

                    var sigma = unit.Output - input;
                    unit.Sigma = sigma;
                    //time += unit.Action(ActionMode.Back);

                    unit.Output.Show("output", 0);
                    sigma.Show("sigma", 0);
                    Terminal.WriteLine(EventState.State, "Step:{0}, TimeSpan[ms]:{1}, FPS:{2}", counter++, (int)time, (int)(1000.0 / time));
                }
            })).Start();
        }
    }
}
