using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.GPGPU.Function.Source
{
    class SpikeNeuronInit_Source : SourceCode
    {
        public override string Name => "SpikeNeuronInit_Source";

        protected override void CreateSource()
        {
            AddParameter("bias", ObjectType.Array, ElementType.FLOAT);

            AddParameter("batsize", ObjectType.Value, ElementType.INT);
        }

        protected override void ParameterConfigration()
        {
            GlobalID(3);
            AddMethodBody(@"

");
        }
    }
}
