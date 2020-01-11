using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu.Cell
{
    public abstract class CellBase : CellCore, IComparable
    {
        public enum State
        {
            Stable,
            Ignition,
            OverShoot,
            Cooling,
        }
        public State state { get; protected set; }

        public bool IsIgnition { get; private set; } = false;

        public static Location ReferencePoint { get; set; } = new Location(0, 0, 0);

        protected List<CellCore> Glial { get; set; } = new List<CellCore>();
        protected List<CellCore> Axson { get; set; } = new List<CellCore>();
        public int GlialCount { get { return Glial.Count; } }
        public int AxsonCount { get { return Axson.Count; } }

        protected bool HasReceptor
        {
            get { return Axson.FindAll(x => x is SpkNeu.Cell.Receptor).Count > 0; }
        }

        public CellBase()
        {
            ID = idseed;
            idseed++;
        }

        public void SetLocation(double area)
        {
            Location = new Location(random, area);
        }

        public void Add(CellCore cell, double probability = 0.5, bool skipglial = false)
        {
            if (cell is CellBase)
            {
                CellBase _cell = cell as CellBase;
                if (!Glial.Contains(_cell))
                {
                    Glial.Add(_cell);
                }
                if (!Axson.Contains(_cell)
                    //&& !_cell.Axson.Contains(this)
                    )
                {
                    if (random.NextDouble() < probability)
                    {
                        Axson.Add(_cell);
                        Connection(_cell.Signal);
                    }
                }
            }
            else if (cell is Receptor)
            {
                Receptor _cell = cell as Receptor;
                if (!Glial.Contains(_cell))
                {
                    Glial.Add(_cell);
                }
                if (!skipglial)
                {
                    foreach (var item in Glial)
                    {
                        if (item is CellBase)
                        {
                            CellBase glialcell = item as CellBase;
                            if (!glialcell.Glial.Contains(cell))
                            {
                                glialcell.Add(cell, 1, true);
                            }
                        }
                    }
                    probability /= 10;
                }
                if (!Axson.Contains(_cell))
                {
                    if (random.NextDouble() < probability)
                    {
                        Axson.Add(_cell);
                        Connection(_cell.Signal);
                    }
                }
            }
        }
        public void Remove(CellCore cell, double probability = 0.5)
        {
            if (HasReceptor && cell is Receptor)
            {
                probability *= 1;
            }
            int index = Axson.IndexOf(cell);
            if (index >= 0)
            {
                //if (Probabirity(probability))
                {
                    Disconnection(index);
                    Axson.Remove(cell);
                }
            }
        }

        public bool Ignition()
        {
            localSignal = ignition(
                calculateUpdateQuantity(new List<double>(Axson.Select(x => x.Signal).ToArray()),
                                        new List<bool>(Axson.Select(x => x.GetType() == typeof(Receptor)).ToArray())), localSignal);
            if (localSignal > 1)
            {
                IsIgnition = true;
            }
            else
            {
                IsIgnition = false;
            }
            return IsIgnition;
        }

        public bool Update()
        {
            Signal = IsIgnition ? 1 : 0;
            CellBaseTermIgnitionRatio(IsIgnition);

            if (AxsonCount == 0)
            {
                return false;
            }
            return true;
        }

        public abstract void Connection(double defaultvalue = 0);
        protected abstract void Disconnection(int index);

        protected abstract double calculateUpdateQuantity(List<double> signals, List<bool> isreceptor);
        protected abstract double ignition(double updateQuantity, double localSignal);

        public int CompareTo(object obj)
        {
            CellBase cell = obj as CellBase;
            double l1 = Location.DistanceTo(ReferencePoint);
            double l2 = cell.Location.DistanceTo(ReferencePoint);
            if (l1 > l2)
            {
                return 1;
            }
            else if (l2 > l1)
            {
                return -1;
            }
            return 0;
        }
    }
}
