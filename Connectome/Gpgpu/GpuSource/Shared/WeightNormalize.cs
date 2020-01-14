using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU.Function;

namespace Connectome.Gpgpu.GpuSource.Shared
{
    class WeightNormalize : Components.GPGPU.Function.SourceCode
    {
        public override string Name
        {
            get
            {
                return @"WeightNormalize";
            }
        }

        protected override bool IsLocalFunction
        {
            get
            {
                return true;
            }
        }

        protected override ReturnType Return
        {
            get
            {
                return ReturnType.VOID;
            }
        }

        protected override void ParameterConfigration()
        {
        }

        protected override void CreateSource()
        {
        }

        public static void WeightNormalize_cpu()
        {

        }

    }
}
