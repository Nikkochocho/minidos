using System;
using Sys = Cosmos.System;
using MiniDOS.Shell;

namespace MiniDOS
{
    public class Kernel : Sys.Kernel
    {
        private static string __YEAR = "2024";
        private static string __VERSION = "0.1";
        private static string __AUTHOR = "Lara H. Ferreira";

        private FileSystem.FileSystem _fs;
        private Command cmd;

        protected override void BeforeRun()
        {
            string license = $"minidos v{__VERSION}. CopyLeft (c) {__YEAR} by {__AUTHOR}\n" +
                             "This program comes with ABSOLUTELY NO WARRANTY;\n" + 
                             "This is free software, and you are welcome to redistribute it under\n" +
                             "certain conditions.\n\n";

            Console.WriteLine(license);

            _fs = new FileSystem.FileSystem();
            cmd = new Command(_fs);
        }

        protected override void Run()
        {
            Console.Write("# ");
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
