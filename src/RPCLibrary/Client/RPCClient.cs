using RPCLibrary.DataProtocol;
using System.Net.Sockets;

namespace RPCLibrary.Client
{
    public class RPCClient
    {
        private readonly TcpClient __tcpClient;

        public bool EnableAllExceptions { get; set; } = false;

        public RPCClient()
        {
            __tcpClient = new TcpClient();
        }

        public RPCClient(TcpClient tcpClient)
        {
            __tcpClient = tcpClient;
        }

        public bool Connect(string host, int port)
        {
            try
            {
                __tcpClient.Connect(host, port);
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
                    NetworkStream stream = __tcpClient.GetStream();
                    BinaryWriter  writer = new BinaryWriter(stream);

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
            }

            return false;
        }

        public bool Recv(TcpClient client, out RPCData data)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                BinaryReader reader = new BinaryReader(stream);
                data = new RPCData();

                data.Type = reader.ReadInt32();
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
            return Recv(__tcpClient, out data);
        }
    }
}
