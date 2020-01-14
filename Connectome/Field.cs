using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace Connectome
{
    class Field
    {
        private class Cell : CellCore
        {
            public double AxsonLength { get; private set; }

            public List<int> AxsonConnectedID { get; set; } = new List<int>();

            public void AddAxson(int ID) { AxsonConnectedID.Add(ID); }

            public Cell(Location loc, double length) : base(loc)
            {
                AxsonLength = length;
            }
        }

        #region Property
        private List<CellCore> Cells { get; set; } = new List<CellCore>();
        private int NeuronCount { get; set; }

        private List<Receptor.Receptor> Receptors { get; set; } = new List<Receptor.Receptor>();

        private Calculation.InitializeNeuron Parameter { get; set; }

        public Field(Calculation.InitializeNeuron parameter)
        {
            Parameter = parameter;
            NeuronCount = parameter.NeuronSources.Count;
            for (int i = 0; i < NeuronCount; i++)
            {
                Cells.Add(new Cell(parameter.NeuronSources[i].Location, parameter.NeuronSources[i].AxsonLength));
            }
        }
        #endregion

        #region Buffer
        private RNdArray CellValue { get; set; }
        private RNdArray ConnectWeight { get; set; }

        private RNdArray AxsonConnectCount { get; set; }
        private RNdArray AxsonConnectMatrix { get; set; }
        #endregion

        public void AddReceptor(Receptor.Receptor receptor)
        {
            Receptors.Add(receptor);
            for (int i = 0; i < receptor.Cells.Count; i++)
            {
                Cells.Add(receptor.Cells[i]);
            }
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
            AxsonConnectMatrix = new RNdArray(len);
            AxsonConnectCount = new RNdArray(Cells.Count);
            int index = 0;
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i] is Cell)
                {
                    var cell = (Cells[i] as Cell);
                    AxsonConnectCount[i] = cell.AxsonConnectedID.Count;
                    for (int j = 0; j < cell.AxsonConnectedID.Count; j++)
                    {
                        AxsonConnectMatrix[index] = cell.AxsonConnectedID[j];
                        index++;
                    }
                }
                else
                {
                    AxsonConnectCount[i] = 0;
                }
            }
            ConnectWeight = new RNdArray(AxsonConnectMatrix.Length);
            // ★WeightInitialize


            new System.Threading.Thread(() =>
            {
                var function = new Gpgpu.Function.FieldUpdateStep();
                function.FunctionConfiguration();
                CellValue = new RNdArray(Cells.Count);
                while (!Core.IsTerminate)
                {
                    CellValue.CopyBy(Cells.Select(x => x.Signal).ToArray());

                    Components.GPGPU.ComputeVariable variable = new Components.GPGPU.ComputeVariable();
                    variable.Add("CellValue", CellValue, State.MemoryModeSet.WriteOnly);
                    variable.Add("ConnectWeight", ConnectWeight, State.MemoryModeSet.WriteOnly);
                    variable.Add("AxsonConnectCount", AxsonConnectCount, State.MemoryModeSet.ReadOnly);
                    variable.Add("AxsonConnectMatrix", AxsonConnectMatrix, State.MemoryModeSet.ReadOnly);
                    variable.Argument.Add(new Components.GPGPU.ComputeVariable.ValueSet("CellCount") { Value = CellValue.Length });
                    variable.Argument.Add(new Components.GPGPU.ComputeVariable.ValueSet("NeuronCount") { Value = NeuronCount });

                    function.Do(false, variable);

                    Parallel.For(0, NeuronCount, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, i =>
                    {
                        if (Cells[i] is Cell)
                        {
                            Cells[i].Signal = CellValue[i];
                        }
                    });
                }
            }).Start();
        }
    }
}
