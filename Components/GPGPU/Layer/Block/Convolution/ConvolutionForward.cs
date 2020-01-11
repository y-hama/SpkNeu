using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Layer.Block.Convolution
{
    class ConvolutionForward : BlockBase
    {
        public int KernelSize { get { return (int)Argument("kernelsize").Value; } set { Argument("kernelsize").Value = value; } }
        public bool Activation { get { return Argument("activation").Value > 0 ? true : false; } set { Argument("kernelsize").Value = value ? 1 : 0; } }
        public float Alpha { get { return Argument("alpha").Value; } set { Argument("alpha").Value = value; } }

        private RNdMatrix Kernel { get; set; }
        private RNdMatrix Bias { get; set; }


        protected override void CreateArgumentSet(List<ComputeVariable.ValueSet> args)
        {
            args.Add(new ComputeVariable.ValueSet("kernelsize", 1));
            args.Add(new ComputeVariable.ValueSet("activation", 1));
            args.Add(new ComputeVariable.ValueSet("alpha", 0.01));
        }

        protected override void SetFunctiuon()
        {
            function = new Function.Method.ConvolutionForward_Method();
        }

        protected override RNdObject CreateOutputArea()
        {
            return new RNdMatrix(Input.BatchSize, OutputChannel, Input.Width, Input.Height);
        }
        protected override void CreateExtendArea(List<Function.ComputeParameter> areas)
        {
            var rand = new Random();
            Kernel = new RNdMatrix(Input.Channels, Output.Channels, 2 * KernelSize + 1, 2 * KernelSize + 1);
            Bias = new RNdMatrix(Input.Channels, Output.Channels, 1, 1);
            double sigma = Math.Sqrt(2.0 / Kernel.Length);
            for (int ic = 0; ic < Kernel.BatchSize; ic++)
            {
                for (int oc = 0; oc < Kernel.Channels; oc++)
                {
                    Real v = 0, s = 0;
                    for (int i = 0; i < Kernel.AreaSize; i++)
                    {
                        Kernel[ic, oc, i] = BoxMullers_Method(sigma);
                        v += Kernel[ic, oc, i];
                    }

                    //Bias[ic, oc, 0] = BoxMullers_Method(sigma);
                }
            }
            areas.Add(new Function.ComputeParameter("output_temporary", Output.Clone(), State.MemoryModeSet.ReadOnly));
            areas.Add(new Function.ComputeParameter("kernel", Kernel, State.MemoryModeSet.ReadOnly));
            areas.Add(new Function.ComputeParameter("bias", Bias, State.MemoryModeSet.ReadOnly));
        }
    }
}
