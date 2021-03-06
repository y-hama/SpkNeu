﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome.Gpgpu.GpuSource.Method
{
    class LocationInitializeStep_Source : Components.GPGPU.Function.SourceCode
    {
        public override string Name
        {
            get
            {
                return @"LocationInitializeStep_Source";
            }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("px", ObjectType.Array, ElementType.FLOAT);
            AddParameter("py", ObjectType.Array, ElementType.FLOAT);
            AddParameter("pz", ObjectType.Array, ElementType.FLOAT);
            AddParameter("paxson", ObjectType.Array, ElementType.FLOAT);
            AddParameter("phasRef", ObjectType.Array, ElementType.FLOAT);

            AddParameter("count", ObjectType.Value, ElementType.INT);
            AddParameter("connectcount", ObjectType.Value, ElementType.INT);
        }

        protected override void CreateSource()
        {
            GlobalID(1);
            AddMethodBody(@"
                phasRef[i0] = 0;
                float x = px[i0];
                float y = py[i0];
                float z = pz[i0];
                float axon = paxson[i0];

                int cnt = 0;

                for (int i = 0; i < count; i++)
                {
                    if (i0 == i) { continue; }
                    if (Distance(x, y, z, px[i], py[i], pz[i]) < axon)
                    {
                        cnt++;
                    }
                }
                if(cnt > connectcount)
                {
                    phasRef[i0] = 1;
                }
");
        }
    }
}
