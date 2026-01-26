using BIT_Simulator.Clock;
using BIT_Simulator.Graphics;
using BIT_Simulator.Input;
using BIT_Simulator.SimLog;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIT_Simulator
{
    internal class AppInstance
    {
        public string Path;
        public Coroutine Co;
    }

    internal class AppCtx
    {
        private static List<AppInstance> appInstances = new List<AppInstance>();

        public void Init()
        {
            UserData.RegisterType<BitGraphics>();
            UserData.RegisterType<BitMouse>();
            UserData.RegisterType<BitClock>();
            Script.DefaultOptions.DebugPrint = s => Console.WriteLine(s);
        }

        public void LoadApp(string appPath)
        {
            if (!File.Exists(appPath))
            {
                SIMLOG.Error($"App not found: {appPath}");
                return;
            }

            string appName =
                Path.GetDirectoryName(appPath)!
                 .Replace("apps/", "")
                 .Replace("apps\\", "");

            try
            {
                string code = File.ReadAllText(appPath);
                Script script = new Script();
                script.Globals["bit_graphics"] = new BitGraphics();
                script.Globals["bit_mouse"] = new BitMouse();
                script.Globals["bit_clock"] = new BitClock();

                DynValue func = script.LoadString(code);
                DynValue coDyn = script.CreateCoroutine(func);

                AppInstance instance = new AppInstance
                {
                    Path = appPath,
                    Co = coDyn.Coroutine
                };

                appInstances.Add(instance);

                SIMLOG.Info($"Loaded app: {appName}");
            }
            catch (Exception ex)
            {
                SIMLOG.Error($"Failed to load app: {appName} - {ex.Message}");
                return;
            }
        }

        public void UpdateApps()
        {
            for (int i = appInstances.Count - 1; i >= 0; i--)
            {
                Coroutine co = appInstances[i].Co;

                if (co.State == CoroutineState.Dead)
                {
                    appInstances.RemoveAt(i);
                    continue;
                }

                co.Resume();
            }
        }

        public void UnloadApp(string appPath)
        {
            string appName =
                Path.GetDirectoryName(appPath)!
                  .Replace("apps/", "")
                  .Replace("apps\\", "");

            for (int i = appInstances.Count - 1; i >= 0; i--)
            {
                if (appInstances[i].Path == appPath)
                {
                    appInstances.RemoveAt(i);
                    SIMLOG.Info($"Unloaded app: {appName}");
                }
            }
        }
    }
}
