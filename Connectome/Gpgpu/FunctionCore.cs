using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome.Gpgpu
{
    public class FunctionCore
    {
        #region LocalMethodInterface
        public static void WeightNormalize(int idx, Real[] connectWeight, Real[] axsonConnectCount)
        {
            GpuSource.Shared.WeightNormalize.WeightNormalize_cpu(idx, connectWeight, axsonConnectCount);
        }
        public static Real Distance(Real x1, Real y1, Real z1, Real x2, Real y2, Real z2)
        {
            return GpuSource.Shared.Distance.Distance_cpu(x1, y1, z1, x2, y2, z2);
        }
        public static int StartPosition(int idx, Real[] connectionCount)
        {
            return GpuSource.Shared.StartPosition.StartPosition_cpu(idx, connectionCount);
        }
        #endregion
    }
}
