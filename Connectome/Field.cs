using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome
{
    public delegate void SignalUpdateEventHandler(FieldState state);
    class Field
    {
        public SignalUpdateEventHandler SignalUpdateEvent { get; set; }

        private class Cell : CellCore
        {
            public IgnitionState State { get; set; }

            public double Energy { get; set; }

            public double AxsonLength { get; private set; }

            public List<int> AxsonConnectedID { get; set; } = new List<int>();

            public void AddAxson(int ID) { AxsonConnectedID.Add(ID); }

            public Cell(Location loc, double length) : base(loc)
            {
                State = IgnitionState.Stable;
                AxsonLength = length;
            }
        }

        private class SignalCell : CellCore
        {
            public double AxsonLength { get; private set; }

            public List<int> AxsonConnectedID { get; set; } = new List<int>();

            public void AddAxson(int ID) { AxsonConnectedID.Add(ID); }

            public SignalCell(Location loc, double length) : base(loc)
            {
                AxsonLength = length;
            }
        }

        #region Property
        private double stepTime = 0;
        public double StepTime { get { return Math.Max(1, stepTime); } }

        private double totalStepTime = 0;
        public double TotalStepTime { get { return Math.Max(1, totalStepTime); } }

        private long stepCount = 0;
        public long StepCount { get { return stepCount; } }

        public double Energy { get; set; } = 0;

        private List<CellCore> Cells { get; set; } = new List<CellCore>();
        private List<SignalCell> Signals { get; set; } = new List<SignalCell>();
        public double Signal(int index)
        {
            return Signals[index].Signal;
        }

        private int NeuronCount { get; set; }

        private List<Receptor.Receptor> Receptors { get; set; } = new List<Receptor.Receptor>();

        private Calculation.InitializeNeuron Parameter { get; set; }
        #endregion

        public Field(Calculation.InitializeNeuron parameter)
        {
            Parameter = parameter;
            NeuronCount = parameter.NeuronSources.Count;
            for (int i = 0; i < NeuronCount; i++)
            {
                Cells.Add(new Cell(parameter.NeuronSources[i].Location, parameter.NeuronSources[i].AxsonLength));
            }
        }

        #region Buffer
        private RNdArray cellValue { get; set; }
        private RNdArray cellSignal { get; set; }
        private RNdArray cellActivity { get; set; }
        private RNdArray cellEnergy { get; set; }
        private RNdArray cellState { get; set; }
        private RNdArray connectWeight { get; set; }

        private RNdArray axsonConnectCount { get; set; }
        private RNdArray axsonConnectStartIndex { get; set; }
        private RNdArray axsonConnectMatrix { get; set; }
        #endregion

        public List<CellState> GetState()
        {
            List<CellState> res = new List<Connectome.CellState>();
            var cls = Cells.FindAll(x => x is Cell);
            foreach (var item in cls)
            {
                var cell = item as Cell;
                res.Add(new Connectome.CellState(cell.Location, cell.Value, cell.Signal, cell.Energy, cell.State));
            }
            return res;
        }

        public void AddReceptor(Receptor.Receptor receptor)
        {
            Receptors.Add(receptor);
            for (int i = 0; i < receptor.Cells.Count; i++)
            {
                Cells.Add(receptor.Cells[i]);
            }
        }

        public void SetSignalLocation(Location location, double areaLength)
        {
            var signal = new SignalCell(location, areaLength);
            for (int i = 0; i < Cells.Count; i++)
            {
                if (location.DistanceTo(Cells[i].Location) < areaLength)
                {
                    signal.AxsonConnectedID.Add(Cells[i].ID);
                }
            }
            Signals.Add(signal);
        }

        public void Confirm()
        {
            Parallel.For(0, NeuronCount, i =>
            {
                var cell = Cells[i] as Cell;
                for (int j = 0; j < Cells.Count; j++)
                {
                    if (i == j) { continue; }
                    if (cell.Location.DistanceTo(Cells[j].Location) < cell.AxsonLength)
                    {
                        if (Cells[j] is Cell)
                        {
                            if ((Cells[j] as Cell).AxsonConnectedID.Contains(cell.ID))
                            {
                                continue;
                            }
                        }
                        cell.AddAxson(Cells[j].ID);
                    }
                }
            });

            Cells.Sort((c1, c2) =>
            {
                if (c1.ID > c2.ID)
                {
                    return 1;
                }
                else if (c1.ID < c2.ID)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            });

            int len = Cells.Sum(x => (x is Cell) ? (x as Cell).AxsonConnectedID.Count : 0);
            axsonConnectMatrix = new RNdArray(len);
            axsonConnectCount = new RNdArray(Cells.Count);
            axsonConnectStartIndex = new RNdArray(Cells.Count);
            int index = 0;
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i] is Cell)
                {
                    var cell = (Cells[i] as Cell);
                    axsonConnectCount[i] = cell.AxsonConnectedID.Count;
                    for (int j = 0; j < cell.AxsonConnectedID.Count; j++)
                    {
                        axsonConnectMatrix[index] = cell.AxsonConnectedID[j];
                        index++;
                    }
                }
                else
                {
                    axsonConnectCount[i] = 0;
                }
            }
            Parallel.For(0, Cells.Count, i =>
            {
                axsonConnectStartIndex[i] = Gpgpu.FunctionBase.StartPosition(i, new Components.GPGPU.Function.ComputeParameter("", axsonConnectCount, State.MemoryModeSet.WriteOnly));
            });

            var random = new Random();
            cellValue = new RNdArray(Cells.Count);
            cellSignal = new RNdArray(Cells.Count);
            cellActivity = new RNdArray(Cells.Count);
            cellState = new RNdArray(Cells.Count);
            connectWeight = new RNdArray(axsonConnectMatrix.Length);
            cellEnergy = new RNdArray(Cells.Count);

            Parallel.For(0, connectWeight.Length, i =>
            {
                connectWeight[i] = 1;// random.NextDouble();
            });
            var weightInitialize = new Gpgpu.Function.WeightInitialize();
            weightInitialize.FunctionConfiguration();
            Components.GPGPU.ComputeVariable variable;
            variable = new Components.GPGPU.ComputeVariable();
            variable.Add("ConnectWeight", connectWeight, State.MemoryModeSet.WriteOnly);
            variable.Add("AxsonConnectCount", axsonConnectCount, State.MemoryModeSet.ReadOnly);
            variable.Add("axsonConnectStartIndex", axsonConnectStartIndex, State.MemoryModeSet.ReadOnly);
            variable.Add("AxsonConnectMatrix", axsonConnectMatrix, State.MemoryModeSet.ReadOnly);
            variable.Argument.Add(new Components.GPGPU.ComputeVariable.ValueSet("CellCount") { Value = cellValue.Length });
            variable.Argument.Add(new Components.GPGPU.ComputeVariable.ValueSet("NeuronCount") { Value = NeuronCount });
            weightInitialize.Do(false, variable);

            foreach (var item in Receptors)
            {
                item.Start();
            }

            DoProcess();
        }

        private void DoProcess()
        {
            Energy = NeuronCount / 2;
            new System.Threading.Thread(() =>
            {
                double prevEnergy = Energy;
                var resValue = new RNdArray(Cells.Count);
                var resSignal = new RNdArray(Cells.Count);
                var resActivity = new RNdArray(Cells.Count);
                var resState = new RNdArray(Cells.Count);
                Components.GPGPU.ComputeVariable variable;
                var function = new Gpgpu.Function.FieldUpdateStep();
                function.FunctionConfiguration();
                while (!CoreObject.IsTerminate)
                {
                    Energy = 1000;
                    float d_energy = 1;// (float)(Energy - prevEnergy);

                    DateTime start = DateTime.Now;
                    cellValue.CopyBy(Cells.Select(x => x.Value).ToArray());
                    cellSignal.CopyBy(Cells.Select(x => x.Signal).ToArray());
                    cellActivity.CopyBy(Cells.Select(x => (x is Cell) ? (Real)(x as Cell).Activity : x.Signal).ToArray());
                    cellState.CopyBy(Cells.Select(x => (x is Cell) ? (Real)(int)(x as Cell).State : 1).ToArray());

                    variable = new Components.GPGPU.ComputeVariable();
                    variable.Add("cellValue", cellValue, State.MemoryModeSet.ReadOnly);
                    variable.Add("cellSignal", cellSignal, State.MemoryModeSet.ReadOnly);
                    variable.Add("cellActivity", cellActivity, State.MemoryModeSet.ReadOnly);
                    variable.Add("cellState", cellState, State.MemoryModeSet.ReadOnly);
                    variable.Add("connectWeight", connectWeight, State.MemoryModeSet.ReadOnly);
                    variable.Add("cellEnergy", cellEnergy, State.MemoryModeSet.WriteOnly);
                    variable.Add("axsonConnectCount", axsonConnectCount, State.MemoryModeSet.ReadOnly);
                    variable.Add("axsonConnectStartIndex", axsonConnectStartIndex, State.MemoryModeSet.ReadOnly);
                    variable.Add("axsonConnectMatrix", axsonConnectMatrix, State.MemoryModeSet.ReadOnly);
                    variable.Add("resValue", resValue, State.MemoryModeSet.WriteOnly);
                    variable.Add("resSignal", resSignal, State.MemoryModeSet.WriteOnly);
                    variable.Add("resActivity", resActivity, State.MemoryModeSet.WriteOnly);
                    variable.Add("resState", resState, State.MemoryModeSet.WriteOnly);
                    variable.Argument.Add(new Components.GPGPU.ComputeVariable.ValueSet("CellCount") { Value = cellValue.Length });
                    variable.Argument.Add(new Components.GPGPU.ComputeVariable.ValueSet("NeuronCount") { Value = NeuronCount });
                    variable.Argument.Add(new Components.GPGPU.ComputeVariable.ValueSet("Energy") { Value = Energy });
                    variable.Argument.Add(new Components.GPGPU.ComputeVariable.ValueSet("dEnergy") { Value = d_energy });

                    function.Do(false, variable);

                    #region /* need to GPGPU */
                    foreach (var item in Signals)
                    {
                        item.Signal = 0;
                        foreach (var id in item.AxsonConnectedID)
                        {
                            item.Signal += Cells[id].Value;
                        }
                        item.Signal /= item.AxsonConnectedID.Count;
                    }
                    #endregion
                    SignalUpdateEvent?.Invoke(new FieldState()
                    {
                        Energy = this.Energy,
                        Signals = new List<double>(Signals.Select(x => (double)x.Signal).ToArray()),
                        Locations = new List<Location>(Signals.Select(x => x.Location).ToArray()),
                    });

#if false
                    Parallel.For(0, NeuronCount, i =>
                    {
#else
                    for (int i = 0; i < NeuronCount; i++)
                    {
#endif
                        if (Cells[i] is Cell)
                        {
                            var cell = (Cells[i] as Cell);
                            cell.Value = resValue[i];
                            cell.Signal = resSignal[i];
                            cell.Activity = resActivity[i];
                            cell.State = (Cell.IgnitionState)Enum.ToObject(typeof(Cell.IgnitionState), (int)resState[i]);
                            cell.Energy = cellEnergy[i];
                        }
                    }
#if false
                    );
#endif
                    stepCount++;
                    stepTime = (stepTime + function.StepElapsedSpan.TotalMilliseconds) / 2;
                    totalStepTime = (totalStepTime + (DateTime.Now - start).TotalMilliseconds) / 2;
                    System.Threading.Thread.Sleep(CoreObject.Interval);
                }
            }).Start();
        }
    }
}
