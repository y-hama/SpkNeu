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
            AddSource(new GpuSource.Method.WeightInitialize_Source());
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
                FunctionCore.WeightNormalize(i0, connectWeight, axsonConnectCount);
            }
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
            var connectWeight = variable.Parameter[0].Instance.Array.Data;
            var axsonConnectCount = variable.Parameter[1].Instance.Array.Data;

            int cellCount = (int)variable["CellCount"].Value;

            using (ComputeBufferSet _connectWeight = ConvertBuffer(variable[0].Instance))
            using (ComputeBufferSet _axsonConnectCount = ConvertBuffer(variable[1].Instance))
            {
                SetParameter(_connectWeight);
                SetParameter(_axsonConnectCount);
                Execute(cellCount);
                ReadBuffer(_connectWeight, ref connectWeight);
            }
        }
    }
}
