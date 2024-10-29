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
        private BinaryWriter? __writer = null;
        private BinaryReader? __reader = null;


        public bool EnableAllExceptions { get; set; } = false;


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
            if (__tcpClient.Connected)
            {
                try
                {
                    __writer.Write(data.Type);
                    __writer.Write(data.EndOfData);
                    __writer.Write(data.IsZipped);
                    __writer.Write(data.DataSize);

                    if (data.Data != null)
                    {
                        __writer.Write(data.Data);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return false;
        }

        public bool Recv(BinaryReader? reader, out RPCData data)
        {
            try
            {
                data = new RPCData();

                data.Type = reader.ReadInt32();
                data.EndOfData = reader.ReadBoolean();
                data.IsZipped = reader.ReadBoolean();
                data.DataSize = reader.ReadInt32();

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
            return Recv(__reader, out data);
        }
    }
}
