using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU.Function;
using Components;

namespace Connectome.Gpgpu.GpuSource.Shared
{
    class StepNext : Components.GPGPU.Function.SourceCode
    {
        public override string Name
        {
            get
            {
                return @"StepNext";
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
                return ReturnType.FLOAT;
            }
        }

        protected override void CreateSource()
        {
            AddParameter("x", ObjectType.Value, ElementType.FLOAT);
            AddParameter("r", ObjectType.Value, ElementType.FLOAT);
            AddParameter("d", ObjectType.Value, ElementType.FLOAT);
        }

        protected override void ParameterConfigration()
        {
            AddMethodBody(@"return r * (x + d);");
        }

        public static Real StepNext_cpu(Real x, Real r, Real d)
        {
            return r * (x + d);
        }
    }
}
