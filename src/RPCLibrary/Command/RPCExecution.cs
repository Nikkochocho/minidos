using RPCLibrary.Client;
using RPCLibrary.DataProtocol;

namespace RPCLibrary.Command
{
    public class RPCExecution
    {
        private const int __BLOCK_SIZE = 512;

        private readonly RPCClient __client;

        public RPCExecution() 
        {
            __client = new RPCClient();
        }

        public bool Execute(string filepath, string host, int port)
        {
            bool ret = true; //__client.Connect(host, port);
            FileStream? fs;

            if (!ret)
            {
                Console.WriteLine("Error handling execution");
                return false;
            }

            // Open file.

            try
            {
                fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            byte[] buffer = new byte[__BLOCK_SIZE];
            int bytesRead = fs.Read(buffer, 0, buffer.Length);
            RPCData data = new RPCData()
            {
                Type = RPCData.TYPE_LUA_EXECUTABLE,
                EndOfData = (bytesRead <= 0),
            };

            // Data read loop

            while (!data.EndOfData)
            {
                data.Data = buffer;
                data.EndOfData = (bytesRead != __BLOCK_SIZE);
                //ret = __client.Send(data);

                if (!ret)
                {
                    Console.WriteLine("Error to send data");
                    break;
                }

                if (!data.EndOfData)
                {
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                }
                else
                {
                    // Clean unused extra byte array
                    Array.Clear(buffer, bytesRead, (buffer.Length - bytesRead));
                }

                // TODO: REMOVER WriteLine abaixo
                Console.WriteLine(System.Text.Encoding.Default.GetString(data.Data));
            }
            fs.Close();

            return ret;
        }
    }
}
