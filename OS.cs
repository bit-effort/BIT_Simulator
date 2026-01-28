using BIT_Simulator.Graphics;
using BIT_Simulator.SimLog;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIT_Simulator
{
    internal class OS
    {
        public bool SkipBootScreen = false;

        static AppCtx ?appCtx;

        static string shellPath = "Apps/shell/shell.lua";

        Texture2D backgroundImage;

        public void Init()
        {
            Raylib.SetTraceLogLevel(TraceLogLevel.Error | TraceLogLevel.Warning | TraceLogLevel.Fatal);
            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
            Raylib.InitWindow(1280, 720, "BIT Simulator");
            Raylib.SetExitKey(KeyboardKey.Null);
            Raylib.SetTargetFPS(60);

            BitFont.LoadInDefaultSizes();

            BootSim.StartBoot(LoadShell, SkipBootScreen);
            backgroundImage = Raylib.LoadTexture("Data/OS/bg.jpg");

            appCtx = new AppCtx();
            appCtx.Init();
        }

        void LoadShell()
        {
            if (appCtx == null)
            {
                SIMLOG.Error("AppRunner not initialized! Cannot load shell!");
                return;
            }

            // Load the shell app
            SIMLOG.Info("Loading shell...");
            appCtx.LoadApp(shellPath);
            appCtx.SetLayer(101, shellPath);
        }

        public void Run()
        {
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                if (!BootSim.isBooting)
                {
                    Raylib.DrawTexture(backgroundImage, 0, 0, Color.White);
                    appCtx?.UpdateApps();
                }

                BootSim.DrawBoot();

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        internal static async void ReloadShell()
        {
            SIMLOG.Info("Reloading shell...");

            appCtx?.UnloadApp(shellPath);
            await Task.Delay(100);
            appCtx?.LoadApp(shellPath);
        }
    }
}
