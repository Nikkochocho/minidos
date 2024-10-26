using RPCLibrary.DataProtocol;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;

namespace RPCLibrary.Client
{
    public class RPCClient
    {
        private readonly TcpClient     __tcpClient;
        private BinaryWriter           __writer;
        private BinaryReader           __reader;


        public bool EnableAllExceptions { get; set; } = false;


        private void Init()
        {
            NetworkStream stream = __tcpClient.GetStream();

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

        public bool Recv(BinaryReader reader, out RPCData data)
        {
            try
            {
                data = new RPCData();

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
            return Recv(__reader, out data);
        }
    }
}
