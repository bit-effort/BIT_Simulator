using BIT_Simulator.Graphics;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BIT_Simulator
{
    internal class BootSim
    {
        public static bool isBooting { get; private set; } = false;

        static Texture2D bootLogo = Raylib.LoadTexture("Data/OS/boot.png");
        static Texture2D spinner = Raylib.LoadTexture("Data/OS/spinner.png");

        static float rot = 0f;

        public static async void StartBoot(Action? onTaskDone, bool skip)
        {
            isBooting = true;
            await Task.Delay(skip ? 100 : 2500);
            isBooting = false;
            onTaskDone?.Invoke();
        }

        public static void DrawBoot()
        {
            if (!isBooting) return;

            Raylib.DrawTexture(bootLogo, Raylib.GetScreenWidth() / 2 - bootLogo.Width / 2, Raylib.GetScreenHeight() / 2 - 100, Color.White);

            var src = new Rectangle(0, 0, spinner.Width, spinner.Height);
            var destX = Raylib.GetScreenWidth() / 2f;
            var destY = Raylib.GetScreenHeight() / 2f + 175f;
            var dest = new Rectangle(destX, destY, spinner.Width, spinner.Height);
            var origin = new Vector2(spinner.Width / 2f, spinner.Height / 2f);
            Raylib.DrawTexturePro(spinner, src, dest, origin, rot, Color.White);

            Raylib.DrawTextEx(BitFont.GetFont("inter"), "Starting simulator...", new Vector2(Raylib.GetScreenWidth() / 2 - 75, Raylib.GetScreenHeight() / 2 + 200), 20, 1, Color.White);

            rot += 4f;
        }
    }
}
