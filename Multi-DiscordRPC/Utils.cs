using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_DiscordRPC
{
    class Utils
    {
        public static void PrettyPrint(string text, ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black, bool HideConsole = false)
        {
            if (HideConsole) return;
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
