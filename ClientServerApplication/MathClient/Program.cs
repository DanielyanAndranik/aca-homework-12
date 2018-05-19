using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MathClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please choose which service do you want to use: TCP or UDP. TCP = 1, UDP = 2");
            var service = Console.ReadLine();
            // Server Endpoint
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.105"), 4000);
            //local endpoint.
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 7888);

            // Creates a Tcp client.
            if (service == "1")
            {
                TcpClient client = new TcpClient();
                client.Connect(remoteEndPoint);
                var stream = client.GetStream();

                Console.WriteLine("Please enter the math operation you want to do. Operation format is above. Or enter 'finish' to finish.");
                Console.WriteLine("operator: first_value:second_value");
               
                while (true)
                {
                    var data = new StringBuilder();
                    string operation = Console.ReadLine();
                    byte[] buffer = Encoding.ASCII.GetBytes(operation);
                    stream.Write(buffer, 0, buffer.Length);
                    if(operation == "finish")
                    {
                        client.Close();
                        break;
                    }

                    var i = stream.Read(buffer, 0, buffer.Length);

                    while (i != 0)
                    {
                        data.Append(Encoding.ASCII.GetString(buffer, 0, i));
                        if (i < buffer.Length)
                        {
                            break;
                        }
                        if(!stream.DataAvailable)
                        {
                            break;
                        }
                        i = stream.Read(buffer, 0, buffer.Length);

                    }
                    Console.WriteLine(data);
                }    
            }
            // Creates a Udp client.
            else if(service == "2")
            {
                UdpClient client = new UdpClient(localEndPoint);
                client.Connect(remoteEndPoint);
                Console.WriteLine("Please enter the math operation you want to do. Operation format is above. Or enter 'finish' to finish.");
                Console.WriteLine("operator: first_value:second_value");
                while(true)
                {
                    //var data = new StringBuilder();
                    string operation = Console.ReadLine();
                    byte[] buffer = Encoding.ASCII.GetBytes(operation);
                    client.Send(buffer, buffer.Length);
                    if(operation == "finish")
                    {
                        client.Close();
                    }

                    var bytes = client.Receive(ref remoteEndPoint);
                    Console.WriteLine(Encoding.ASCII.GetString(bytes));
                    if(Encoding.ASCII.GetString(bytes) == "bye")
                        break;
                }
                client.Close();
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }
        }
    }
}
