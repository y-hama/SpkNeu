using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function.Optimization
{
    public abstract class Optimizer : FunctionBase
    {
        protected bool Initialize { get; set; }

        public Real Delta { get; private set; }

        private ComputeParameter ConvertParameter(float[] param)
        {
            return new ComputeParameter("parameter", new RNdArray(param), State.MemoryModeSet.ReadOnly);
        }

        private ComputeParameter ConvertBuffer(string name, Real[] buffer, State.MemoryModeSet memorymode)
        {
            RNdArray array = new RNdArray(buffer.Length) { Data = buffer };
            return new ComputeParameter(name, array, memorymode);
        }

        public void Update(ref Real[] grad, ref Real[] w, params float[] param)
        {
            if (!Initialize) { FunctionConfiguration(); InitalizeOption(w.Length); Initialize = true; }
            Real[] u = new Real[w.Length];

            var variable = new ComputeVariable();
            foreach (var item in param)
            {
                variable.Argument.Add(new ComputeVariable.ValueSet("") { Value = item });
            }
            variable.Add("grad", new RNdArray(grad), State.MemoryModeSet.ReadOnly);
            variable.Add("w", new RNdArray(w), State.MemoryModeSet.WriteOnly);
            variable.Add("u", new RNdArray(u), State.MemoryModeSet.WriteOnly);
            Do(true, variable);
            Delta = 0;
            for (int i = 0; i < grad.Length; i++) { Delta += u[i]; grad[i] = 0; }
            Delta /= (Real)grad.Length;
            w = variable.Parameter[1].Instance.Array.Data;
            BatchParameterUpdate();
        }

        protected virtual void InitalizeOption(int areasize) { }

        protected virtual void BatchParameterUpdate() { }
    }
}
