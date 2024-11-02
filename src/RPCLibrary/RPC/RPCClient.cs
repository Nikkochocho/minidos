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

namespace RPCLibrary.RPC
{
    public class RPCClient
    {
        private readonly TcpClient __tcpClient;
        private readonly RPCData   __data     = new RPCData(true);
        private readonly byte[]    __bufferIn = new byte[BufferSize];
        private BinaryWriter?      __writer   = null;
        private BinaryReader?      __reader   = null;


        public bool EnableAllExceptions { get; set; } = false;

        public static int BufferSize
        {
            get
            {
                // Extra bytes (for safety data transfer screen operations)
                return (RPCData.SCREEN_BUFFER_SIZE + 512);
            }
        }

        public RPCData Data
        {
            get
            {
                __data.Data = __data.Data ?? __bufferIn;

                return __data;
            }
        }


        private void Init()
        {
            var stream = __tcpClient.GetStream();

            __writer = new BinaryWriter(stream);
            __reader = new BinaryReader(stream);
        }

        public RPCClient()
        {
            __tcpClient = new TcpClient();
        }

        public RPCClient(TcpClient tcpClient)
        {
            __tcpClient = tcpClient;
            Init();
        }

        public bool Connect(string host, int port)
        {
            try
            {
                __tcpClient.Connect(host, port);
                Init();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool Close()
        {
            if (__tcpClient.Connected)
            {
                __tcpClient.Close();
                return true;
            }

            return false;
        }

        public bool Send(RPCData data)
        {
            return Serialize(__writer, data);
        }

        public bool Serialize(BinaryWriter? writer, RPCData data)
        {
            try
            {
                writer.Write(data.Type);
                writer.Write(data.EndOfData);
                writer.Write(data.IsZipped);
                writer.Write(data.DataSize);

                if (data.Data != null)
                {
                    writer.Write(data.Data);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public bool Deserialize(Stream stream, out RPCData data)
        {
            try
            {
                int   size;

                data      = __data;
                data.Data = __bufferIn;

                // Type
                size = sizeof(int);
                if (stream.Read(__bufferIn, 0, size) != size)
                    return false;

                data.Type = BitConverter.ToInt32(__bufferIn, 0);

                // EndOfData
                size = sizeof(bool);
                if (stream.Read(__bufferIn, 0, size) != size)
                    return false;

                data.EndOfData = BitConverter.ToBoolean(__bufferIn, 0);

                // IsZipped
                size = sizeof(bool);
                if (stream.Read(__bufferIn, 0, size) != size)
                    return false;

                data.IsZipped = BitConverter.ToBoolean(__bufferIn, 0);

                // DataSize
                size = sizeof(int);
                if (stream.Read(__bufferIn, 0, size) != size)
                    return false;

                data.DataSize = BitConverter.ToInt32(__bufferIn, 0);

                // Data
                if (data.DataSize > 0)
                {
                    if (stream.Read(__bufferIn, 0, data.DataSize) != data.DataSize)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                data = default;

                if (ex.GetType() != typeof(EndOfStreamException) && EnableAllExceptions)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return false;
        }

        public bool Deserialize(BinaryReader? reader, out RPCData data)
        {
            try
            {
                data = __data;

                data.Type      = reader.ReadInt32();
                data.EndOfData = reader.ReadBoolean();
                data.IsZipped  = reader.ReadBoolean();
                data.DataSize  = reader.ReadInt32();

                if (data.DataSize > 0)
                {
                    data.Data = reader.ReadBytes(data.DataSize);
                }

                return true;
            }
            catch (Exception ex)
            {
                data = default;

                if (ex.GetType() != typeof(EndOfStreamException) && EnableAllExceptions)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return false;
        }

        public bool Recv(out RPCData data)
        {
            return Deserialize(__reader, out data);
        }

        public bool RecvFromStream(ref RPCData data)
        {
            return Deserialize(__tcpClient.GetStream(), out data);
        }
    }
}
