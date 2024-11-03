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

using RPCLibrary.Compression;
using System.Collections.Concurrent;
using System.Text;

namespace RPCLibrary.RPC
{
    public class RPCScreenCompression
    {
        private const int                    __TIME_WAIT_CHECK_QUEUE = 0;
        private const int                    __TIME_WAIT_SEND        = 50;

        private readonly RPCClient           __rpcClient;
        private bool                         __isRunning       = false;
        private BlockingCollection<RPCData>  __screenDataQueue = new BlockingCollection<RPCData>();

        public bool IsRunning
        {
            get { return __isRunning; }
        }

        private void StartConsumerQueueThread()
        {
            _ = Task.Factory.StartNew(() =>
            {
                MemoryStream   stream     = new MemoryStream(RPCClient.RecvBufferSize);
                BinaryWriter   writer     = new BinaryWriter(stream);
                RPCData        bufferData = new RPCData()
                {
                    Type = RPCData.TYPE_LUA_SCREEN_LOW_LATENCY_RESPONSE,
                    IsZipped  = false,
                    EndOfData = false,
                };

                while (__isRunning)
                {
                    if (__screenDataQueue.TryTake(out RPCData? data, __TIME_WAIT_CHECK_QUEUE))
                    {
                        if (data.Type != RPCData.TYPE_LUA_ANSI_COMMAND_RESPONSE)
                        {
                            var compressedData = BitCompression.Compress(Encoding.Default.GetChars(data.Data));

                            data.Data = compressedData;
                        }

                        __rpcClient.Serialize(writer, data);

                        if (stream.Length >= RPCConstants.RECV_BUFFER_SIZE)
                        {
                            bufferData.Data = stream.ToArray();

                            __rpcClient.Send(bufferData);

                            // Reset Stream and Writer
                            stream.Position = 0;
                            stream.SetLength(0);
                            writer.Seek(0, SeekOrigin.Begin);

                            Thread.Sleep(__TIME_WAIT_SEND);
                        }
                    }
                }
            });
        }

        public RPCScreenCompression(RPCClient rpcClient)
        {
            __rpcClient = rpcClient;
        }

        public bool Start()
        {
            if (!__isRunning)
            {
                StartConsumerQueueThread();
                __isRunning = true;

            }

            return __isRunning;
        }

        public bool Stop()
        {
            if (__isRunning)
            {
                __isRunning = false;
                return true;
            }

            return __isRunning;
        }

        public bool Send(RPCData data)
        {
            if (__isRunning)
            {
                __screenDataQueue.TryAdd(data);

                return true;
            }

            return __rpcClient.Send(data);
        }
    }
}
