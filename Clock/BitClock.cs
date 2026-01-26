using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIT_Simulator.Clock
{
    [MoonSharpUserData]
    internal class BitClock
    {
        public string get_time_hhmm()
        {
            return DateTime.Now.ToString("HH:mm");
        }
        public string get_time_hhmmss()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        public string get_date_ddmmyyyy()
        {
            return DateTime.Now.ToString("dd/MM/yyyy");
        }
    }
}
