using System.Net;

namespace RPCServerApp
{
    class Program
    {
        private const string __RPC_SERVER_APP = "RPC SERVER";
        private const int __port = 1999;
        private static string __DESTINATION_PATH = "../../../Resources";

        static void Main(string[] args)
        {
            RPCLibrary.Server.RPCServer server = new RPCLibrary.Server.RPCServer(IPAddress.Any, 1999, __DESTINATION_PATH);

            Console.Title = $"{__RPC_SERVER_APP} - {IPAddress.Any}:{__port}";

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