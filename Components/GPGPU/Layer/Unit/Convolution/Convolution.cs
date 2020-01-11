using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Layer.Unit.Convolution
{
    public class Convolution : UnitBase
    {
        public int KernelSize
        {
            set
            {
                ((Layer.Block.Convolution.ConvolutionForward)ForwardBlock).KernelSize = value;
                ((Layer.Block.Convolution.ConvolutionBack)BackwardBlock).KernelSize = value;
            }
        }

        public bool Activation
        {
            set
            {
                ((Layer.Block.Convolution.ConvolutionForward)ForwardBlock).Activation = value;
                ((Layer.Block.Convolution.ConvolutionBack)BackwardBlock).Activation = value;
            }
        }

        public double Rho
        {
            set
            {
                ((Layer.Block.Convolution.ConvolutionBack)BackwardBlock).Rho = (float)value;
            }
        }

        public double Alpha
        {
            set
            {
                ((Layer.Block.Convolution.ConvolutionForward)ForwardBlock).Alpha = (float)value;
                ((Layer.Block.Convolution.ConvolutionBack)BackwardBlock).Alpha = (float)value;
            }
        }

        protected override void SetBlock()
        {
            ForwardBlock = new Layer.Block.Convolution.ConvolutionForward();
            BackwardBlock = new Layer.Block.Convolution.ConvolutionBack();
        }

        protected override void ConnectShareParameter()
        {
            for (int i = 0; i < ForwardBlock.AreaList.Count; i++)
            {
                BackwardBlock.AreaList[i + PARAMETER_OFFSET].Array = ForwardBlock.AreaList[i].Array;
            }
        }
    }
}
