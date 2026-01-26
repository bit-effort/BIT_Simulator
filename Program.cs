using BIT_Simulator.SimLog;
using System.Threading.Tasks;

namespace BIT_Simulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SIMLOG.Info("Starting BIT Simulator...");

            SIMLOG.SetBottomBarLine(0, "=====================");
            SIMLOG.SetBottomBarLine(1, "Refresh Shell: [F1]");
            SIMLOG.SetBottomBarLine(2, "=====================");

            SIMLOG.StartListeningToToolbarCalls();

            OS os = new OS();
            os.SkipBootScreen = false;
            os.Init();
            os.Run();
        }
    }
}
