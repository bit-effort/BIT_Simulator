using BIT_Simulator.Clock;
using BIT_Simulator.FileSystem;
using BIT_Simulator.Graphics;
using BIT_Simulator.Input;
using BIT_Simulator.SimLog;
using BIT_Simulator.Timeout;
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
        private static List<string> appsToRemove = new List<string>();

        private List<DynValue> signalListeners = new List<DynValue>();

        private BitGraphics bitGraphics = new BitGraphics();
        private BitMouse bitMouse = new BitMouse();
        private BitKeyboard bitKeyboard = new BitKeyboard();
        private BitClock bitClock = new BitClock();
        private BitFileSystem bitFileSystem = new BitFileSystem();
        private BitTimeout bitTimeout = new BitTimeout();


        [MoonSharpHidden]
        public void Init()
        {
            UserData.RegisterType<BitGraphics>();
            UserData.RegisterType<BitMouse>();
            UserData.RegisterType<BitKeyboard>();
            UserData.RegisterType<BitClock>();
            UserData.RegisterType<BitFileSystem>();
            UserData.RegisterType<AppCtx>();
            UserData.RegisterType<BitTimeout>();

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

                script.Globals["bit_graphics"] = bitGraphics;
                script.Globals["bit_mouse"] = bitMouse;
                script.Globals["bit_keyboard"] = bitKeyboard;
                script.Globals["bit_clock"] = bitClock;
                script.Globals["bit_filesystem"] = bitFileSystem;
                script.Globals["bit_timeout"] = bitTimeout;

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

                EmitSignal("__system_app_load_requested", appPath);
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

            // Process deferred removals after all apps have been updated
            ProcessDeferredRemovals();

            bitTimeout.Update();
        }

        [MoonSharpHidden]
        private void ProcessDeferredRemovals()
        {
            if (appsToRemove.Count == 0)
                return;

            foreach (string appPath in appsToRemove)
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

            appsToRemove.Clear();
        }

        public void UnloadApp(string appPath)
        {
            string target = Normalize(appPath);

            if (!appsToRemove.Contains(target))
            {
                appsToRemove.Add(target);
                EmitSignal("__system_app_unload_requested", target);
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

        public void SetAppLayer(string appPath, int layer)
        {
            string target = Normalize(appPath);
            var app = appInstances.Find(a => Normalize(a.Path) == target);
            if (app != null)
            {
                app.Layer = layer;
                SIMLOG.Info($"Set layer of {target} to {layer}");
            }
            else
            {
                SIMLOG.Error($"SetAppLayer failed: App not found - {target}");
            }
        }

        public void ConnectSignalListener(DynValue callback)
        {
            if (callback.Type == DataType.Function)
            {
                signalListeners.Add(callback);
                SIMLOG.Info("New signal listener connected.");
            }
            else
            {
                SIMLOG.Error("Connect failed: Argument must be a function.");
            }
        }
        public void ConnectSignalListenerBackend(Action<string, DynValue> callback)
        {
            var dynCallback = DynValue.NewCallback((context, args) =>
            {
                if (args.Count >= 2)
                {
                    callback(args[0].String, args[1]);
                }
                else if (args.Count == 1)
                {
                    callback(args[0].String, DynValue.Nil);
                }

                return DynValue.Nil;
            });

            signalListeners.Add(dynCallback);
            SIMLOG.Info("New backend signal listener connected.");
        }

        public void EmitSignal(string signalName, string data)
        {
            for (int i = signalListeners.Count - 1; i >= 0; i--)
            {
                var listener = signalListeners[i];
                if (listener == null) continue;

                try
                {
                    // If it's a native Lua function
                    if (listener.Type == DataType.Function)
                    {
                        listener.Function.Call(signalName, data);
                    }
                    // If it's a C# callback created via NewCallback
                    else if (listener.Type == DataType.ClrFunction)
                    {
                        listener.Callback.Invoke(null, new DynValue[] {
                            DynValue.NewString(signalName),
                            DynValue.NewString(data)
                        });
                    }
                }
                catch (Exception ex)
                {
                    SIMLOG.Error($"Signal Error: {ex.Message}");
                    signalListeners.RemoveAt(i);
                }
            }
        }
    }
}