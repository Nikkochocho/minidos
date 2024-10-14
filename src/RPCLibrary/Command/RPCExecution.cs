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
            FileStream? fs;

            // Open file.

            try
            {
                fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File open exception => [{ex.Message}]");
                return false;
            }

            bool ret = __client.Connect(host, port);

            if (!ret)
            {
                Console.WriteLine("Error handling execution");
                return false;
            }

            string fileName = Path.GetFileName(filepath);
            byte[] aFileName = Encoding.ASCII.GetBytes(fileName);
            byte[] buffer = new byte[RPCData.DEFAULT_BLOCK_SIZE];
            int bytesRead = aFileName.Length;           
            RPCData data = new RPCData()
            {
                Type = RPCData.TYPE_LUA_FILENAME,
                EndOfData = false,
                Data = aFileName 
            };

            // Send file name
            ret = __client.Send(data);

            if (!ret)
            {
                Console.WriteLine("Error to send file name data");
                __client.Close();

                return false;
            }

            // Send command line arguments (if any)
            if (cmdLineArgs != null)
            {
                data.Type = RPCData.TYPE_LUA_PARMS;
                data.EndOfData = false;
                data.Data = Encoding.ASCII.GetBytes(cmdLineArgs);

                ret = __client.Send(data);

                if (!ret)
                {
                    Console.WriteLine("Error to send lua parameters data");
                    __client.Close();

                    return false;
                }
            }

            bytesRead = RPCData.DEFAULT_BLOCK_SIZE;
            data.Type = RPCData.TYPE_LUA_EXECUTABLE;
            data.DataSize = bytesRead;
            data.Data = buffer;

            // Read Lua executable script on 512 bytes chunks and send to execute on server

            while (!data.EndOfData)
            {
                bytesRead = fs.Read(buffer, 0, buffer.Length);
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

            // Receive and process client responses
            if (ret)
            {
                ReceiveClientResponse();
            }

            fs.Close();
            __client.Close();

            return ret;
        }

        private void ReceiveClientResponse()
        {
            // Receive and process client responses
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

                default:
                    Console.WriteLine(data);
                    break;
            }
        }
    }
}
