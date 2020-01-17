using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;
using Components.GPGPU.Function;
using Components;

namespace Connectome.Gpgpu.Function
{
    class FieldUpdateStep : FunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new GpuSource.Method.FieldUpdateStep_Source());
        }

        private double MP_Param { get; set; } = 0.9;
        private double ActRho_Param { get; set; } = 0.75;

        private ComputeParameter cellValue;
        private ComputeParameter cellSignal;
        private ComputeParameter cellActivity;
        private ComputeParameter cellState;
        private ComputeParameter connectWeight;
        private ComputeParameter cellEnergy;
        private ComputeParameter axsonConnectCount;
        private ComputeParameter axsonConnectStartIndex;
        private ComputeParameter axsonConnectMatrix;
        private ComputeParameter resValue;
        private ComputeParameter resSignal;
        private ComputeParameter resActivity;
        private ComputeParameter resState;
        private int cellCount;
        private float energy;
        private float denergy;
        private float mp;
        private float actrho;

        private static Random random = new Random();

        protected override void ConvertVariable(ComputeVariable variable)
        {
            cellValue = variable.Parameter[0].Instance;
            cellSignal = variable.Parameter[1].Instance;
            cellActivity = variable.Parameter[2].Instance;
            cellState = variable.Parameter[3].Instance;
            connectWeight = variable.Parameter[4].Instance;
            cellEnergy = variable.Parameter[5].Instance;
            axsonConnectCount = variable.Parameter[6].Instance;
            axsonConnectStartIndex = variable.Parameter[7].Instance;
            axsonConnectMatrix = variable.Parameter[8].Instance;
            resValue = variable.Parameter[9].Instance;
            resSignal = variable.Parameter[10].Instance;
            resActivity = variable.Parameter[11].Instance;
            resState = variable.Parameter[12].Instance;

            cellCount = (int)variable["CellCount"].Value;
            energy = (float)variable["Energy"].Value;
            denergy = (float)variable["dEnergy"].Value;
            mp = (float)MP_Param;
            actrho = (float)ActRho_Param;
        }

        protected override void CpuFunction()
        {
            for (int i0 = 0; i0 < cellCount; i0++)
            {
                int axsonCount = (int)axsonConnectCount[i0];
                if (axsonCount != 0)
                {
                    int pos = (int)axsonConnectStartIndex[i0];
                    for (int i = 0; i < axsonCount; i++)
                    {
                        int id = (int)axsonConnectMatrix[pos + i];
                        float w = connectWeight[pos + i];
                        float tcelv = cellValue[id];
                        resValue[i0] += w * tcelv / 2;
                    }
                    if (resValue[i0] > 1) { resValue[i0] = 0; }
                }
            }
        }

        protected override void GpuFunction()
        {
            using (ComputeBufferSet _cellValue = ConvertBuffer(cellValue))
            using (ComputeBufferSet _cellSignal = ConvertBuffer(cellSignal))
            using (ComputeBufferSet _cellActivity = ConvertBuffer(cellActivity))
            using (ComputeBufferSet _cellState = ConvertBuffer(cellState))
            using (ComputeBufferSet _connectWeight = ConvertBuffer(connectWeight))
            using (ComputeBufferSet _cellEnergy = ConvertBuffer(cellEnergy))
            using (ComputeBufferSet _axsonConnectCount = ConvertBuffer(axsonConnectCount))
            using (ComputeBufferSet _axsonConnectStartIndex = ConvertBuffer(axsonConnectStartIndex))
            using (ComputeBufferSet _axsonConnectMatrix = ConvertBuffer(axsonConnectMatrix))
            using (ComputeBufferSet _resValue = ConvertBuffer(resValue))
            using (ComputeBufferSet _resSignal = ConvertBuffer(resSignal))
            using (ComputeBufferSet _resActivity = ConvertBuffer(resActivity))
            using (ComputeBufferSet _resState = ConvertBuffer(resState))
            {
                SetParameter(_cellValue);
                SetParameter(_cellSignal);
                SetParameter(_cellActivity);
                SetParameter(_cellState);
                SetParameter(_connectWeight);
                SetParameter(_cellEnergy);
                SetParameter(_axsonConnectCount);
                SetParameter(_axsonConnectStartIndex);
                SetParameter(_axsonConnectMatrix);
                SetParameter(_resValue);
                SetParameter(_resSignal);
                SetParameter(_resActivity);
                SetParameter(_resState);
                SetParameter(cellCount, ValueMode.INT);
                SetParameter(energy, ValueMode.FLOAT);
                SetParameter(denergy, ValueMode.FLOAT);
                SetParameter(mp, ValueMode.FLOAT);
                SetParameter(actrho, ValueMode.FLOAT);
                Execute(cellCount);
                ReadBuffer(_cellEnergy, ref cellEnergy.Array.Data);
                ReadBuffer(_connectWeight, ref connectWeight.Array.Data);
                ReadBuffer(_resValue, ref resValue.Array.Data);
                ReadBuffer(_resSignal, ref resSignal.Array.Data);
                ReadBuffer(_resActivity, ref resActivity.Array.Data);
                ReadBuffer(_resState, ref resState.Array.Data);
            }
        }
    }
}
