using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpkNeu
{
    public static class Core
    {
        public static bool IsTerminate { get; private set; } = false;
        public static bool IsPause { get; private set; } = false;

        public static void Terminate()
        {
            IsTerminate = true;
        }

        public static void Pause()
        {
            if (IsPause)
            {
                IsPause = false;
            }
            else
            {
                IsPause = true;
            }
        }

        private static Field field;
        private static SensoryField receptor;
        public static void FieldTest(Cell.CellBase typebase, int count, int timing, Field.IgnitionEventHandler handler = null)
        {
            field = new SpkNeu.Field(typebase, count);
            if (handler != null)
            {
                field.Ignition += handler;
            }
            receptor = new SpkNeu.SensoryField(10);

            field.SetReceotor(receptor);

            receptor.Start(timing);
            field.Start(timing);
        }
        public static Cell.CellBase HeadCell { get { return field.Neurons[0]; } }
    }
}
