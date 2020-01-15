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
        }

        protected override void CreateSource()
        {
            GlobalID(1);
            AddMethodBody(@"
                int axsonCount = (int)axsonConnectCount[i0];
                if (axsonCount != 0)
                {
                    int pos = StartPosition(i0, axsonConnectCount);
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
                        WeightNormalize(i0, connectWeight, axsonConnectCount);
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
");
        }
    }
}
