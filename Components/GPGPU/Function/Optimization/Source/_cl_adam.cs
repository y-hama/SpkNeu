using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function.Optimization.Source
{
    class _cl_adam : SourceCode
    {
        public override string Name
        {
            get { return @"Optimization_Adam"; }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("grad", ObjectType.Array, ElementType.FLOAT);
            AddParameter("w", ObjectType.Array, ElementType.FLOAT);
            AddParameter("m", ObjectType.Array, ElementType.FLOAT);
            AddParameter("v", ObjectType.Array, ElementType.FLOAT);
            AddParameter("u", ObjectType.Array, ElementType.FLOAT);

            AddParameter("mu", ObjectType.Value, ElementType.FLOAT);
            AddParameter("bcount", ObjectType.Value, ElementType.FLOAT);
            AddParameter("ep", ObjectType.Value, ElementType.FLOAT);
            AddParameter("rho1", ObjectType.Value, ElementType.FLOAT);
            AddParameter("rho2", ObjectType.Value, ElementType.FLOAT);
            AddParameter("t", ObjectType.Value, ElementType.FLOAT);
        }

        protected override void CreateSource()
        {
            GlobalID(1);
            AddMethodBody(@"
                float g = grad[i0] / (float)bcount;
                m[i0] = rho1 * m[i0] + (1 - rho1) * g;
                v[i0] = rho2 * v[i0] + (1 - rho2) * g * g;
                float mh = (float)(m[i0] / (1 - pow(rho1, t)));
                float vh = (float)(v[i0] / (1 - pow(rho2, t)));
                u[i0] = (mu) * ((mh) / (sqrt(vh) + ep));
                w[i0] -= u[i0];
            ");
        }
    }
}
