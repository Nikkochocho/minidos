using System.Net.Sockets;
using System.Net;
using RPCLibrary.Client;
using RPCLibrary.DataProtocol;


namespace RPCLibrary.Server
{
    public class RPCServer
    {
        private readonly string __destinationPath;
        private TcpListener __server;
        private bool __listening;


        public RPCServer(IPAddress address, int port, string destinationPath)
        {
            __destinationPath = destinationPath;
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
                    TcpClient? client;

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

                    _ = Task.Factory.StartNew(() =>
                    {
                        string? fileName = null;
                        string? cmdLineArgs = null;
                        FileStream? fs = default;
                        bool exit = false;


                        while (!exit && Read(client, out RPCData data))
                        {
                            // Send response based on client packet request type
                            if (data.Data != null)
                            {
                                switch (data.Type)
                                {
                                    case RPCData.TYPE_LUA_FILENAME:
                                        var luaFileName = System.Text.Encoding.Default.GetString(data.Data);

                                        Console.WriteLine($"RECEIVED LUA EXECUTABLE FILE NAME [{luaFileName}]");

                                        try
                                        {
                                            var name = $"{__destinationPath}/{Path.GetRandomFileName()}.lua";
                                           
                                            fs = File.Open(name, FileMode.Create, FileAccess.Write, FileShare.Write);
                                            fileName = name;
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                            fs?.Close();
                                            fs = null;
                                            exit = true;
                                        }
                                        continue;

                                    case RPCData.TYPE_LUA_PARMS:
                                        cmdLineArgs = System.Text.Encoding.Default.GetString(data.Data);
                                        Console.WriteLine($"RECEIVED LUA COMMAND LINE ARGUMENTS [{cmdLineArgs}]");
                                        continue;

                                    case RPCData.TYPE_LUA_EXECUTABLE:
                                        Console.WriteLine("RECEIVING LUA EXECUTABLE FILE CONTENT");
                                        fs?.Write(data.Data);
                                        fs?.Flush();

                                        // Execute Lua script sending screen content
                                        if (data.EndOfData)
                                        {
                                            fs?.Close();
                                            fs = null;

                                            if ((fileName == null) || !ExecLuaScript(client, fileName, cmdLineArgs))
                                            {
                                                Console.WriteLine($"Error to executing file {fileName}");
                                            }

                                            if (fileName != null)
                                            {
                                                System.IO.File.Delete(fileName);
                                            }

                                            exit = true;
                                            fileName = null;
                                            cmdLineArgs = null;
                                        }
                                        break;
                                }
                            }
                        }

                        client?.Close();
                        fs?.Close();

                        Console.WriteLine("Client connection handling finished");
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

        private bool ExecLuaScript(TcpClient client, string filename, string? args)
        {
            try
            {
                LuaEngine lua = new LuaEngine(client);

                lua.Args = (args ?? string.Empty);

                return lua.RunScript(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }
    }
}
