using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PraTaiko
{
    public static class PlayConfig
    {
        static bool Save = false;
        public static bool Auto { get; private set; } = false;
        public static float Speed { get; private set; } = 1;
        public static bool NoteOff { get; private set; } = false;

        public static int ReplacePer { get; private set; } = 0;
        public static string ReplaceSeed = null;

        public static void ChangeReplacePer()
        {
            if (ReplacePer < 20)
            {
                ReplacePer = 20;
            }
            else if (ReplacePer < 50)
            {
                ReplacePer = 50;
            }
            else if (ReplacePer < 100)
            {
                ReplacePer = 100;
            }
            else
            {
                ReplacePer = 0;
            }
        }
        public static void ChangeReplacePer(int sp)
        {
            ReplacePer = sp;
        }
        public static void ChangeAuto()
        {
            Auto = !Auto;
        }
        public static void ChangeAuto(bool a)
        {
            Auto = a;
        }
        public static void ChangeSpeed()
        {
            Speed = (int)Speed;
            Speed++;
            if (Speed <= 0)
            {
                Speed = 1;
            }
            if (Speed >= 5)
            {
                Speed = 1;
            }
        }
        public static void ChangeSpeed(float s)
        {
            if (s > 0)
            {
                Speed = s;
            }
        }
        public static void ChangeNoteOff()
        {
            NoteOff = !NoteOff;
        }
        public static void ChangeNoteOff(bool n)
        {
            NoteOff = n;
        }

        public static int judgeRangeGreat;
        public static int judgeRangeGood;
        public static int judgeRangeBad;        
    }
}
