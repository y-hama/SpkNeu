using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Layer.Action
{
    class Temporary : LayerBase
    {
        protected override void SetFunctiuon()
        {
            function = new Function.Method.Temporary();
        }
        protected override RNdObject CreateOutputArea()
        {
            return Input.Clone();
        }
        protected override void CreateExtendArea(List<Function.ComputeParameter> areas)
        {
        }
        protected override void CreateArgumentSet(List<ComputeVariable.ValueSet> args)
        {
            args.Add(new ComputeVariable.ValueSet("area", 1000));
        }
    }
}
