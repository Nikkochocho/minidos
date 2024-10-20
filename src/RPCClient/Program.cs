﻿using RPCLibrary.Command;

namespace RPCClientApp
{
    class Program
    {
        private const int    __WINDOW_HEIGHT  = 33;
        private const string __RPC_CLIENT_APP = "RPC CLIENT";
        private const string __EXECUTE_LUA    = "exec";
        private const string __EXIT           = "exit";
        private const string __HELP           = "help";

        private static string __hostname      = "127.0.0.1";
        private static string __port          = "1999";
        private static string __filename      = "../../../Resources/bad_apple_80x31.zip";
        private static string __parms         = "TEST PARMS";

        static void Main(string[] args)
        {
            bool exit = false;

            Console.Title        = __RPC_CLIENT_APP;
            Console.WindowHeight = __WINDOW_HEIGHT;

            Console.WriteLine("RPC Client started");
            Console.WriteLine();
            Console.Write("type ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("help ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("to see all available commands");
            Console.WriteLine();

            while (!exit)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("# ");
                Console.ForegroundColor = ConsoleColor.White;

                var input = Console.ReadLine();

                switch(input)
                {
                    case __EXECUTE_LUA:
                        ExecuteLua();
                        break;

                    case __HELP:
                        Help();
                        break;

                    case __EXIT:
                        exit = true;
                        break;
                }
            }
        }

        static void ExecuteLua()
        {
            WriteLabel("HOSTNAME", __hostname);
            string hostname = Console.ReadLine();
            WriteLabel("PORT", __port);
            string port = Console.ReadLine();
            WriteLabel("LUA FILE NAME", __filename);
            string filename = Console.ReadLine();
            WriteLabel("LUA PARAMETERS", __parms);
            string parms = Console.ReadLine();

            __hostname = (hostname.Length == 0 ? __hostname : hostname);
            __port = (port.Length == 0 ? __port : port);
            __filename = (filename.Length == 0 ? __filename : filename);
            __parms = (parms.Length == 0 ? __parms: parms);

            RPCExecution exec = new RPCExecution();

            if (exec.Execute(__filename, __hostname, int.Parse(__port), __parms))
            {
                Console.WriteLine("Execution sucessfull");
            }
            else
            {
                Console.WriteLine("Execution failed");
            }
        }

        static void WriteLabel(string label, string value)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(label + " ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{value}] => ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void Help()
        {
            Console.WriteLine("Available commands");
            Console.WriteLine();
            Console.WriteLine("exec - Execute a Lua file sendint it to the RPC server");
            Console.WriteLine("exit - exit RPC client application");
            Console.WriteLine("help - Show this help");
        }
    }
}