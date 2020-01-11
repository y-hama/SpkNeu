using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function.Method
{
    class Convolution : FunctionBase
    {
        protected override void CreateGpuSource()
        {
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
            throw new NotImplementedException();
        }
    }
}
