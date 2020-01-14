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
            AddSource(new GpuSource.Method.FieldUpdateStep_Source());
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
            var cellValue = variable.Parameter[0].Instance.Array.Data;
            var connectWeight = variable.Parameter[1].Instance.Array.Data;
            var axsonConnectCount = variable.Parameter[2].Instance.Array.Data;
            var axsonConnectMatrix = variable.Parameter[3].Instance.Array.Data;
            var resValue = variable.Parameter[4].Instance.Array.Data;

            int cellCount = (int)variable["CellCount"].Value;

            for (int i0 = 0; i0 < cellCount; i0++)
            {
                int axsonCount = (int)axsonConnectCount[i0];
                if (axsonCount != 0)
                {
                    int pos = FunctionCore.StartPosition(i0, axsonConnectCount);
                    float cvl = 0;
                    for (int i = 0; i < axsonCount; i++)
                    {
                        int cellIndex = (int)axsonConnectMatrix[pos + i];
                        cvl += cellValue[cellIndex] * connectWeight[pos + i];
                    }
                    resValue[i0] = (cellValue[i0] + cvl);
                    resValue[i0] *= 0.9;
                }
            }
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
            var cellValue = variable.Parameter[0].Instance.Array.Data;
            var connectWeight = variable.Parameter[1].Instance.Array.Data;
            var axsonConnectCount = variable.Parameter[2].Instance.Array.Data;
            var axsonConnectMatrix = variable.Parameter[3].Instance.Array.Data;
            var resValue = variable.Parameter[4].Instance.Array.Data;

            int cellCount = (int)variable["CellCount"].Value;

            using (ComputeBufferSet _cellValue = ConvertBuffer(variable[0].Instance))
            using (ComputeBufferSet _connectWeight = ConvertBuffer(variable[1].Instance))
            using (ComputeBufferSet _axsonConnectCount = ConvertBuffer(variable[2].Instance))
            using (ComputeBufferSet _axsonConnectMatrix = ConvertBuffer(variable[3].Instance))
            using (ComputeBufferSet _resValue = ConvertBuffer(variable[4].Instance))
            {
                SetParameter(_cellValue);
                SetParameter(_connectWeight);
                SetParameter(_axsonConnectCount);
                SetParameter(_axsonConnectMatrix);
                SetParameter(_resValue);
                SetParameter(cellCount, ValueMode.INT);
                Execute(cellCount);
                ReadBuffer(_resValue, ref resValue);
            }
        }
    }
}
