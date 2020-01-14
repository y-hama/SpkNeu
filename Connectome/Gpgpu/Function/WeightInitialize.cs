using Components.GPGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome.Gpgpu.Function
{
    class WeightInitialize : Components.GPGPU.Function.FunctionBase
    {

        protected override void CreateGpuSource()
        {
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
            var connectWeight = variable.Parameter[0].Instance.Array.Data;
            var axsonConnectCount = variable.Parameter[1].Instance.Array.Data;
            var axsonConnectMatrix = variable.Parameter[2].Instance.Array.Data;

            int cellCount = (int)variable["CellCount"].Value;
            int neuronCount = (int)variable["NeuronCount"].Value;

            for (int i0 = 0; i0 < cellCount; i0++)
            {
                int axsonCount = (int)axsonConnectCount[i0];
                if (axsonCount != 0)
                {
                    int pos = FunctionCore.StartPosition(i0, axsonConnectCount);
                    float sum = 0;
                    for (int i = 0; i < axsonCount; i++)
                    {
                        sum += connectWeight[pos + i];
                    }
                    if (sum != 0)
                    {
                        for (int i = 0; i < axsonCount; i++)
                        {
                            connectWeight[pos + i] /= sum;
                        }
                    }
                }
            }
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
        }
    }
}
