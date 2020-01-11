using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Layer
{
    public abstract class BlockBase
    {
        #region Property
        private static Random random = new Random();

        protected GPGPU.Function.FunctionBase function { get; set; }
        public TimeSpan ElapsedTime { get { return function.StepElapsedSpan; } }

        public RNdObject Input { get { return AreaList[0].Array; } set { AreaList[0].Array = value; } }
        public RNdObject Output { get { return AreaList[1].Array; } set { AreaList[1].Array = value; } }
        public int OutputChannel { get; set; }

        private List<Function.ComputeParameter> area { get; set; }

        public List<Function.ComputeParameter> AreaList { get; set; }
        protected List<ComputeVariable.ValueSet> ArgumentList { get; set; }
        public ComputeVariable.ValueSet Argument(string name)
        {
            return ArgumentList.Find(x => x.Name == name);
        }
        #endregion

        #region Constructor
        public BlockBase()
        {
            OutputChannel = -1;
            ArgumentList = new List<ComputeVariable.ValueSet>();
            CreateArgumentSet(ArgumentList);
        }
        #endregion

        #region Virtual/Abstruct
        protected abstract void SetFunctiuon();
        protected abstract RNdObject CreateOutputArea();
        protected abstract void CreateExtendArea(List<Function.ComputeParameter> areas);
        protected abstract void CreateArgumentSet(List<ComputeVariable.ValueSet> args);
        public virtual void CreateTemporaryAreaInnerMethod(List<Function.ComputeParameter> areas) { }
        #endregion

        #region ProtectedMethod
        protected float BoxMullers_Method(double sigma = 1.0, double ave = 0.0)
        {
            double X = random.NextDouble();
            double Y = random.NextDouble();

            return (float)(sigma * Math.Sqrt(-2.0 * Math.Log(X)) * Math.Cos(2.0 * Math.PI * Y) + ave);
        }

        #endregion

        #region PrivateMethod
        private ComputeVariable CreateVariable()
        {
            var variable = new ComputeVariable();
            foreach (var item in AreaList)
            {
                variable.Add(item.Name, item.Array, item.MemoryModeBase);
            }
            foreach (var item in ArgumentList)
            {
                variable.Argument.Add(new ComputeVariable.ValueSet(item.Name) { Value = item.Value });
            }
            return variable;
        }
        private void UpdateVariable(ComputeVariable variable)
        {
            for (int i = 0; i < variable.Parameter.Count; i++)
            {
                if (variable[i].Instance.MemoryModeBase == State.MemoryModeSet.WriteOnly)
                {
                    AreaList[i].Array = variable[i].Instance.Array;
                }
            }
        }
        #endregion

        #region PublicMethod
        public void Initialize(RNdObject inputsource)
        {
            area = new List<Function.ComputeParameter>();
            AreaList = new List<Function.ComputeParameter>();
            SetFunctiuon();
            if (function == null) { throw new Exception(); }
            function.FunctionConfiguration();

            AreaList.Add(new Function.ComputeParameter("input", inputsource, State.MemoryModeSet.ReadOnly));
            AreaList.Add(new Function.ComputeParameter("output", CreateOutputArea(), State.MemoryModeSet.WriteOnly));

        }

        public void Confirm()
        {
            CreateExtendArea(area);
            foreach (var item in area)
            {
                AreaList.Add(item);
            }
        }

        public void Action(bool forceUpdate = false)
        {
            var variable = CreateVariable();
            function.Do(forceUpdate, variable);
            UpdateVariable(variable);
        }

        public void CreateTemporaryArea()
        {
            CreateTemporaryAreaInnerMethod(AreaList);
        }
        #endregion
    }
}
