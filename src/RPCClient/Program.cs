using RPCLibrary.Client;
using RPCLibrary.DataProtocol;

namespace RPCClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            RPCClient   client = new RPCClient();
            RPCData data = new RPCData()
            {
                Type = RPCData.TYPE_LUA_EXECUTABLE,
                EndOfData = true,
                Data = System.Text.Encoding.UTF8.GetBytes("TESTE".ToCharArray())
            };

            client.Connect("127.0.0.1", 1999);
            client.Send(data);

            Console.WriteLine("RPC Client started");

            Console.ReadLine();

        }
    }
}