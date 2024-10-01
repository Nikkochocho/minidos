using RPCLibrary.Client;
using RPCLibrary.DataProtocol;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            RPCData data = new RPCData()
            {
                Type = RPCData.TYPE_LUA_EXECUTABLE,
                EndOfData = false,
            };

            if (!ret)
            {
                Console.WriteLine("Error handling execution");
                return false;
            }

            // Abrir arquivo.
            
            using (FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                byte[] buffer = new byte[__BLOCK_SIZE];
                int bytesRead = fs.Read(buffer, 0, buffer.Length);

                //loop leitura do arquivo

                if (bytesRead > 0)
                {
                    while(!data.EndOfData) 
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

                        Console.WriteLine(System.Text.Encoding.Default.GetString(data.Data));
                    }
                }
                fs.Close();
            }


            return ret;
        }

    }
}
