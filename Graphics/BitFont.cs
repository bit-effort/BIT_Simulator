using BIT_Simulator.SimLog;
using MoonSharp.Interpreter;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIT_Simulator.Graphics
{
    internal struct BitFontImpl
    {
        public string Name;
        public Font Data;

    }

    internal class BitFont
    {
        static List<BitFontImpl> Fonts = new List<BitFontImpl>();

        internal static void LoadInDefaultSizes()
        {
            SIMLOG.Info("Loading fonts...");
         
            var inter = Raylib.LoadFontEx($"Data/OS/Fonts/Inter_28pt-Regular.ttf", 28, null, 0);
            var interBold = Raylib.LoadFontEx($"Data/OS/Fonts/Inter_28pt-SemiBold.ttf", 28, null, 0);

            Raylib.SetTextureFilter(inter.Texture, TextureFilter.Trilinear);
            Raylib.SetTextureFilter(interBold.Texture, TextureFilter.Trilinear);

            Fonts.Add(new BitFontImpl
            {
                Name = "inter",
                Data = inter
            });
           
            Fonts.Add(new BitFontImpl
            {
                Name = "inter_bold",
                Data = interBold
            });
        }

        internal static Font GetFont(string name)
        {
            return Fonts.Find(f => f.Name == name).Data;
        }
    }
}
