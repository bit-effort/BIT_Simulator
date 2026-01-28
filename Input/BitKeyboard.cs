using MoonSharp.Interpreter;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIT_Simulator.Input
{
    [MoonSharpUserData]
    internal class BitKeyboard
    {
        private static readonly Dictionary<string, KeyboardKey> KeyMap = new Dictionary<string, KeyboardKey>
        {
            // Letters
            { "a", KeyboardKey.A }, { "b", KeyboardKey.B}, { "c", KeyboardKey.C},
            { "d", KeyboardKey.D }, { "e", KeyboardKey.E}, { "f", KeyboardKey.F},
            { "g", KeyboardKey.G }, { "h", KeyboardKey.H}, { "i", KeyboardKey.I},
            { "j", KeyboardKey.J }, { "k", KeyboardKey.K}, { "l", KeyboardKey.L},
            { "m", KeyboardKey.M }, { "n", KeyboardKey.N}, { "o", KeyboardKey.O},
            { "p", KeyboardKey.P }, { "q", KeyboardKey.Q }, { "r", KeyboardKey.R},
            { "s", KeyboardKey.S }, { "t", KeyboardKey.T }, { "u", KeyboardKey.U},
            { "v", KeyboardKey.V }, { "w", KeyboardKey.W}, { "x", KeyboardKey.X},
            { "y", KeyboardKey.Y}, { "z", KeyboardKey.Z },

            // Numbers
            { "0", KeyboardKey.Zero }, { "1", KeyboardKey.One}, { "2", KeyboardKey.Two },
            { "3", KeyboardKey.Three }, { "4", KeyboardKey.Four}, { "5", KeyboardKey.Five },
            { "6", KeyboardKey.Six}, { "7", KeyboardKey.Seven }, { "8", KeyboardKey.Eight },
            { "9", KeyboardKey.Nine},

            // Special Keys
            { "space", KeyboardKey.Space },
            { "enter", KeyboardKey.Enter },
            { "escape", KeyboardKey.Escape },
            { "backspace", KeyboardKey.Backspace },
            { "tab", KeyboardKey.Tab },
    
            // Modifiers
            { "left_alt", KeyboardKey.LeftAlt },
            { "right_alt", KeyboardKey.RightAlt},
            { "left_control", KeyboardKey.LeftControl},
            { "right_control", KeyboardKey.RightControl},
            { "left_shift", KeyboardKey.LeftShift},
            { "right_shift", KeyboardKey.RightShift},

            // Arrows
            { "up", KeyboardKey.Up },
            { "down", KeyboardKey.Down },
            { "left", KeyboardKey.Left },
            { "right", KeyboardKey.Right }
        };

        private Dictionary<string, float> _keyTimers = new Dictionary<string, float>();
        private const float InitialDelay = 0.5f; // Half a second before repeating starts
        private const float RepeatInterval = 0.05f; // Speed of repeats once started

        public bool is_key_pressed(string key)
        {
            var k = MapKey(key);
            return Raylib.IsKeyPressed(k);
        }
        public bool is_key_pressed_repeat(string key)
        {
            var k = MapKey(key);

            // 1. Initial Press
            if (Raylib.IsKeyPressed(k))
            {
                _keyTimers[key] = -InitialDelay;
                return true;
            }

            if (Raylib.IsKeyDown(k))
            {
                _keyTimers[key] += Raylib.GetFrameTime();

                if (_keyTimers[key] >= RepeatInterval)
                {
                    _keyTimers[key] = 0;
                    return true;
                }
            }
            else
            { 
                // Clear timer when key is released
                _keyTimers.Remove(key);
            }

            return false;
        }

        public bool is_key_released(string key)
        {
            var k = MapKey(key);
            return Raylib.IsKeyReleased(k);
        }

        public bool is_key_down(string key)
        {
            var k = MapKey(key);
            return Raylib.IsKeyDown(k);
        }

        public bool is_any_key_pressed()
        {
            return Raylib.IsKeyPressed(KeyboardKey.Null) == false;
        }

        public string get_pressed_char()
        {
            int charCode = Raylib.GetCharPressed();

            if (charCode == 0)
            {
                return "";
            }

            return char.ConvertFromUtf32(charCode);
        }

        private KeyboardKey MapKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return KeyboardKey.Null;

            // Normalize to lowercase so "A" and "a" both work
            string normalizedKey = key.ToLower();

            if (KeyMap.TryGetValue(normalizedKey, out var result))
            {
                return result;
            }

            return KeyboardKey.Null;
        }
    }
}
