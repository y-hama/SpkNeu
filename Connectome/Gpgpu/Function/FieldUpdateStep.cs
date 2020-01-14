using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace Connectome.Gpgpu.Function
{
    class FieldUpdateStep : Components.GPGPU.Function.FunctionBase
    {
        protected override void CreateGpuSource()
        {
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
            var cellValue = variable.Parameter[0].Instance.Array.Data;
            var connectWeight = variable.Parameter[1].Instance.Array.Data;
            var axsonConnectCount = variable.Parameter[2].Instance.Array.Data;
            var axsonConnectMatrix = variable.Parameter[3].Instance.Array.Data;

            int cellCount = (int)variable["CellCount"].Value;
            int neuronCount = (int)variable["NeuronCount"].Value;

            for (int i0 = 0; i0 < cellCount; i0++)
            {
                int axsonCount = (int)axsonConnectCount[i0];
                if (axsonCount == 0)
                {
                    return;
                }
                else
                {

                }
            }
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
        }
    }
}
