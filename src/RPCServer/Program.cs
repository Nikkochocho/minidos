using System.Diagnostics;
using System.Net;

namespace RPCServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length == 0)
            //{
            //    Process process = new Process();
            //    process.StartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
            //    process.StartInfo.UseShellExecute = false;
            //    process.StartInfo.RedirectStandardOutput = true;
            //    process.StartInfo.Arguments = "child";
            //    //process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            //    //{
            //    //    // Prepend line numbers to each line of the output.
            //    //    if (!String.IsNullOrEmpty(e.Data))
            //    //    {
            //    //        int lineCount = 0;
            //    //        lineCount++;
            //    //    }
            //    //});

            //    process.Start();

            //    // Asynchronously read the standard output of the spawned process.
            //    // This raises OutputDataReceived events for each line of output.
            //    process.BeginOutputReadLine();
            //    process.WaitForExit();
            //}
            //else
            {
                RPCLibrary.Server.RPCServer server = new RPCLibrary.Server.RPCServer(IPAddress.Any, 1999);

                if (server.Start())
                {
                    Console.WriteLine("RPC Server Started");
                    server.Run();
                    Console.WriteLine("RPC Server Finished");
                }
                else
                {
                    Console.WriteLine("Error to start RPC server");
                }
            }
        }
    }
}