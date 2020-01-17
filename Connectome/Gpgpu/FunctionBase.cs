using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome.Gpgpu
{
    abstract class FunctionBase : Components.GPGPU.Function.FunctionBase
    {
        #region LocalMethodInterface
        public static void WeightNormalize(int idx, Components.GPGPU.Function.ComputeParameter connectWeight, Components.GPGPU.Function.ComputeParameter axsonConnectCount, int pos)
        {
            GpuSource.Shared.WeightNormalize.WeightNormalize_cpu(idx, connectWeight, axsonConnectCount, pos);
        }
        public static Real Distance(Real x1, Real y1, Real z1, Real x2, Real y2, Real z2)
        {
            return GpuSource.Shared.Distance.Distance_cpu(x1, y1, z1, x2, y2, z2);
        }
        public static int StartPosition(int idx, Components.GPGPU.Function.ComputeParameter connectionCount)
        {
            return GpuSource.Shared.StartPosition.StartPosition_cpu(idx, connectionCount);
        }
        public static Real StepNext(Real x, Real r, Real d)
        {
            return GpuSource.Shared.StepNext.StepNext_cpu(x, r, d);
        }
        #endregion
    }
}
