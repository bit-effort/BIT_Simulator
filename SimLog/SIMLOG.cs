using MoonSharp.Interpreter;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;

namespace BIT_Simulator.SimLog
{
    internal class SIMLOG
    {
        private class LogLine
        {
            public string Text;
            public ConsoleColor Foreground;
            public ConsoleColor Background;

            public LogLine(string text, ConsoleColor fg, ConsoleColor bg)
            {
                Text = text;
                Foreground = fg;
                Background = bg;
            }
        }

        private static readonly List<LogLine> logLines = new List<LogLine>();
        private static readonly string logFilePath = "simulator.log";

        static SIMLOG()
        {
            File.WriteAllText(logFilePath, "");
        }

        public static void InitializeSignals(AppCtx context)
        {
            context.ConnectSignalListenerBackend((name, data) =>
            {
                if (name == "__system-log-request-full")
                {
                    string fullLog = string.Join("\n", logLines.ConvertAll(line => line.Text));
                    context.EmitSignal("__system-log-recieved-full", fullLog);
                }
            });

            SIMLOG.Info("SIMLOG Signal Listener Attached.");
        }

        private static void AddLog(string text, ConsoleColor fg, ConsoleColor bg)
        {
            logLines.Add(new LogLine(text, fg, bg));
            SaveLog(text);

            OS.appCtx?.EmitSignal("__system-log-recieved", text);
        }

        private static void SaveLog(string msg)
        {
            File.AppendAllTextAsync(logFilePath, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + msg + Environment.NewLine);
        }

        public static void Info(string msg) => AddLog("[INFO] " + msg, ConsoleColor.White, ConsoleColor.DarkBlue);
        public static void Warning(string msg) => AddLog("[WARN] " + msg, ConsoleColor.White, ConsoleColor.DarkYellow);
        public static void Error(string msg) => AddLog("[ERROR] " + msg, ConsoleColor.White, ConsoleColor.DarkRed);
    }
}
