using MoonSharp.Interpreter;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIT_Simulator.Input
{
    [MoonSharpUserData]
    internal class BitMouse
    {
        public int get_x()
        {
            return Raylib.GetMouseX();
        }
        public int get_y()
        {
            return Raylib.GetMouseY();
        }

        public bool is_button_clicked(int button)
        {
            return Raylib.IsMouseButtonPressed((MouseButton)button);
        }
    }
}
    