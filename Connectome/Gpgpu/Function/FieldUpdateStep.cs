using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;
using Components;

namespace Connectome.Gpgpu.Function
{
    class FieldUpdateStep : Components.GPGPU.Function.FunctionBase
    {
        private double MP_Param { get; set; } = 0.8;
        private double ActRho_Param { get; set; } = 0.75;

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
            float energy = (float)variable["Energy"].Value;
            float denergy = (float)variable["dEnergy"].Value;
            float mp = (float)MP_Param;
            float actrho = (float)ActRho_Param;

            for (int i0 = 0; i0 < cellCount; i0++)
            {
                int axsonCount = (int)axsonConnectCount[i0];
                if (axsonCount != 0)
                {
                    int pos = FunctionCore.StartPosition(i0, axsonConnectCount);
                    float cvl = 0.0f, f = 0.0f, w = 0.0f;
                    float min = 100.0f, max = 0.0f, delta;
                    float wmin = 100.0f, wmax = 0.0f, wdelta;
                    for (int i = 0; i < axsonCount; i++)
                    {
                        int cellIndex = (int)axsonConnectMatrix[pos + i];
                        f = cellValue[cellIndex];
                        w = connectWeight[pos + i];
                        if (f >= 0.0f && cellState[cellIndex] == 1)
                        {
                            cvl += f * w;
                            float act = cellActivity[cellIndex];
                            if (min > act) { min = act; }
                            if (max < act) { max = act; }
                        }
                        if (wmin > w) { wmin = w; }
                        if (wmax < w) { wmax = w; }
                    }
                    delta = max - min;
                    wdelta = wmax - wmin;
                    if (delta > 0)
                    {
                        wdelta = wdelta == 0.0f ? 1 : wdelta;
                        for (int i = 0; i < axsonCount; i++)
                        {
                            int cellIndex = (int)axsonConnectMatrix[pos + i];
                            f = cellValue[cellIndex];
                            if (f >= 0.0f && cellState[cellIndex] == 1)
                            {
                                float wr, ar;
                                wr = (float)(mp * ((connectWeight[pos + i] - wmin) / (wdelta)) + (1 - mp));
                                ar = (float)(mp * ((cellActivity[cellIndex] - min) / delta) + (1 - mp));
                                connectWeight[pos + i] += wr * ar;
                            }
                        }
                        FunctionCore.WeightNormalize(i0, connectWeight, axsonConnectCount);
                    }

                    if ((int)cellState[i0] == 0)
                    {
                        resValue[i0] = (cellValue[i0] + cvl * 0.5f);
                        if ((resValue[i0] > 0.25f) || (resValue[i0] > 0.1f && cellEnergy[i0] >= 1.0f))
                        {
                            resState[i0] = 1.0f;
                            cellEnergy[i0] -= 1.0f;
                            cellEnergy[i0] = cellEnergy[i0] > 0 ? cellEnergy[i0] : 0;
                        }
                        else
                        {
                            cellEnergy[i0] += denergy;
                            resValue[i0] *= 0.5f;
                        }
                    }
                    else if ((int)cellState[i0] == 1)
                    {
                        resValue[i0] = cellValue[i0] * 1.25f;
                        if (resValue[i0] > 1.0f)
                        {
                            resValue[i0] = 1;
                            resState[i0] = 2;
                        }
                    }
                    else if ((int)cellState[i0] == 2)
                    {
                        resValue[i0] = cellValue[i0] * 0.75 - 0.1f;
                        if (resValue[i0] < -0.25f)
                        {
                            resState[i0] = 3;
                        }
                    }
                    else if ((int)cellState[i0] == 3)
                    {
                        resValue[i0] = cellValue[i0] * 0.75f + (0.0001f);
                        if (resValue[i0] > 0.0f)
                        {
                            resState[i0] = 0;
                        }
                    }
                    cellEnergy[i0] *= 0.99f;
                    float tmpval = resValue[i0] > 0.5f ? 1.0f : 0.0f;
                    resActivity[i0] = actrho * cellActivity[i0] + (1.0f - actrho) * tmpval;
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
            float energy = (float)variable["Energy"].Value;
            float d_energy = (float)variable["dEnergy"].Value;
            float mp = (float)MP_Param;
            float actrho = (float)ActRho_Param;

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
                SetParameter(energy, ValueMode.FLOAT);
                SetParameter(d_energy, ValueMode.FLOAT);
                SetParameter(mp, ValueMode.FLOAT);
                SetParameter(actrho, ValueMode.FLOAT);
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
