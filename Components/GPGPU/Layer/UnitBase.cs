using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Layer
{
    public abstract class UnitBase
    {
        protected const int PARAMETER_OFFSET = 2;

        public RNdObject Input { set { ForwardBlock.Input.Data = value.Data; } }
        public RNdObject Output { get { return ForwardBlock.Output; } }

        public RNdObject Sigma { set { BackwardBlock.Input.Data = value.Data; } }
        public RNdObject Propagator { get { return BackwardBlock.Output; } }

        protected BlockBase ForwardBlock { get; set; }
        protected BlockBase BackwardBlock { get; set; }

        public int OutputChannels { get; set; }

        #region Constructor
        public UnitBase()
        {
            SetBlock();
        }
        #endregion

        #region Virtual/Abstruct
        protected abstract void SetBlock();
        protected abstract void ConnectShareParameter();
        #endregion


        #region PublicMethod
        public void Initialize(RNdObject inputsource)
        {
            ForwardBlock.OutputChannel = OutputChannels;
            ForwardBlock.Initialize(inputsource);
            BackwardBlock.OutputChannel = inputsource.Channels;
            BackwardBlock.Initialize(ForwardBlock.Output.Clone());
        }

        public void Confirm()
        {
            ForwardBlock.Confirm();
            BackwardBlock.Confirm();
            ConnectShareParameter();
            BackwardBlock.CreateTemporaryArea();
        }

        public double Action(State.ActionMode mode, bool forceUpdate = false)
        {
            double timespan = 0;
            switch (mode)
            {
                case State.ActionMode.Forward:
                    ForwardBlock.Action(forceUpdate);
                    timespan = ForwardBlock.ElapsedTime.TotalMilliseconds;
                    break;
                case State.ActionMode.Back:
                    BackwardBlock.Action(forceUpdate);
                    timespan = BackwardBlock.ElapsedTime.TotalMilliseconds;
                    break;
                default:
                    break;
            }
            return timespan;
        }
        #endregion
    }
}
