using MagratheaCore;
using System;

namespace MagratheaWindows
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class ProgramMain
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new MagratheaGame())
            {
                game.Run();
            }
        }
    }
}
