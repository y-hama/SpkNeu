using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome.Gpgpu.GpuSource.Method
{
    class WeightInitialize_Source : Components.GPGPU.Function.SourceCode
    {
        public override string Name
        {
            get
            {
                return @"WeightInitialize_Source";
            }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("connectWeight", ObjectType.Array, ElementType.FLOAT);
            AddParameter("axsonConnectCount", ObjectType.Array, ElementType.FLOAT);
        }

        protected override void CreateSource()
        {
            GlobalID(1);
            AddMethodBody(@"
WeightNormalize(i0, connectWeight, axsonConnectCount);
");
        }
    }
}
