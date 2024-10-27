/*
 * MiniDOS
 * Copyright (C) 2024  Lara H. Ferreira and others.
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Net.Sockets;
using System.Net;
using RPCLibrary.Client;
using RPCLibrary.DataProtocol;
using System.IO.Compression;
using RPCLibrary.Config;


namespace RPCLibrary.Server
{
    public class RPCServer
    {
        private const int            __MAX_RETRIES = 10;

        private readonly ServerParms __parms;
        private readonly TcpListener __server;
        private readonly char[]      __aCursor;
        private int                  __cursorPos;
        private bool                 __listening;

        public RPCServer(IPAddress address, int port, ServerParms parms)
        {
            __server = new TcpListener(address, port);
            __parms  = parms;

            __aCursor    = new char[4];
            __aCursor[0] = '|';
            __aCursor[1] = '/';
            __aCursor[2] = '-';
            __aCursor[3] = '\\';
            __cursorPos  = 0;
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
                while (__listening)
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

                    StartClientThread(client);
                }

                return true;
            }

            return false;
        }

        private void StartClientThread(TcpClient client)
        {
            _ = Task.Factory.StartNew(() =>
            {
                FileStream? fs          = default;
                string?     fileName    = null;
                string?     luaFileName = null;
                string?     cmdLineArgs = null;
                bool        exit        = false;
                bool        isShare     = false;
                bool        isZipped    = false;

                Console.WriteLine("Handling new connection");

                while (!exit && Read(client, out RPCData? data))
                {
                    // Send response based on client packet request type
                    if (data?.Data != null)
                    {
                        switch (data.Type)
                        {
                            case RPCData.TYPE_LUA_FILENAME:
                                var ext = (data.IsZipped ? LuaEngineConstants.ZIP_EXTENSION : LuaEngineConstants.LUA_EXTENSION);

                                luaFileName = System.Text.Encoding.Default.GetString(data.Data);
                                isShare     = luaFileName.ToLower().Contains(RPCData.SERVER_SHARED_FILE_PROTOCOL);
                                isZipped    = data.IsZipped;

                                UpdateProgress($"FOUND ==> [{luaFileName}] ");

                                if (!isShare)
                                {
                                    try
                                    {
                                        fileName = $"{__parms.DownloadFolder}/{Path.GetRandomFileName()}{ext}";
                                        fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                        fs?.Close();
                                        fs = null;
                                        exit = true;
                                    }
                                }
                                else
                                {
                                    var pos     = luaFileName.IndexOf(RPCData.SERVER_SHARED_FILE_PROTOCOL);

                                    pos += RPCData.SERVER_SHARED_FILE_PROTOCOL.Length;
                                    luaFileName = luaFileName.Remove(0, pos);
                                    fileName    = $"{__parms.SharedFolder}/{luaFileName}";
                                    exit = data.EndOfData;
                                }
                                continue;

                            case RPCData.TYPE_LUA_PARMS:
                                cmdLineArgs = System.Text.Encoding.Default.GetString(data.Data);
                                exit = data.EndOfData;
                                UpdateProgress($"COMMAND LINE ARGUMENTS ==> [{cmdLineArgs}] ");
                                continue;

                            case RPCData.TYPE_LUA_EXECUTABLE:
                                if (!isShare && fs != null)
                                {
                                    UpdateProgress($"DOWNLOADING ==> [{luaFileName}] ");

                                    fs?.Write(data.Data);
                                    fs?.Flush();

                                    // Execute Lua script sending screen content
                                    if (data.EndOfData)
                                    {
                                        exit     = true;
                                    }
                                }
                                break;
                        }
                    }
                }

                fs?.Close();

                if (luaFileName != null)
                {
                    UpdateProgress($"EXECUTING ==> [{luaFileName}] ");
                }

                // Execute Lua script
                if ((fileName == null) || !ExecLuaScript(client, fileName, luaFileName, cmdLineArgs, isZipped, isShare))
                {
                    Console.WriteLine($"Error to executing file {fileName}");
                }

                client?.Close();

                Console.WriteLine("Client connection handling finished");
            });
        }

        private bool Read(TcpClient tcpClient, out RPCData? data)
        {
            data = default;

            try
            {
                RPCClient rpcClient = new RPCClient(tcpClient);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream);

                return rpcClient.Recv(reader, out data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {ex.Message}");
                return false;
            }
        }

        private bool ExecLuaScript(TcpClient client, string filename, string originalFileName, string? args, bool isZipped, bool isShare)
        {
            string? destination = null;

            try
            {
                LuaEngine lua         = new LuaEngine(client, __parms);
                bool      ret         = false;
                bool      retry       = false;
                string    targetFile  = filename;
                int       retryCount  = 0;

                lua.Args = (args ?? string.Empty);

                if (isZipped)
                {
                    ret = ExtractExecutable(filename, isShare, out destination);

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

                do {
                    retry = !CleanExecutableData(filename, destination);

                    if (retry)
                    {
                        retryCount++;
                        Thread.Sleep(1000);

                        if (retryCount >= __MAX_RETRIES)
                        {
                            Console.WriteLine("Max retrties cleaning executable temporary data reached");
                            retry = false;
                        }
                        else
                        {
                            Console.WriteLine($"Error cleaning executable temporary data [{filename}]. Retrying.");
                        }
                    }
                } while (retry) ;

                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                CleanExecutableData(filename, destination);

                return false;
            }
        }

        private bool ExtractExecutable(string filename, bool isShare,out string? destination)
        {
            string path = (isShare ? __parms.SharedFolder : __parms.DownloadFolder);

            destination = $"{path}/{Path.GetRandomFileName()}";

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
