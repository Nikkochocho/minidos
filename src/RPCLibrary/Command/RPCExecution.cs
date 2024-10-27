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

using RPCLibrary.Client;
using RPCLibrary.DataProtocol;
using System.Text;

namespace RPCLibrary.Command
{
    public class RPCExecution
    {
        private readonly RPCClient __client;

        public RPCExecution() 
        {
            __client = new RPCClient();
        }

        public bool Execute(string filepath, string host, int port, string? cmdLineArgs)
        {
            FileStream? fs       = default;
            bool        isShared = filepath.ToLower().Contains(RPCData.SERVER_SHARED_FILE_PROTOCOL);

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

            string fileName     = (isShared ? filepath : Path.GetFileName(filepath));
            bool   isZippedFile = fileName.ToLower().Contains(LuaEngineConstants.ZIP_EXTENSION);
            byte[] aFileName    = Encoding.Default.GetBytes(fileName);
            byte[] buffer       = new byte[RPCData.DEFAULT_BLOCK_SIZE];
            int bytesRead       = aFileName.Length;           
            RPCData data        = new RPCData()
            {
                Type      = RPCData.TYPE_LUA_FILENAME,
                EndOfData = (isShared && (cmdLineArgs == null)),
                IsZipped  = isZippedFile,
                Data      = aFileName 
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
                data.Type      = RPCData.TYPE_LUA_PARMS;
                data.EndOfData = isShared;
                data.Data      = Encoding.Default.GetBytes(cmdLineArgs);

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
                    bytesRead  = fs.Read(buffer, 0, buffer.Length);
                    data.EndOfData = (bytesRead != RPCData.DEFAULT_BLOCK_SIZE);

                    if (data.EndOfData)
                    {
                        // Clean unused extra byte array
                        data.DataSize = bytesRead;
                        Array.Copy(buffer, data.Data, bytesRead);
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
                    case RPCData.TYPE_LUA_SCREEN_RESPONSE:
                        string strData = Encoding.Default.GetString(data.Data);

                        ScreenResponseHandling(strData);
                        break;
                }

                if (data.EndOfData)
                {
                    break;
                }
            }
        }

        private void ScreenResponseHandling(string data)
        {
            switch (data) // Handle ANSI escape commands

            {
                case RPCData.ANSI_CLEAR_SCREEN_CODE:
                    Console.Clear();
                    break;

                case RPCData.ANSI_SET_CURSOR_HOME_POSITION:
                    Console.SetCursorPosition(0, 0);
                    break;

                default:
                    Console.Write(data);
                    break;
            }
        }
    }
}
