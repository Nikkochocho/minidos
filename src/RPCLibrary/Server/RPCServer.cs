using System.Net.Sockets;
using System.Net;
using RPCLibrary.Client;
using RPCLibrary.DataProtocol;


namespace RPCLibrary.Server
{
    public class RPCServer
    {
        private TcpListener __server;
        private bool __listening;


        public RPCServer(IPAddress address, int port)
        {
            __server = new TcpListener(address, port);
        }

        public bool Start()
        {
            if (!__listening)
            {
                try
                {
                    __server.Start();
                    __listening = true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());   
                    __listening = false;
                }
            }

            return __listening;
        }

        public bool Stop()
        {
            if (__listening)
            {
                try
                {
                    __server.Stop();
                    __listening= false;
                    return true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }

            return false;
        }

        public bool Run()
        {
            if (__listening)
            {
                while (true)
                {
                    TcpClient client;

                    try
                    {
                        client = __server.AcceptTcpClient();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        break;
                    }

                    Console.WriteLine("Handling new connection");

                    Task.Factory.StartNew(() =>
                    {
                        // TODO: LUA RESPONSE

                        if (Read(client, out RPCData data))
                        {
                            Console.WriteLine("TYPE -> " + data.Type);
                            Console.WriteLine("END OF DATA -> " + data.EndOfData);
                            Console.WriteLine("DATA SIZE -> " + data.DataSize);

                            if (data.Data != null)
                            {
                                Console.WriteLine("DATA -> " + System.Text.Encoding.Default.GetString(data.Data));
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error reading data");
                        }
                    });
                }

                return true;
            }

            return false;
        }

        private bool Read(TcpClient tcpClient, out RPCData data)
        {
            RPCClient rpcClient = new RPCClient(tcpClient);

            return rpcClient.Recv(tcpClient, out data);
        }
    }
}
