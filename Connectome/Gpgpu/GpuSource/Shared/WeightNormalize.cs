using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU.Function;
using Components;

namespace Connectome.Gpgpu.GpuSource.Shared
{
    class WeightNormalize : Components.GPGPU.Function.SourceCode
    {
        public override string Name
        {
            get
            {
                return @"WeightNormalize";
            }
        }

        protected override bool IsLocalFunction
        {
            get
            {
                return true;
            }
        }

        protected override ReturnType Return
        {
            get
            {
                return ReturnType.VOID;
            }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("idx", ObjectType.Value, ElementType.INT);
            AddParameter("connectWeight", ObjectType.Array, ElementType.FLOAT);
            AddParameter("axsonConnectCount", ObjectType.Array, ElementType.FLOAT);
        }

        protected override void CreateSource()
        {
            AddMethodBody(@"
            int axsonCount = (int)axsonConnectCount[idx];
            if (axsonCount != 0)
            {
                int pos = 0;
                for (int i = 0; i < idx; i++)
                {
                    pos += (int)axsonConnectCount[i];
                } 
                float sum = 0;
                for (int i = 0; i < axsonCount; i++)
                {
                    sum += connectWeight[pos + i];
                }
                if (sum != 0)
                {
                    for (int i = 0; i < axsonCount; i++)
                    {
                        connectWeight[pos + i] /= sum;
                    }
                }
            }
");
        }

        public static void WeightNormalize_cpu(int idx, Real[] connectWeight, Real[] axsonConnectCount)
        {
            int axsonCount = (int)axsonConnectCount[idx];
            if (axsonCount != 0)
            {
                int pos = FunctionCore.StartPosition(idx, axsonConnectCount);
                float sum = 0;
                for (int i = 0; i < axsonCount; i++)
                {
                    sum += connectWeight[pos + i];
                }
                if (sum != 0)
                {
                    for (int i = 0; i < axsonCount; i++)
                    {
                        connectWeight[pos + i] /= sum;
                    }
                }
            }
        }

    }
}
