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
            AddParameter("connectWeight", ObjectType.Array, ElementType.FLOAT);
            AddParameter("axsonConnectCount", ObjectType.Array, ElementType.FLOAT);
            AddParameter("axsonConnectMatrix", ObjectType.Array, ElementType.FLOAT);
            AddParameter("resValue", ObjectType.Array, ElementType.FLOAT);

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
                    float cvl = 0;
                    for (int i = 0; i < axsonCount; i++)
                    {
                        int cellIndex = (int)axsonConnectMatrix[pos + i];
                        cvl += cellValue[cellIndex] * connectWeight[pos + i];
                    }
                    resValue[i0] = (cellValue[i0] + cvl);
                    resValue[i0] *= 0.9f;
                }
");
        }
    }
}
