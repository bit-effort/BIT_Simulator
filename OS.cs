using BIT_Simulator.Graphics;
using BIT_Simulator.SimLog;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BIT_Simulator
{
    internal class OS
    {
        public bool SkipBootScreen = false;

        public static AppCtx ?appCtx;

        static string shellPath = "Apps/shell/shell.lua";

        Texture2D backgroundImage;

        public void Init()
        {
            Raylib.SetTraceLogLevel(TraceLogLevel.Error | TraceLogLevel.Warning | TraceLogLevel.Fatal);
            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
            Raylib.InitWindow(1920, 1080, "BIT Simulator");
            Raylib.SetExitKey(KeyboardKey.Null);
            Raylib.SetTargetFPS(60);

            BitFont.LoadInDefaultSizes();

            BootSim.StartBoot(LoadShell, SkipBootScreen);
            backgroundImage = Raylib.LoadTexture("Data/OS/bg.jpg");

            appCtx = new AppCtx();
            appCtx.Init();

            SIMLOG.InitializeSignals(appCtx);
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

            appCtx.SetAppLayer(shellPath, 101);
        }

        public void Run()
        {
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                if (!BootSim.isBooting)
                {
                    Raylib.DrawTexturePro(backgroundImage, new Rectangle(0, 0, 1280, 720), new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), new Vector2(0, 0), 0, Color.White);
                    appCtx?.UpdateApps();
                }

                BootSim.DrawBoot();

                Raylib.DrawFPS(10, 40);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        internal static void ReloadShell()
        {
            SIMLOG.Info("Reloading shell...");

            appCtx?.UnloadApp(shellPath);
            appCtx?.LoadApp(shellPath);
        }
    }
}
