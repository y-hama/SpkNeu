using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function.Source
{
    class MA_Source : SourceCode
    {
        public override string Name
        {
            get { return @"MA_Source"; }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("input", ObjectType.Array, ElementType.FLOAT);
            AddParameter("output", ObjectType.Array, ElementType.FLOAT);
            AddParameter("window", ObjectType.Array, ElementType.FLOAT);

            AddParameter("len", ObjectType.Value, ElementType.INT);
            AddParameter("area", ObjectType.Value, ElementType.INT);
        }

        protected override void CreateSource()
        {
            GlobalID(1);
            AddMethodBody(@"
                int start;
                int min, max;
                float arv = 0;
                if (i0 - area / 2 > 0) { min = i0 - area / 2; } else { min = 0; }
                if (i0 + area / 2 < len) { max = i0 + area / 2; } else { max = len - 1; }
                start = area - (max - min);
                output[i0] = 0;
                for (int i = min; i < max; i++)
                {
                    output[i0] += (input[i]) *(window[start + i - min]);
                    arv += window[start + i - min];
                }
                output[i0] /= arv;
            ");
        }
    }
}
