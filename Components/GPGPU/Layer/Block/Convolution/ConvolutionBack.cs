using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Layer.Block.Convolution
{
    class ConvolutionBack : BlockBase
    {
        public int KernelSize { get { return (int)Argument("kernelsize").Value; } set { Argument("kernelsize").Value = value; } }
        public bool Activation { get { return Argument("activation").Value > 0 ? true : false; } set { Argument("kernelsize").Value = value ? 1 : 0; } }
        public float Rho { get { return (int)Argument("rho").Value; } set { Argument("rho").Value = value; } }
        public float Alpha { get { return Argument("alpha").Value; } set { Argument("alpha").Value = value; } }

        private RNdMatrix Kernel { get { return (RNdMatrix)AreaList[5].Array; } }
        private RNdMatrix Bias { get { return (RNdMatrix)AreaList[6].Array; } }
        private RNdMatrix dKernel { get; set; }
        private RNdMatrix dBias { get; set; }

        protected override void CreateArgumentSet(List<ComputeVariable.ValueSet> args)
        {
            args.Add(new ComputeVariable.ValueSet("kernelsize", 1));
            args.Add(new ComputeVariable.ValueSet("activation", 1));
            args.Add(new ComputeVariable.ValueSet("rho", 0.001));
            args.Add(new ComputeVariable.ValueSet("alpha", 0.01));
        }

        protected override void SetFunctiuon()
        {
            function = new Function.Method.ConvolutionBack_Method();
        }

        protected override RNdObject CreateOutputArea()
        {
            return new RNdMatrix(Input.BatchSize, OutputChannel, Input.Width, Input.Height);
        }

        protected override void CreateExtendArea(List<Function.ComputeParameter> areas)
        {
            areas.Add(new Function.ComputeParameter("fwdin", RNdObject.Zeros(), State.MemoryModeSet.WriteOnly));
            areas.Add(new Function.ComputeParameter("fwpout", RNdObject.Zeros(), State.MemoryModeSet.WriteOnly));
            areas.Add(new Function.ComputeParameter("fwout", RNdObject.Zeros(), State.MemoryModeSet.WriteOnly));
            areas.Add(new Function.ComputeParameter("kernel", RNdObject.Zeros(), State.MemoryModeSet.WriteOnly));
            areas.Add(new Function.ComputeParameter("bias", RNdObject.Zeros(), State.MemoryModeSet.WriteOnly));
        }

        public override void CreateTemporaryAreaInnerMethod(List<Function.ComputeParameter> areas)
        {
            dKernel = new RNdMatrix(Kernel.Shape);
            dBias = new RNdMatrix(Bias.Shape);
            areas.Add(new Function.ComputeParameter("dKernel", dKernel, State.MemoryModeSet.WriteOnly));
            areas.Add(new Function.ComputeParameter("dBias", dBias, State.MemoryModeSet.WriteOnly));
        }
    }
}
