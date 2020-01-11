using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function.Method
{
    class Convolution_Method : FunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new Source.Convolution_Source());
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
            var input = variable[0].Instance.Array.Data;
            var output = variable[1].Instance.Array.Data;
            var kn = variable[2].Instance.Array.Data;
            var bias = variable[3].Instance.Array.Data;

            var inputparam = variable[0].Instance;
            var outputparam = variable[1].Instance;
            var kernelparam = variable[2].Instance;

            int batsize = inputparam.Array.BatchSize;
            int inch = inputparam.Array.Channels;
            int outch = outputparam.Array.Channels;
            int iw = inputparam.Array.Width;
            int ih = inputparam.Array.Height;
            int ow = outputparam.Array.Width;
            int oh = outputparam.Array.Height;
            int iasize = inputparam.Array.AreaSize;
            int oasize = outputparam.Array.AreaSize;
            int kasize = kernelparam.Array.AreaSize;
            int ksize = (int)variable["kernelsize"].Value;

            for (int i0 = 0; i0 < batsize; i0++)
            {
                for (int i1 = 0; i1 < outch; i1++)
                {
                    for (int i2 = 0; i2 < oasize; i2++)
                    {
                        output[i0 * i1 * oasize + i1 * oasize + i2] = 0;
                        float val = 0;
                        for (int c = 0; c < inch; c++)
                        {
                            int y = (int)(i2 / ow);
                            int x = (int)(i2 - y * ow);
                            int ix, iy, idx, kidx;
                            for (int s = -ksize; s <= ksize; s++)
                            {
                                for (int t = -ksize; t <= ksize; t++)
                                {
                                    ix = x + s; iy = y + t;
                                    if (ix >= 0 && ix < iw && iy >= 0 && iy < ih)
                                    {
                                        idx = i0 * c * iasize + c * iasize + iy * iw + ix;
                                        kidx = c * outch * kasize + i1 * kasize + (t + ksize) * (2 * ksize + 1) + (s + ksize);
                                        val += input[idx] * kn[kidx];
                                    }
                                }
                            }
                            val += bias[i1 * inch + c];
                        }
                        output[i0 * i1 * oasize + i1 * oasize + i2] = val;
                    }
                }
            }

        }

        protected override void GpuFunction(ComputeVariable variable)
        {
            var inputparam = variable[0].Instance;
            var outputparam = variable[1].Instance;
            var kernelparam = variable[2].Instance;

            int batsize = inputparam.Array.BatchSize;
            int inch = inputparam.Array.Channels;
            int outch = outputparam.Array.Channels;
            int iw = inputparam.Array.Width;
            int ih = inputparam.Array.Height;
            int ow = outputparam.Array.Width;
            int oh = outputparam.Array.Height;
            int iasize = inputparam.Array.AreaSize;
            int oasize = outputparam.Array.AreaSize;
            int kasize = kernelparam.Array.AreaSize;
            int ksize = (int)variable["kernelsize"].Value;

            using (Cloo.ComputeBuffer<Real> _input = ConvertBuffer(variable[0].Instance))
            using (Cloo.ComputeBuffer<Real> _output = ConvertBuffer(variable[1].Instance))
            using (Cloo.ComputeBuffer<Real> _kernel = ConvertBuffer(variable[2].Instance))
            using (Cloo.ComputeBuffer<Real> _bias = ConvertBuffer(variable[3].Instance))
            {
                SetParameter(_input);
                SetParameter(_output);
                SetParameter(_kernel);
                SetParameter(_bias);
                SetParameter(batsize, ValueMode.INT);
                SetParameter(inch, ValueMode.INT);
                SetParameter(outch, ValueMode.INT);
                SetParameter(iw, ValueMode.INT);
                SetParameter(ih, ValueMode.INT);
                SetParameter(ow, ValueMode.INT);
                SetParameter(oh, ValueMode.INT);
                SetParameter(iasize, ValueMode.INT);
                SetParameter(oasize, ValueMode.INT);
                SetParameter(kasize, ValueMode.INT);
                SetParameter(ksize, ValueMode.INT);
                Execute(batsize, outch, oasize);
                ReadBuffer(_output, ref variable[1].Instance.Array.Data);
            }

        }
    }
}
