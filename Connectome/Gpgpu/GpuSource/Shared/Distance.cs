using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome.Gpgpu.GpuSource.Shared
{
    class Distance : Components.GPGPU.Function.SourceCode
    {
        public override string Name
        {
            get
            {
                return @"Distance";
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
                return ReturnType.FLOAT;
            }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("x1", ObjectType.Value, ElementType.FLOAT);
            AddParameter("y1", ObjectType.Value, ElementType.FLOAT);
            AddParameter("z1", ObjectType.Value, ElementType.FLOAT);
            AddParameter("x2", ObjectType.Value, ElementType.FLOAT);
            AddParameter("y2", ObjectType.Value, ElementType.FLOAT);
            AddParameter("z2", ObjectType.Value, ElementType.FLOAT);
        }

        protected override void CreateSource()
        {
            AddMethodBody(@"
float dx = x1 - x2;
float dy = y1 - y2;
float dz = z1 - z2;
return sqrt(dx * dx + dy * dy + dz * dz);
");
        }

        public static Real Distance_cpu(Real x1, Real y1, Real z1, Real x2, Real y2, Real z2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            float dz = z1 - z2;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
