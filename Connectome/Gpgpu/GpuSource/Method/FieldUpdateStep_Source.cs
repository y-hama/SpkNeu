using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connectome.Gpgpu.GpuSource.Method
{
    class FieldUpdateStep_Source : Components.GPGPU.Function.SourceCode
    {
        public override string Name
        {
            get
            {
                return @"FieldUpdateStep_Source";
            }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("cellValue", ObjectType.Array, ElementType.FLOAT);
            AddParameter("cellActivity", ObjectType.Array, ElementType.FLOAT);
            AddParameter("cellState", ObjectType.Array, ElementType.FLOAT);
            AddParameter("connectWeight", ObjectType.Array, ElementType.FLOAT);
            AddParameter("cellEnergy", ObjectType.Array, ElementType.FLOAT);
            AddParameter("axsonConnectCount", ObjectType.Array, ElementType.FLOAT);
            AddParameter("axsonConnectMatrix", ObjectType.Array, ElementType.FLOAT);
            AddParameter("resValue", ObjectType.Array, ElementType.FLOAT);
            AddParameter("resActivity", ObjectType.Array, ElementType.FLOAT);
            AddParameter("resState", ObjectType.Array, ElementType.FLOAT);

            AddParameter("cellCount", ObjectType.Value, ElementType.INT);
            AddParameter("energy", ObjectType.Value, ElementType.FLOAT);
            AddParameter("denergy", ObjectType.Value, ElementType.FLOAT);
            AddParameter("mp", ObjectType.Value, ElementType.FLOAT);
            AddParameter("actrho", ObjectType.Value, ElementType.FLOAT);
        }

        protected override void CreateSource()
        {
            GlobalID(1);
            AddMethodBody(@"
                int axsonCount = (int)axsonConnectCount[i0];
                if (axsonCount != 0)
                {
                    int pos = StartPosition(i0, axsonConnectCount);
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
                        WeightNormalize(i0, connectWeight, axsonConnectCount);
                    }

                    if ((int)cellState[i0] == 0)
                    {
                        resValue[i0] = (cellValue[i0] + cvl * 0.5f);
                        if ((resValue[i0] > 0.25f) && (cellEnergy[i0] >= 1.0f))
                        {
                            resState[i0] = 1.0f;
                            cellEnergy[i0] -= 1.0f;
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
                    cellEnergy[i0] *= 0.999f;
                    float tmpval = resValue[i0] > 0.5f ? 1.0f : 0.0f;
                    resActivity[i0] = actrho * cellActivity[i0] + (1.0f - actrho) * tmpval;
                }
");
        }
    }
}
