using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;
using Components;
using Components.GPGPU.Function;

namespace Connectome.Gpgpu.Function
{
    class LocationInitializeStep : FunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new GpuSource.Method.LocationInitializeStep_Source());
        }

        private ComputeParameter px;
        private ComputeParameter py;
        private ComputeParameter pz;
        private ComputeParameter paxson;
        private ComputeParameter phasRef;
        private int count;
        private int connectcount;

        protected override void ConvertVariable(ComputeVariable variable)
        {
            px = variable.Parameter[0].Instance;
            py = variable.Parameter[1].Instance;
            pz = variable.Parameter[2].Instance;
            paxson = variable.Parameter[3].Instance;
            phasRef = variable.Parameter[4].Instance;

            count = (int)variable["count"].Value;
            connectcount = (int)variable["connectcount"].Value;
        }

        protected override void CpuFunction()
        {
            for (int i0 = 0; i0 < count; i0++)
            {
                phasRef[i0] = 0;
                float x = px[i0];
                float y = py[i0];
                float z = pz[i0];
                float axon = paxson[i0];

                for (int i = 0; i < count; i++)
                {
                    if (i0 == i) { continue; }
                    if (Distance(x, y, z, px[i], py[i], pz[i]) < axon)
                    {
                        phasRef[i0] = 1;
                        break;
                    }
                }
            }
        }

        protected override void GpuFunction()
        {
            using (ComputeBufferSet _px = ConvertBuffer(px))
            using (ComputeBufferSet _py = ConvertBuffer(py))
            using (ComputeBufferSet _pz = ConvertBuffer(pz))
            using (ComputeBufferSet _paxson = ConvertBuffer(paxson))
            using (ComputeBufferSet _phasRef = ConvertBuffer(phasRef))
            {
                SetParameter(_px);
                SetParameter(_py);
                SetParameter(_pz);
                SetParameter(_paxson);
                SetParameter(_phasRef);
                SetParameter(count, ValueMode.INT);
                SetParameter(connectcount, ValueMode.INT);
                Execute(count);
                ReadBuffer(_phasRef, ref phasRef.Array.Data);
            }
        }
    }
}
