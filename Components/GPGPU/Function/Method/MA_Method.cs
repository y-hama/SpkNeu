using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function.Method
{
    class MA_Method : FunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new Source.MA_Source());
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
            Real[] input = variable[0].Instance.Array.Data;
            Real[] output = variable[1].Instance.Array.Data;
            Real[] window = variable[2].Instance.Array.Data;

            int len = variable[0].Infomation.ArrayLength;
            int area = (int)variable["area"].Value;

            for (int i0 = 0; i0 < len; i0++)
            {
                int start;
                int min, max;
                float arv = 0;
                if (i0 - area / 2 > 0) { min = i0 - area / 2; } else { min = 0; }
                if (i0 + area / 2 < len) { max = i0 + area / 2; } else { max = len - 1; }
                start = area - (max - min);
                output[i0] = 0;
                for (int i = min; i < max; i++)
                {
                    output[i0] += (input[i]) * (window[start + i - min]);
                    arv += window[start + i - min];
                }
                output[i0] /= arv;
            }
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
            int len = variable[0].Infomation.ArrayLength;
            int area = (int)variable["area"].Value;

            using (Cloo.ComputeBuffer<Real> _input = ConvertBuffer(variable[0].Instance))
            using (Cloo.ComputeBuffer<Real> _output = ConvertBuffer(variable[1].Instance))
            using (Cloo.ComputeBuffer<Real> _window = ConvertBuffer(variable[2].Instance))
            {
                SetParameter(_input);
                SetParameter(_output);
                SetParameter(_window);
                SetParameter(len, ValueMode.INT);
                SetParameter(area, ValueMode.INT);
                Execute(len);
                ReadBuffer(_output, ref variable[1].Instance.Array.Data);
            }
        }
    }
}
