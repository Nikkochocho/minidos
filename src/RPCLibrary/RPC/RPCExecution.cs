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

using RPCLibrary.Array;
using RPCLibrary.Compression;
using System.Text;

namespace RPCLibrary.RPC
{
    public class RPCExecution
    {
        private readonly RPCClient      __client;
        private char[]                  __screenBuffer;

        public RPCExecution()
        {
            __client       = new RPCClient();
            __screenBuffer = new char[BitCompression.DEFAULT_UNCOMPRESSED_DATA];
        }

        public bool Execute(string filepath, string host, int port, string? cmdLineArgs)
        {
            FileStream? fs = default;
            bool isShared = filepath.ToLower().Contains(RPCData.SERVER_SHARED_FILE_PROTOCOL);

            // If is a resource present on server is not needed send file content data,
            // so open file is not needed
            if (!isShared)
            {
                // Open file.
                try
                {
                    fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"File open exception => [{ex.Message}]");
                    return false;
                }
            }

            bool ret = __client.Connect(host, port);

            if (!ret)
            {
                Console.WriteLine("Error handling execution");
                fs?.Close();
                return false;
            }

            string fileName = isShared ? filepath : Path.GetFileName(filepath);
            bool isZippedFile = fileName.ToLower().Contains(LuaEngineConstants.ZIP_EXTENSION);
            byte[] aFileName = Encoding.Default.GetBytes(fileName);
            byte[] buffer = new byte[RPCData.DEFAULT_BLOCK_SIZE];
            int bytesRead = aFileName.Length;
            RPCData data = new RPCData()
            {
                Type = RPCData.TYPE_LUA_FILENAME,
                EndOfData = isShared && cmdLineArgs == null,
                IsZipped = isZippedFile,
                Data = aFileName
            };

            // Send file name
            ret = __client.Send(data);

            if (!ret)
            {
                Console.WriteLine("Error to send file name data");
                fs.Close();
                __client.Close();

                return false;
            }

            // Send command line arguments (if any)
            if (cmdLineArgs != null)
            {
                data.Type = RPCData.TYPE_LUA_PARMS;
                data.EndOfData = isShared;
                data.Data = Encoding.Default.GetBytes(cmdLineArgs);

                ret = __client.Send(data);

                if (!ret)
                {
                    Console.WriteLine("Error to send lua parameters data");
                    fs?.Close();
                    __client.Close();

                    return false;
                }
            }

            // If is a resource present on server, is not needed send file content data
            if (!isShared)
            {
                bytesRead = RPCData.DEFAULT_BLOCK_SIZE;
                data.Type = RPCData.TYPE_LUA_EXECUTABLE;
                data.DataSize = bytesRead;
                data.Data = buffer;

                // Read Lua executable script on DEFAULT_BLOCK_SIZE byte chunks and send to execute on server
                while (!data.EndOfData)
                {
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                    data.EndOfData = bytesRead != RPCData.DEFAULT_BLOCK_SIZE;

                    if (data.EndOfData)
                    {
                        // Clean unused extra byte array
                        data.DataSize = bytesRead;
                        System.Array.Copy(buffer, data.Data, bytesRead);
                    }

                    ret = __client.Send(data);

                    if (!ret)
                    {
                        Console.WriteLine("Error to send data");
                        break;
                    }
                }

                fs?.Close();
            }

            // Receive and process Lua screen response
            if (ret)
            {
                ReceiveLuaScreenResponse();
            }

            __client.Close();

            return ret;
        }

        private void ReceiveLuaScreenResponse()
        {
            // Receive and process Lua script responses
            while (__client.Recv(out RPCData data))
            {
                switch (data.Type)
                {
                    case RPCData.TYPE_LUA_ANSI_COMMAND_RESPONSE:
                    case RPCData.TYPE_LUA_SCREEN_RESPONSE:
                        {
                            System.Array.Clear(__screenBuffer);
                            ArrayHelper.Convert(data.Data, ref __screenBuffer);
                            ScreenResponseHandling(__screenBuffer);
                        }
                        break;

                    case RPCData.TYPE_LUA_SCREEN_LOW_LATENCY_RESPONSE:
                        {
                            MemoryStream stream = new MemoryStream(data.Data);

                            while(stream.Position < stream.Length)
                            {
                                if (__client.Deserialize(stream, out RPCData screenData))
                                {
                                    switch(screenData.Type)
                                    {
                                        case RPCData.TYPE_LUA_SCREEN_RESPONSE:
                                            BitCompression.UnCompress(screenData.Data, __screenBuffer, screenData.DataSize);
                                            break;

                                        case RPCData.TYPE_LUA_ANSI_COMMAND_RESPONSE:
                                            ArrayHelper.Convert(screenData.Data, ref __screenBuffer);
                                            break;
                                    }

                                    ScreenResponseHandling(__screenBuffer, true);
                                }
                                else
                                {
                                    Console.WriteLine("Error reading low latency screen data");
                                }
                            }
                        }
                        break;
                }

                if (data.EndOfData)
                {
                    break;
                }
            }
        }

        private void ScreenResponseHandling(char[] data, bool newLine = false)
        {
            if (ArrayHelper.Contains(data, RPCData.ANSI_CLEAR_SCREEN_CODE_ARRAY))
            {
                Console.Clear();
                return;
            }

            if (ArrayHelper.Contains(data, RPCData.ANSI_SET_CURSOR_HOME_POSITION_ARRAY))
            {
                Console.SetCursorPosition(0, 0);
                return;
            }

            if (newLine)
            {
                Console.WriteLine(data);
            }
            else
            {
                Console.Write(data);
            }
        }
    }
}
