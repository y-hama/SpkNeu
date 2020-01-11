using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function.Optimization.Method
{
    class Optimizer_Adam : Optimizer
    {
        private Real[] m, v;
        private ulong t { get; set; }

        private float rho1 = 0.9f;
        private float rho2 = 0.999f;
        private float ep = 1E-8f;

        protected override void InitalizeOption(int areasize)
        {
            m = new Real[areasize];
            v = new Real[areasize];
            t = 1;
        }


        protected override void BatchParameterUpdate()
        {
            t++;
        }

        protected override void CreateGpuSource()
        {
            AddSource(new Source._cl_adam());
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
            float mu = variable.Argument[0].Value;
            float bcount = variable.Argument[1].Value;
            int lcount = variable[0].Infomation.ArrayLength;

            Real[] grad = variable[0].Instance.Array.Data;
            Real[] w = variable[1].Instance.Array.Data;
            Real[] u = variable[2].Instance.Array.Data;

            for (int i0 = 0; i0 < lcount; i0++)
            {
                float g = grad[i0] / (float)bcount;
                m[i0] = rho1 * m[i0] + (1 - rho1) * g;
                v[i0] = rho2 * v[i0] + (1 - rho2) * g * g;
                float mh = (float)(m[i0] / (1 - pow(rho1, t)));
                float vh = (float)(v[i0] / (1 - pow(rho2, t)));
                u[i0] = (mu) * ((mh) / (sqrt(vh) + ep));
                w[i0] -= u[i0];
            }
        }
        protected override void GpuFunction(ComputeVariable variable)
        {
            float mu = variable.Argument[0].Value;
            float bcount = variable.Argument[1].Value;
            int lcount = variable.Parameter[0].Infomation.ArrayLength;

            using (Cloo.ComputeBuffer<Real> grad = ConvertBuffer(variable[0].Instance))
            using (Cloo.ComputeBuffer<Real> w = ConvertBuffer(variable[1].Instance))
            using (Cloo.ComputeBuffer<Real> _m = ConvertBuffer(new ComputeParameter("m", new RNdArray(m), State.MemoryModeSet.WriteOnly)))
            using (Cloo.ComputeBuffer<Real> _v = ConvertBuffer(new ComputeParameter("v", new RNdArray(v), State.MemoryModeSet.WriteOnly)))
            using (Cloo.ComputeBuffer<Real> _u = ConvertBuffer(variable[2].Instance))
            {
                SetParameter(grad);
                SetParameter(w);
                SetParameter(_m);
                SetParameter(_v);
                SetParameter(_u);

                SetParameter(mu, ValueMode.FLOAT);
                SetParameter(bcount, ValueMode.FLOAT);
                SetParameter(ep, ValueMode.FLOAT);
                SetParameter(rho1, ValueMode.FLOAT);
                SetParameter(rho2, ValueMode.FLOAT);
                SetParameter((float)t, ValueMode.FLOAT);

                Execute(lcount);

                ReadBuffer(w, ref variable[1].Instance.Array.Data);
                ReadBuffer(_m, ref m);
                ReadBuffer(_v, ref v);
                ReadBuffer(_u, ref variable[2].Instance.Array.Data);
            }

        }
    }
}
