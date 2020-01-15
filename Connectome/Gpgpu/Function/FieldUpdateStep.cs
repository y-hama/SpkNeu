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
            var cellActivity = variable.Parameter[1].Instance.Array.Data;
            var cellState = variable.Parameter[2].Instance.Array.Data;
            var connectWeight = variable.Parameter[3].Instance.Array.Data;
            var cellEnergy = variable.Parameter[4].Instance.Array.Data;
            var axsonConnectCount = variable.Parameter[5].Instance.Array.Data;
            var axsonConnectMatrix = variable.Parameter[6].Instance.Array.Data;
            var resValue = variable.Parameter[7].Instance.Array.Data;
            var resActivity = variable.Parameter[8].Instance.Array.Data;
            var resState = variable.Parameter[9].Instance.Array.Data;

            int cellCount = (int)variable["CellCount"].Value;

            for (int i0 = 0; i0 < cellCount; i0++)
            {
                int axsonCount = (int)axsonConnectCount[i0];
                if (axsonCount != 0)
                {
                    int pos = FunctionCore.StartPosition(i0, axsonConnectCount);
                    float cvl = 0, f = 0;
                    float min = 100, max = 0, delta;
                    for (int i = 0; i < axsonCount; i++)
                    {
                        int cellIndex = (int)axsonConnectMatrix[pos + i];
                        f = cellValue[cellIndex];
                        if (f >= 0 && cellState[cellIndex] == 1)
                        {
                            cvl += f * connectWeight[pos + i];
                            float act = cellActivity[cellIndex];
                            if (min > act) { min = act; }
                            if (max < act) { max = act; }
                        }
                    }
                    delta = max - min;
                    if (delta > 0)
                    {
                        for (int i = 0; i < axsonCount; i++)
                        {
                            int cellIndex = (int)axsonConnectMatrix[pos + i];
                            f = cellValue[cellIndex];
                            if (f >= 0 && cellState[cellIndex] == 1)
                            {
                                connectWeight[pos + i] += 0.01 * (cellActivity[cellIndex] - min) / delta;
                            }
                        }
                        FunctionCore.WeightNormalize(i0, connectWeight, axsonConnectCount);
                    }

                    if ((int)cellState[i0] == 0)
                    {
                        cellEnergy[i0] += cvl * 0.75;
                        resValue[i0] = (cellValue[i0] + cvl * 0.75);
                        if (resValue[i0] > 0.25 && cellEnergy[i0] > 1)
                        {
                            resState[i0] = 1;
                            cellEnergy[i0] = 0;
                        }
                        else
                        {
                            if (cellEnergy[i0] > 1)
                            {
                                resValue[i0] *= 0.25;
                            }
                            else
                            {
                                resValue[i0] *= 0.5;
                            }
                        }
                    }
                    else if ((int)cellState[i0] == 1)
                    {
                        resValue[i0] = cellValue[i0] * 1.25;
                        if (resValue[i0] > 1)
                        {
                            resValue[i0] = 1;
                            resState[i0] = 2;
                        }
                    }
                    else if ((int)cellState[i0] == 2)
                    {
                        resValue[i0] = cellValue[i0] * 0.75 - 0.1;
                        if (resValue[i0] < -0.25)
                        {
                            resState[i0] = 3;
                        }
                    }
                    else if ((int)cellState[i0] == 3)
                    {
                        cellEnergy[i0] += cvl * 0.001;
                        resValue[i0] = cellValue[i0] * 0.5 + 0.01;
                        if (resValue[i0] > 0)
                        {
                            resState[i0] = 0;
                        }
                    }
                    cellEnergy[i0] *= 0.95;
                    float tmpval = resValue[i0] > 0.5 ? 1 : 0;
                    float actrho = 0.75f;
                    resActivity[i0] = actrho * cellActivity[i0] + (1 - actrho) * tmpval;
                }
            }
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
            var cellValue = variable.Parameter[0].Instance.Array.Data;
            var cellActivity = variable.Parameter[1].Instance.Array.Data;
            var cellState = variable.Parameter[2].Instance.Array.Data;
            var connectWeight = variable.Parameter[3].Instance.Array.Data;
            var cellEnergy = variable.Parameter[4].Instance.Array.Data;
            var axsonConnectCount = variable.Parameter[5].Instance.Array.Data;
            var axsonConnectMatrix = variable.Parameter[6].Instance.Array.Data;
            var resValue = variable.Parameter[7].Instance.Array.Data;
            var resActivity = variable.Parameter[8].Instance.Array.Data;
            var resState = variable.Parameter[9].Instance.Array.Data;

            int cellCount = (int)variable["CellCount"].Value;

            using (ComputeBufferSet _cellValue = ConvertBuffer(variable[0].Instance))
            using (ComputeBufferSet _cellActivity = ConvertBuffer(variable[1].Instance))
            using (ComputeBufferSet _cellState = ConvertBuffer(variable[2].Instance))
            using (ComputeBufferSet _connectWeight = ConvertBuffer(variable[3].Instance))
            using (ComputeBufferSet _cellEnergy = ConvertBuffer(variable[4].Instance))
            using (ComputeBufferSet _axsonConnectCount = ConvertBuffer(variable[5].Instance))
            using (ComputeBufferSet _axsonConnectMatrix = ConvertBuffer(variable[6].Instance))
            using (ComputeBufferSet _resValue = ConvertBuffer(variable[7].Instance))
            using (ComputeBufferSet _resActivity = ConvertBuffer(variable[8].Instance))
            using (ComputeBufferSet _resState = ConvertBuffer(variable[9].Instance))
            {
                SetParameter(_cellValue);
                SetParameter(_cellActivity);
                SetParameter(_cellState);
                SetParameter(_connectWeight);
                SetParameter(_cellEnergy);
                SetParameter(_axsonConnectCount);
                SetParameter(_axsonConnectMatrix);
                SetParameter(_resValue);
                SetParameter(_resActivity);
                SetParameter(_resState);
                SetParameter(cellCount, ValueMode.INT);
                Execute(cellCount);
                ReadBuffer(_cellEnergy, ref cellEnergy);
                ReadBuffer(_connectWeight, ref connectWeight);
                ReadBuffer(_resValue, ref resValue);
                ReadBuffer(_resActivity, ref resActivity);
                ReadBuffer(_resState, ref resState);
            }
        }
    }
}
