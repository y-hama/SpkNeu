using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function.Method
{
    class ConvolutionBack_Method : FunctionBase
    {
        private Real[] dKn, dBs;

        Optimization.Optimizer k_optimizer { get; set; }
        Optimization.Optimizer b_optimizer { get; set; }

        protected override void CreateGpuSource()
        {
            //AddSource(new Source.ConvolutionForward_Source());

            k_optimizer = new Optimization.Method.Optimizer_Adam();
            b_optimizer = new Optimization.Method.Optimizer_Adam();
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
            var input = variable[0].Instance.Array.Data;
            var output = variable[1].Instance.Array.Data;
            var fwin = variable[2].Instance.Array.Data;
            var fwpout = variable[3].Instance.Array.Data;
            var fwout = variable[4].Instance.Array.Data;
            var kn = variable[5].Instance.Array.Data;
            var bias = variable[6].Instance.Array.Data;
            dKn = variable[7].Instance.Array.Data;
            dBs = variable[8].Instance.Array.Data;

            var inputparam = variable[0].Instance;
            var outputparam = variable[1].Instance;
            var kernelparam = variable[5].Instance;
            var biasparam = variable[6].Instance;

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
            int activation = (int)variable["activation"].Value;
            float alpha = variable["alpha"].Value;
            int expand = 1;
            float stride = 1;

            #region Activation
            if (activation > 0)
            {
                for (int i0 = 0; i0 < batsize; i0++)
                {
                    for (int i1 = 0; i1 < inch * iasize; i1++)
                    {
                        input[i0 * inch * iasize + i1] *= (fwpout[i0 * inch * iasize + i1] < 0) ? alpha : 1;
                    }
                }
            }
            #endregion

            #region Bias
            for (int i0 = 0; i0 < inch; i0++)
            {
                for (int i1 = 0; i1 < outch; i1++)
                {
                    for (int bcnt = 0; bcnt < batsize; bcnt++)
                    {
                        int bidx = i0 * outch + i1;

                        int ial = iw * ih;
                        for (int i = 0; i < ial; i++)
                        {
                            dBs[bidx] += input[bcnt * inch * ial + i0 * ial + i];
                        }
                    }
                }
            }
            #endregion

            #region Kernel
            for (int i0 = 0; i0 < inch; i0++)
            {
                for (int i1 = 0; i1 < outch; i1++)
                {
                    for (int i2 = 0; i2 < kasize; i2++)
                    {
                        for (int bcnt = 0; bcnt < batsize; bcnt++)
                        {
                            float _stride = 1.0f / (float)stride;
                            int s = (int)(i2 / (2 * ksize + 1));
                            int t = i2 - s * (2 * ksize + 1);
                            int _s = s - ksize, _t = t - ksize;
                            int kidx = i0 * outch * kasize + i1 * kasize + i2;
                            int _i, _ix, _iy, _idx;
                            int _ox, _oy, _odx;
                            int ial = iw * ih;
                            for (int i = 0; i < ial; i++)
                            {
                                _i = i;
                                _iy = (int)((_i) / iw);
                                _ix = _i - _iy * iw;
                                _idx = bcnt * (inch * iasize) + i0 * ial + i;

                                _ox = (int)((float)_ix * _stride) + _s * expand;
                                _oy = (int)((float)_iy * _stride) + _t * expand;
                                if (_ox >= 0 && _oy >= 0 && _ox < ow && _oy < oh)
                                {
                                    _odx = bcnt * (outch * oasize) + i1 * oasize + _oy * ow + _ox;
                                    dKn[kidx] += fwout[_odx] * input[_idx];
                                }
                            }
                        }
                    }
                }
            }
            #endregion
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
            var input = variable[0].Instance.Array.Data;
            var output = variable[1].Instance.Array.Data;
            var fwin = variable[2].Instance.Array.Data;
            var fwpout = variable[3].Instance.Array.Data;
            var fwout = variable[4].Instance.Array.Data;
            var kn = variable[5].Instance.Array.Data;
            var bias = variable[6].Instance.Array.Data;
            dKn = variable[7].Instance.Array.Data;
            dBs = variable[8].Instance.Array.Data;

            var inputparam = variable[0].Instance;
            var outputparam = variable[1].Instance;
            var kernelparam = variable[5].Instance;
            var biasparam = variable[6].Instance;

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
            int activation = (int)variable["activation"].Value;
            float alpha = variable["alpha"].Value;
            int expand = 1;
            float stride = 1;



        }

        protected override void UpdateWithCondition(bool ForceUpdate, ComputeVariable variable)
        {
            float rho = variable["rho"].Value;
            k_optimizer.Update(ref dKn, ref variable[5].Instance.Array.Data, rho, variable.Parameter[0].Instance.Array.BatchSize);
            b_optimizer.Update(ref dBs, ref variable[6].Instance.Array.Data, rho, variable.Parameter[0].Instance.Array.BatchSize);
        }
    }
}
