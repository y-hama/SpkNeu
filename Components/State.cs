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
        }
    }
}
