using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.Imaging.Capture
{
    abstract class CaptureBase
    {
        public abstract void Initialize();

        public abstract OpenCvSharp.Mat[] GetFrames(int batchsize, int channel, int width, int height);
    }
}
