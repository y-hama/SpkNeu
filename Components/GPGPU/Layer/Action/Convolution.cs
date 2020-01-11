using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Layer.Action
{
    class Convolution : LayerBase
    {
        public int KernelSize { get { return (int)Argument("kernelsize").Value; } set { Argument("kernelsize").Value = value; } }

        private RNdMatrix Kernel { get; set; }
        private RNdMatrix Bias { get; set; }


        protected override void CreateArgumentSet(List<ComputeVariable.ValueSet> args)
        {
            args.Add(new ComputeVariable.ValueSet("kernelsize", 1));
        }

        protected override void SetFunctiuon()
        {
            function = new Function.Method.Convolution_Method();
        }

        protected override RNdObject CreateOutputArea()
        {
            return new RNdMatrix(Input.BatchSize, Input.Channels, Input.Width, Input.Height);
        }
        protected override void CreateExtendArea(List<Function.ComputeParameter> areas)
        {
            var rand = new Random();
            Kernel = new RNdMatrix(Input.Channels, Output.Channels, 2 * KernelSize + 1, 2 * KernelSize + 1);
            for (int i = 0; i < Kernel.Data.Length; i++)
            {
                Kernel.Data[i] = BoxMullers_Method(2 / Math.Sqrt(Kernel.Length));
                //if (i % Kernel.AreaSize == c) { Kernel.Data[i] = 1; c++; i += Kernel.AreaSize; }
            }
            Bias = new RNdMatrix(Input.Channels, Output.Channels, 1, 1);
            for (int i = 0; i < Bias.Data.Length; i++)
            {
                Bias.Data[i] = BoxMullers_Method(2 / Math.Sqrt(Kernel.Length));
            }
            areas.Add(new Function.ComputeParameter("kernel", Kernel, State.MemoryModeSet.ReadOnly));
            areas.Add(new Function.ComputeParameter("bias", Bias, State.MemoryModeSet.ReadOnly));
        }
    }
}
