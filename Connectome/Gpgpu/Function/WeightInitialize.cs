using Components.GPGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU.Function;

namespace Connectome.Gpgpu.Function
{
    class WeightInitialize : FunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new GpuSource.Method.WeightInitialize_Source());
        }

        private ComputeParameter connectWeight;
        private ComputeParameter axsonConnectCount;
        private ComputeParameter axsonConnectStartIndex;
        private ComputeParameter axsonConnectMatrix;
        private int cellCount;
        private int neuronCount;

        protected override void ConvertVariable(ComputeVariable variable)
        {
            connectWeight = variable.Parameter[0].Instance;
            axsonConnectCount = variable.Parameter[1].Instance;
            axsonConnectStartIndex = variable.Parameter[2].Instance;
            axsonConnectMatrix = variable.Parameter[3].Instance;

            cellCount = (int)variable["CellCount"].Value;
            neuronCount = (int)variable["NeuronCount"].Value;
        }

        protected override void CpuFunction()
        {
            for (int i0 = 0; i0 < cellCount; i0++)
            {
                WeightNormalize(i0, connectWeight, axsonConnectCount, (int)axsonConnectStartIndex[i0]);
            }
        }

        protected override void GpuFunction()
        {
            using (ComputeBufferSet _connectWeight = ConvertBuffer(connectWeight))
            using (ComputeBufferSet _axsonConnectCount = ConvertBuffer(axsonConnectCount))
            using (ComputeBufferSet _axsonConnectStartIndex = ConvertBuffer(axsonConnectStartIndex))
            {
                SetParameter(_connectWeight);
                SetParameter(_axsonConnectCount);
                SetParameter(_axsonConnectStartIndex);
                Execute(cellCount);
                ReadBuffer(_connectWeight, ref connectWeight.Array.Data);
            }
        }
    }
}
