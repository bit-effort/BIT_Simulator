using BIT_Simulator.SimLog;
using System.Threading.Tasks;

namespace BIT_Simulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SIMLOG.Info("Starting BIT Simulator...");

            OS os = new OS();
            os.SkipBootScreen = false;
            os.Init();
            os.Run();
        }
    }
}
