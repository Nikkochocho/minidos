using System;
using Sys = Cosmos.System;
using MiniDOS.Shell;
using MiniDOS.FileSystem;

namespace MiniDOS
{
    public class Kernel : Sys.Kernel
    {
        private FileSystemManager _fs;
        private Command cmd;

        private void ShowPrompt()
        {
            ConsoleColor color = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{cmd.CurrentDir}]");
            Console.ForegroundColor = color;
            Console.Write("# ");
        }

        protected override void BeforeRun()
        {
            _fs = new FileSystemManager();
            cmd = new Command(_fs);

            cmd.ShowTitle();
        }

        protected override void Run()
        {
            ShowPrompt();

            var input = Console.ReadLine();
            
            if (!cmd.Exec(input))
            {
                Console.WriteLine("Incorrect command syntax");
            }

            if(cmd.Shutdown)
            {
                this.Stop();
            }
        }
    }
}
