using BIT_Simulator.Clock;
using BIT_Simulator.FileSystem;
using BIT_Simulator.Graphics;
using BIT_Simulator.Input;
using BIT_Simulator.SimLog;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BIT_Simulator
{
    internal class AppInstance
    {
        public string Path;
        public Coroutine Co;
        public int Layer = 0;
    }

    [MoonSharpUserData]
    internal class AppCtx
    {
        private static List<AppInstance> appInstances = new List<AppInstance>();

        public void Init()
        {
            UserData.RegisterType<BitGraphics>();
            UserData.RegisterType<BitMouse>();
            UserData.RegisterType<BitKeyboard>();
            UserData.RegisterType<BitClock>();
            UserData.RegisterType<BitFileSystem>();
            UserData.RegisterType<AppCtx>();

            Script.DefaultOptions.DebugPrint = s => SIMLOG.Info("[FROM APP] " + s);
        }


        [MoonSharpHidden]
        private string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            // Force forward slashes and remove double slashes
            return path.Replace("\\", "/").Replace("//", "/");
        }

        [MoonSharpHidden]
        private static string GetCleanAppName(string path) =>
            Path.GetDirectoryName(path)!
                .Replace("Apps/", "")
                .Replace("Apps\\", "")
                .Replace(".app", "");


        public void LoadApp(string appPath)
        {
            string normalizedPath = Normalize(appPath);

            if (!File.Exists(normalizedPath))
            {
                SIMLOG.Error($"App not found: {normalizedPath}");
                return;
            }
            if (appInstances.Exists(a => Normalize(a.Path) == normalizedPath))
            {
                SIMLOG.Warning($"App already loaded: {normalizedPath}");
                return;
            }

            string appName = GetCleanAppName(normalizedPath);

            try
            {
                string code = File.ReadAllText(normalizedPath);
                Script script = new Script();

                script.Globals["bit_graphics"] = new BitGraphics();
                script.Globals["bit_mouse"] = new BitMouse();
                script.Globals["bit_keyboard"] = new BitKeyboard();
                script.Globals["bit_clock"] = new BitClock();
                script.Globals["bit_filesystem"] = new BitFileSystem();

                // Ensure __APP_FOLDER is also normalized for the Lua side
                string folder = Path.GetDirectoryName(normalizedPath);
                script.Globals["__APP_FOLDER"] = Normalize(folder);

                script.Globals["app_ctx"] = this;

                DynValue func = script.LoadString(code);
                DynValue coDyn = script.CreateCoroutine(func);

                AppInstance instance = new AppInstance
                {
                    Path = normalizedPath,
                    Co = coDyn.Coroutine,
                    Layer = 0
                };

                appInstances.Add(instance);
                SIMLOG.Info($"Loaded app: {appName} ({normalizedPath})");
            }
            catch (Exception ex)
            {
                SIMLOG.Error($"Failed to load app: {appName} - {ex.Message}");
            }
        }

        [MoonSharpHidden]
        public void UpdateApps()
        {
            // Sort so Layer 0 is first, Layer 99 is last
            appInstances.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            // Run from 0 to Count-1 so highest layer is the LAST to resume/draw
            for (int i = 0; i < appInstances.Count; i++)
            {
                Coroutine co = appInstances[i].Co;

                if (co.State == CoroutineState.Dead)
                {
                    appInstances.RemoveAt(i);
                    i--;
                    continue;
                }

                try
                {
                    co.Resume();
                }
                catch (Exception ex)
                {
                    SIMLOG.Error($"Runtime error in {appInstances[i].Path}: {ex.Message}");
                    appInstances.RemoveAt(i);
                    i--;
                }
            }
        }

        public void UnloadApp(string appPath)
        {
            string target = Normalize(appPath);
            for (int i = appInstances.Count - 1; i >= 0; i--)
            {
                if (Normalize(appInstances[i].Path) == target)
                {
                    appInstances.RemoveAt(i);
                    SIMLOG.Info($"Unloaded app: {target}");
                }
            }
        }

        public string[] GetLoadedApps()
        {
            List<string> loadedApps = new List<string>();
            foreach (var app in appInstances)
            {
                loadedApps.Add(app.Path);
            }
            return loadedApps.ToArray();
        }

        public void SetLayer(int layer, string ctxPath)
        {
            string target = Normalize(ctxPath);
            foreach (var app in appInstances)
            {
                if (Normalize(app.Path) == target)
                {
                    app.Layer = layer;
                    break;
                }
            }
        }

        public void BringToFront(string ctxPath)
        {
            string target = Normalize(ctxPath);
            int maxLayer = 0;

            // Find current highest layer
            foreach (var app in appInstances)
            {
                if (app.Layer > maxLayer) maxLayer = app.Layer;
            }

            int nextLayer = Math.Clamp(maxLayer + 1, 0, 99);
            SetLayer(nextLayer, target);
        }

        public int GetLayer(string ctxPath)
        {
            string target = Normalize(ctxPath);
            foreach (var app in appInstances)
            {
                if (Normalize(app.Path) == target)
                {
                    return app.Layer;
                }
            }
            return -1;
        }
    }
}