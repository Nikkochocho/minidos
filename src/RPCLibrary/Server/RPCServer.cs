﻿using System.Net.Sockets;
using System.Net;
using RPCLibrary.Client;
using RPCLibrary.DataProtocol;


namespace RPCLibrary.Server
{
    public class RPCServer
    {
        private string __DEST_PATH = "D:\\Projects\\C_SHARP\\minidos\\src\\RPCServer\\Resources";
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

                    _ = Task.Factory.StartNew(() =>
                    {
                        string tempFileName = Path.GetRandomFileName();
                        string filename = $"{__DEST_PATH}\\{tempFileName}";
                        string luaFileName = null;
                        FileStream? fs;

                        try
                        {
                            fs = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Write);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return;
                        }

                        while (Read(client, out RPCData data))
                        {
                            // Send response based on client packaet request type
                            if (data.Data != null)
                            {
                                switch (data.Type)
                                {
                                    case RPCData.TYPE_LUA_FILENAME:
                                        luaFileName = System.Text.Encoding.Default.GetString(data.Data);
                                        continue;
                                    case RPCData.TYPE_LUA_EXECUTABLE:
                                        fs?.Write(data.Data);
                                        fs?.Flush();
                                        break;
                                }
                            }

                            // Send Lua executable Screen content to client
                            if (data.EndOfData)
                            {
                                if (luaFileName != null)
                                {
                                    fs?.Close();
                                    fs = null;

                                    try
                                    {
                                        string destination = $"{__DEST_PATH}\\{luaFileName}";
                                        LuaEngine lua = new LuaEngine(client);

                                        System.IO.File.Move(filename, destination, true);

                                        if (!lua.RunScript(destination))
                                        {
                                            Console.WriteLine($"Error to executing file {destination}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Error lua file name not provided");
                                }
                            }
                        }

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
    }
}
