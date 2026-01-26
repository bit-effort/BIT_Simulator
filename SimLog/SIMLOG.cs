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
        private static string[] bottomBar = new string[3] { "", "", "" };
        private static readonly string logFilePath = "simulator.log";

        private static int LogAreaHeight => Console.WindowHeight - 3;

        static SIMLOG()
        {
            File.WriteAllText(logFilePath, "");
        }

        private static void AddLog(string text, ConsoleColor fg, ConsoleColor bg)
        {
            logLines.Add(new LogLine(text, fg, bg));
            SaveLog(text);

            while (logLines.Count > LogAreaHeight)
                logLines.RemoveAt(0);

            RenderConsole();
        }

        public static void SetBottomBarLine(int line, string text)
        {
            if (line < 0 || line >= 3) return;
            bottomBar[line] = text;
            RenderConsole();
        }

        private static void RenderConsole()
        {
            Console.Clear();

            for (int i = 0; i < logLines.Count; i++)
            {
                Console.SetCursorPosition(0, i);
                var line = logLines[i];
                Console.ForegroundColor = line.Foreground;
                Console.BackgroundColor = line.Background;

                string text = line.Text.PadRight(Console.WindowWidth);
                Console.Write(text);
                Console.ResetColor();
            }

            int height = Console.WindowHeight;
            for (int i = 0; i < 3; i++)
            {
                Console.SetCursorPosition(0, height - 3 + i);
                Console.BackgroundColor = ConsoleColor.DarkMagenta;
                Console.ForegroundColor = ConsoleColor.White;

                string text = bottomBar[i] ?? "";
                if (text.Length > Console.WindowWidth)
                    text = text.Substring(0, Console.WindowWidth);
                else
                    text = text.PadRight(Console.WindowWidth);

                Console.Write(text);
                Console.ResetColor();
            }
        }

        private static void SaveLog(string msg)
        {
            File.AppendAllText(logFilePath, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + msg + Environment.NewLine);
        }

        public static void Info(string msg) => AddLog("[INFO] " + msg, ConsoleColor.White, ConsoleColor.DarkBlue);
        public static void Warning(string msg) => AddLog("[WARN] " + msg, ConsoleColor.White, ConsoleColor.DarkYellow);
        public static void Error(string msg) => AddLog("[ERROR] " + msg, ConsoleColor.White, ConsoleColor.DarkRed);

        public static void StartListeningToToolbarCalls()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    ConsoleKeyInfo? input = Console.ReadKey();
                    if (input == null) continue;

                    if (input.Value.Key == ConsoleKey.F1)
                    {
                        OS.ReloadShell();
                    }

                    await Task.Delay(500);
                }
            });
        }
    }
}
