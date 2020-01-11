using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Layer.Action
{
    public class MovingAverage : LayerBase
    {
        public enum WindowShape
        {
            Rectangle,
        }

        public Real Area { get { return Argument("area").Value; } set { Argument("area").Value = value; } }
        public WindowShape Shape { get; set; }

        private RNdArray Window { get; set; }

        protected override void CreateArgumentSet(List<ComputeVariable.ValueSet> args)
        {
            args.Add(new ComputeVariable.ValueSet("area", 1000));
        }

        protected override void SetFunctiuon()
        {
            function = new Function.Method.MA_Method();
        }
        protected override RNdObject CreateOutputArea()
        {
            return Input.Clone();
        }

        protected override void CreateExtendArea(List<Function.ComputeParameter> areas)
        {
            Random rand = new Random();
            var window = new RNdArray(new Real[(int)Area]);
            switch (Shape)
            {
                case WindowShape.Rectangle:
                    CreateWindow_Rectangle(ref window);
                    break;
                default:
                    break;
            }
            Window = window;
            areas.Add(new Function.ComputeParameter("window", Window, State.MemoryModeSet.ReadOnly));
        }
        private void CreateWindow_Rectangle(ref RNdArray window)
        {
            for (int i = 0; i < Area; i++)
            {
                window[i] = 1;
            }
        }
    }
}
