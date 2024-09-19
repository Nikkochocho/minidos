using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;      //required
using System.Net.Sockets;    //required

namespace ServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 1999);
            // we set our IP address as server's address, and we also set the port: 9999

            server.Start();  // this will start the server

            Console.WriteLine("ServerStarted");

            while (true)   //we wait for a connection
            {
                TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it

                Task.Factory.StartNew(() =>
                {
                    Console.WriteLine("Handling new connection");

                    NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

                    byte[] hello = new byte[100];   //any message must be serialized (converted to byte array)
                    hello = Encoding.Default.GetBytes("hello world");  //conversion string => byte array
                    int bytesRead = 0;

                    while (client.Connected && bytesRead >= 0)  //while the client is connected, we look for incoming messages
                    {
                        byte[] msg = new byte[1024 * 8];     //the messages arrive as byte array
                        
                        bytesRead = ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client

                        if (bytesRead > 0)
                        {
                            Console.WriteLine(Encoding.ASCII.GetString(msg)); //now , we write the message as string
                            ns.Write(msg, 0, bytesRead);     //sending the message
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    Console.WriteLine("Disconnected");
                });
            }

        }
    }
}