using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;
using Components;

namespace Connectome.Gpgpu.Function
{
    class LocationInitializeStep : Components.GPGPU.Function.FunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new GpuSource.Method.LocationInitializeStep_Source());
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
            var px = variable.Parameter[0].Instance.Array.Data;
            var py = variable.Parameter[1].Instance.Array.Data;
            var pz = variable.Parameter[2].Instance.Array.Data;
            var paxson = variable.Parameter[3].Instance.Array.Data;
            var phasRef = variable.Parameter[4].Instance.Array.Data;

            int count = (int)variable["count"].Value;

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
                    if (FunctionCore.Distance(x, y, z, px[i0], px[i0], px[i0]) < axon)
                    {
                        phasRef[i0] = 1;
                        break;
                    }
                }
            }
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
            var px = variable.Parameter[0].Instance.Array.Data;
            var py = variable.Parameter[1].Instance.Array.Data;
            var pz = variable.Parameter[2].Instance.Array.Data;
            var paxson = variable.Parameter[3].Instance.Array.Data;
            var phasRef = variable.Parameter[4].Instance.Array.Data;

            int count = (int)variable["count"].Value;

            using (ComputeBufferSet _px = ConvertBuffer(variable[0].Instance))
            using (ComputeBufferSet _py = ConvertBuffer(variable[1].Instance))
            using (ComputeBufferSet _pz = ConvertBuffer(variable[2].Instance))
            using (ComputeBufferSet _paxson = ConvertBuffer(variable[3].Instance))
            using (ComputeBufferSet _phasRef = ConvertBuffer(variable[4].Instance))
            {
                SetParameter(_px);
                SetParameter(_py);
                SetParameter(_pz);
                SetParameter(_paxson);
                SetParameter(_phasRef);
                SetParameter(count, ValueMode.INT);
                Execute(count);
                ReadBuffer(_phasRef, ref phasRef);
            }
        }
    }
}
