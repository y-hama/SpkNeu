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
            AddParameter("cellSignal", ObjectType.Array, ElementType.FLOAT);
            AddParameter("cellActivity", ObjectType.Array, ElementType.FLOAT);
            AddParameter("cellState", ObjectType.Array, ElementType.FLOAT);
            AddParameter("connectWeight", ObjectType.Array, ElementType.FLOAT);
            AddParameter("cellEnergy", ObjectType.Array, ElementType.FLOAT);
            AddParameter("axsonConnectCount", ObjectType.Array, ElementType.FLOAT);
            AddParameter("axsonConnectStartIndex", ObjectType.Array, ElementType.FLOAT);
            AddParameter("axsonConnectMatrix", ObjectType.Array, ElementType.FLOAT);
            AddParameter("resValue", ObjectType.Array, ElementType.FLOAT);
            AddParameter("resSignal", ObjectType.Array, ElementType.FLOAT);
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
                    int pos = (int)axsonConnectStartIndex[i0];
                    float rv = 0;
                    float act, actmin = 100, actmax = 0, dact = 0;
                    for (int i = 0; i < axsonCount; i++)
                    {
                        int cellrefindex = (int)axsonConnectMatrix[pos + i];
                        act = cellActivity[cellrefindex];
                        rv += connectWeight[pos + i] * (cellSignal[cellrefindex]);
                        if (actmin > act) { actmin = act; }
                        if (actmax < act) { actmax = act; }
                    }
                    dact = actmax - actmin;

                    if (cellState[i0] == 0)
                    {
                        resValue[i0] = StepNext(cellValue[i0], 0.5, rv);
                        if (resValue[i0] > 0.5 && cellEnergy[i0] > 1)
                        {
                            cellEnergy[i0] = cellEnergy[i0] - 1;
                            resState[i0] = 1;
                        }
                        else
                        {
                            cellEnergy[i0] += denergy;
                            if (dact > 0)
                            {
                                for (int i = 0; i < axsonCount; i++)
                                {
                                    int cellrefindex = (int)axsonConnectMatrix[pos + i];
                                    connectWeight[pos + i] += 0.1 * (1 - cellActivity[i0]) * (cellActivity[cellrefindex] - actmin) / dact;
                                }
                                WeightNormalize(i0, connectWeight, axsonConnectCount, pos);
                            }
                        }
                    }
                    else if (cellState[i0] == 1)
                    {
                        resValue[i0] = StepNext(cellValue[i0], 1.75, 0);
                        if (resValue[i0] > 1)
                        {
                            resState[i0] = 2;
                        }
                    }
                    else if (cellState[i0] == 2)
                    {
                        resValue[i0] = StepNext(cellValue[i0], 0.5, -0.5);
                        if (resValue[i0] < -0.25)
                        {
                            resState[i0] = 3;
                        }
                    }
                    else if (cellState[i0] == 3)
                    {
                        resValue[i0] = StepNext(cellValue[i0], 0.75, 0.1);
                        if (resValue[i0] > 0)
                        {
                            resState[i0] = 0;
                        }
                        else
                        {
                            cellEnergy[i0] += denergy;
                        }
                    }
                    resSignal[i0] = resValue[i0] > 0.5 ? 1 : 0;
                    resActivity[i0] = cellActivity[i0] + (resSignal[i0] > 0 ? 0.1 : -0.1);
                    if (resActivity[i0] < 0) { resActivity[i0] = 0; }
                    if (resActivity[i0] > 1) { resActivity[i0] = 1; }
                }
");
        }
    }
}
