using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenCvSharp;

namespace Components.Imaging.Capture.Camera
{
    class Device : CaptureBase
    {
        private VideoCapture capture { get; set; }

        public override void Initialize()
        {
            capture = new VideoCapture(0);
            if (!capture.IsOpened()) { throw new Exception(); }

        }

        public override Mat[] GetFrames(int batchsize, int channel, int width, int height)
        {
            Size size = new Size(width, height);
            var frames = new Mat[batchsize];
            for (int b = 0; b < batchsize; b++)
            {
                frames[b] = new Mat();
                capture.Read(frames[b]);
                if (size != frames[b].Size())
                {
                    Cv2.Resize(frames[b], frames[b], size);
                }
            }
            return frames;
        }
    }
}
