using System.Net;
using RPCLibrary.Server;

namespace RPCServerApp
{
    class Program
    {
        static void Main(string[] args)
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