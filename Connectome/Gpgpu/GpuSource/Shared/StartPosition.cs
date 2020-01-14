using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome.Gpgpu.GpuSource.Shared
{
    class StartPosition : Components.GPGPU.Function.SourceCode
    {
        public override string Name
        {
            get
            {
                return @"StartPosition";
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
                return ReturnType.INT;
            }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("idx", ObjectType.Value, ElementType.INT);
            AddParameter("connectionCount", ObjectType.Array, ElementType.FLOAT);
        }

        protected override void CreateSource()
        { 
            AddMethodBody(@"
            int res = 0;
            for (int i = 0; i < idx; i++)
            {
                res += (int)connectionCount[i];
            }
            return res;
");
        }

        public static int StartPosition_cpu(int idx, Real[] connectionCount)
        {
            int res = 0;
            for (int i = 0; i < idx; i++)
            {
                res += (int)connectionCount[i];
            }
            return res;
        }
    }
}
