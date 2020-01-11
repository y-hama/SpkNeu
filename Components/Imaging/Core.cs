using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.Imaging
{
    public class Core
    {
        private static Core _instance = new Core();
        public static Core Instance { get { return _instance; } }
        private Core()
        {

        }

        #region Property
        private Capture.CaptureBase capture { get; set; }
        #endregion

        public void Initialize(State.CaptureMode mode)
        {
            switch (mode)
            {
                case State.CaptureMode.Device_Camera:
                    capture = new Capture.Camera.Device();
                    break;
                default:
                    break;
            }
            capture.Initialize();
        }

        public RNdMatrix GetFrame(int batchsize, int channel, int width, int height)
        {
            var frames = capture.GetFrames(batchsize, channel, width, height);
            return new RNdMatrix(frames);
        }
        public RNdMatrix GetFrame(int[] shape)
        {
            if (shape.Length < 4) { throw new Exception(); }
            var frames = capture.GetFrames(shape[0], shape[1], shape[2], shape[3]);
            return new RNdMatrix(frames);
        }
        public RNdMatrix GetFrame(RNdMatrix inmat)
        {
            var frames = capture.GetFrames(inmat.BatchSize, inmat.Channels, inmat.Width, inmat.Height);
            return new RNdMatrix(frames);
        }
    }
}
