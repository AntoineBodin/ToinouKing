using System.Collections.Generic;

namespace Assets.Scripts.Helpers
{
    public static class TimerManager
    {
        static readonly List<Timer> timers = new();
        static readonly List<Timer> sweep = new();

        public static void RegisterTimer(Timer timer) => timers.Add(timer);
        public static void DeregisterTimer(Timer timer) => timers.Remove(timer);


        public static void Clear()
        {
            sweep.RefreshWith(timers);
            foreach (var timer in sweep)
            {
                timer.Dispose();
            }

            timers.Clear();
            sweep.Clear();
        }
    }
}
