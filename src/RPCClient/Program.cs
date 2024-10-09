using RPCLibrary.Command;

namespace RPCClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            RPCExecution exec = new RPCExecution();

            string filepath = "D:\\Projects\\C_SHARP\\minidos\\src\\RPCClient\\Resources\\teste.txt";
            string host = "localhost";
            int port = 1999;

            Console.WriteLine("RPC Client started");

            if(exec.Execute(filepath, host, port))
            {
                Console.WriteLine("Execution worked sucessfully");
            }
            else
            {
                Console.WriteLine("Execution failed");
            }

            Console.ReadLine();

        }
    }
}