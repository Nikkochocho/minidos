﻿using RPCLibrary.Client;
using RPCLibrary.DataProtocol;
using System.Text;

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
            bool ret = __client.Connect(host, port);
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

            string fileName = Path.GetFileName(filepath);
            byte[] aFileName = Encoding.ASCII.GetBytes(fileName);
            byte[] buffer = new byte[__BLOCK_SIZE];
            int bytesRead = aFileName.Length;           
            RPCData data = new RPCData()
            {
                Type = RPCData.TYPE_LUA_FILENAME,
                EndOfData = (bytesRead <= 0),
                Data = aFileName 
            };


            // Send file name
            ret = __client.Send(data);

            if (!ret)
            {
                Console.WriteLine("Error to send data");
                return false;
            }

            bytesRead = __BLOCK_SIZE;
            data.Type = RPCData.TYPE_LUA_EXECUTABLE;
            data.DataSize = bytesRead;
            data.Data = buffer;

            // Data read loop

            while (!data.EndOfData)
            {
                bytesRead = fs.Read(buffer, 0, buffer.Length);
                data.EndOfData = (bytesRead != __BLOCK_SIZE);

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

                // TODO: REMOVER WriteLine abaixo
                Console.WriteLine(System.Text.Encoding.Default.GetString(data.Data));
            }
            fs.Close();

            return ret;
        }
    }
}
