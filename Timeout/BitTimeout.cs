using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace BIT_Simulator.Timeout
{
    [MoonSharpUserData]
    public class BitTimeout
    {
        private struct PendingTask
        {
            public long ExecuteAt;
            public DynValue Callback;
        }

        private readonly List<PendingTask> _tasks = new List<PendingTask>();

        public void timeout(int delayMs, DynValue callback)
        {
            _tasks.Add(new PendingTask
            {
                ExecuteAt = Environment.TickCount + delayMs,
                Callback = callback
            });
        }

        [MoonSharpHidden]
        public void Update()
        {
            if (_tasks.Count == 0) return;

            long now = Environment.TickCount;

            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                if (now >= _tasks[i].ExecuteAt)
                {
                    var task = _tasks[i];
                    _tasks.RemoveAt(i);

                    task.Callback.Function.Call();
                }
            }
        }
    }
}