using System.Net.Sockets;
using System.Net;
using RPCLibrary.Client;
using RPCLibrary.DataProtocol;
using System.IO.Compression;


namespace RPCLibrary.Server
{
    public class RPCServer
    {
        private readonly string __destinationPath;
        private readonly OpenAIParms __openAiParms;
        private readonly TcpListener __server;
        private readonly char[] __aCursor;
        private int __cursorPos;
        private bool __listening;


        public RPCServer(IPAddress address, int port, string destinationPath, OpenAIParms openAiParms)
        {
            __destinationPath = destinationPath;
            __server      = new TcpListener(address, port);
            __openAiParms = openAiParms;

            __aCursor     = new char[4];
            __aCursor[0]  = '|';
            __aCursor[1]  = '/';
            __aCursor[2]  = '-';
            __aCursor[3]  = '\\';
            __cursorPos   = 0;
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
                        string?     fileName     = null;
                        string?     luaFileName  = null;
                        string?     cmdLineArgs  = null;
                        FileStream? fs           = default;
                        bool        exit         = false;

                        while (!exit && Read(client, out RPCData data))
                        {
                            // Send response based on client packet request type
                            if (data.Data != null)
                            {
                                switch (data.Type)
                                {
                                    case RPCData.TYPE_LUA_FILENAME:
                                        var ext = (data.IsZipped ? LuaEngineConstants.ZIP_EXTENSION : LuaEngineConstants.LUA_EXTENSION);

                                        luaFileName = System.Text.Encoding.Default.GetString(data.Data);

                                        UpdateProgress($"RECEIVED LUA EXECUTABLE FILE NAME [{luaFileName}] ");

                                        try
                                        {
                                            var name = $"{__destinationPath}/{Path.GetRandomFileName()}{ext}";
                                           
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
                                        UpdateProgress($"RECEIVED LUA COMMAND LINE ARGUMENTS [{cmdLineArgs}] ");
                                        continue;

                                    case RPCData.TYPE_LUA_EXECUTABLE:
                                        UpdateProgress($"RECEIVING LUA EXECUTABLE FILE CONTENT [{luaFileName}] ");
                                        fs?.Write(data.Data);
                                        fs?.Flush();

                                        // Execute Lua script sending screen content
                                        if (data.EndOfData)
                                        {
                                            fs?.Close();
                                            fs = null;

                                            if ((fileName == null) || !ExecLuaScript(client, fileName, luaFileName, cmdLineArgs, data.IsZipped))
                                            {
                                                Console.WriteLine($"Error to executing file {fileName}");
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

        private bool ExecLuaScript(TcpClient client, string filename, string originalFileName, string? args, bool isZipped)
        {
            try
            {
                LuaEngine lua         = new LuaEngine(client, __openAiParms);
                bool      ret         = false;
                string    targetFile  = filename;
                string?   destination = null;

                lua.Args = (args ?? string.Empty);

                if (isZipped)
                {
                    ret = ExtractExecutable(filename, out destination);

                    if (!ret) 
                    {
                        Console.WriteLine($"Error extracting zipped file [{filename}]");

                        if (!CleanExecutableData(filename, destination))
                        {
                            Console.WriteLine($"Error cleaning executable temporary data [{filename}]");
                        }

                        return ret;
                    }

                    var pos = originalFileName.ToLower().IndexOf(LuaEngineConstants.ZIP_EXTENSION, StringComparison.OrdinalIgnoreCase);

                    ret = (pos >= 0);

                    if(!ret)
                    {
                        Console.WriteLine($"Error extracting zipped file [{filename}]. Invalid extension");

                        if (!CleanExecutableData(filename, destination))
                        {
                            Console.WriteLine($"Error cleaning executable temporary data [{filename}]");
                        }

                        return ret;
                    }

                    var luaFileName = originalFileName.Substring(0, pos); 

                    targetFile = $"{destination}/{luaFileName}{LuaEngineConstants.LUA_EXTENSION}";
                }

                ret = lua.RunScript(targetFile);

                if (!CleanExecutableData(filename, destination))
                {
                    Console.WriteLine($"Error cleaning executable temporary data [{filename}]");
                }

                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool ExtractExecutable(string filename, out string? destination)
        {
            destination = $"{__destinationPath}/{Path.GetRandomFileName()}";

            try
            {
                ZipFile.ExtractToDirectory(filename, destination);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                destination = null;
                return false;
            }
        }

        private bool CleanExecutableData(string filename, string? destination)
        {
            try
            {
                File.Delete(filename);

                if (destination != null)
                {
                    Directory.Delete(destination, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }

        private void UpdateProgress(string title)
        {
            var pos = Console.GetCursorPosition();

            Console.Write(title);

            Console.Write( $"{__aCursor[__cursorPos]}" );
            Console.SetCursorPosition(pos.Left, pos.Top);

            if (__cursorPos == 3)
                __cursorPos = 0;
            else
                __cursorPos++;
        }
    }
}
