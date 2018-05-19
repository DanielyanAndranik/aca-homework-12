using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MathServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Server local endpoint.
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 4000);

            // Creates a new task to manage tcp clients.
            var tcpListenerTask = new Task(() =>
            {
                TcpListener tcpListener = new TcpListener(localEndPoint);

                tcpListener.Start();

                // Accepts as more clients as they connect.
                while (true)
                {
                    var tcpClient = tcpListener.AcceptTcpClient();

                    Console.WriteLine("Connection accepted from " + tcpClient.Client.RemoteEndPoint);

                    // Creates a new task for each client.
                    var tcpTask = new Task((c) =>
                    {
                        var client = (TcpClient)c;

                        var stream = client.GetStream();

                        byte[] buffer = new byte[1024];

                        while (true)
                        {
                            var data = new StringBuilder();
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

                            if (data.ToString() == "finish")
                            {                              
                                tcpClient.Close();
                                break;
                            }

                            double? result;

                            string answer = null;

                            if (!TryDoMathOperation(data.ToString(), out result))
                            {
                                answer = "Invalid operation.";
                            }
                            else
                            {
                                answer = result.ToString();
                            }

                            stream.Write(Encoding.ASCII.GetBytes(answer), 0, Encoding.ASCII.GetBytes(answer).Length);

                        }

                    }, tcpClient);

                    tcpTask.Start();                    
                }
                tcpListener.Stop();
            });

            tcpListenerTask.Start();

            // Creates a Udp client.
            UdpClient udpClient = new UdpClient(localEndPoint);

            // Creates a task to manage  clients.
            var udpTask = new Task(() =>
            {
               
                while (true)
                {
                    var endPoint = new IPEndPoint(IPAddress.Any, 0);
                    var buffer = udpClient.Receive(ref endPoint);
                    Console.WriteLine("Connection accepted from " + endPoint.Address.Address);
                    // Creates a task for each client.
                    var clientTask = new Task((udpData) =>
                    {
                        var data = (UdpData)udpData;
                        while(true)
                        {
                            var request = Encoding.ASCII.GetString(data.Buffer);
                            Console.WriteLine(request);

                            if(request == "finish")
                            {
                                break;
                            }

                            double? result;

                            string answer = null;

                            if (!TryDoMathOperation(request.ToString(), out result))
                            {
                                answer = "Invalid format.";
                            }
                            else
                            {
                                answer = result.ToString();
                            }

                            udpClient.Send(Encoding.ASCII.GetBytes(answer), Encoding.ASCII.GetBytes(answer).Length, data.EndPoint);
                            buffer = udpClient.Receive(ref endPoint);
                        }                       

                    }, new UdpData { EndPoint = endPoint, Buffer = buffer});

                    clientTask.Start();
                }

                udpClient.Close();
            });

            udpTask.Start();

            Task.WaitAll(tcpListenerTask, udpTask);
        }

        /// <summary>
        /// Basic struct for passing as argument to the udp task.
        /// </summary>
        private struct UdpData
        {
            public IPEndPoint EndPoint { get; set; }
            public byte[] Buffer { get; set; }
        }

        /// <summary>
        /// Makes basic math operations.
        /// </summary>
        /// <param name="operation">Operation with a specified format.</param>
        /// <param name="result">The result of operation.</param>
        /// <returns>Returns true if operation was successfully done, else returns false.</returns>
        private static bool TryDoMathOperation(string operation, out double? result)
        {
            result = null;
            var splitString = operation.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (splitString.Length != 3) return false;
            var oper = splitString[0];

            double firstOperand;          
            if (!Double.TryParse(splitString[1], out firstOperand)) return false;
            double secondOperand;
            if (!Double.TryParse(splitString[2], out secondOperand)) return false;

            MathService service = new MathService();

            Operations operations = null;

            switch(oper)
            {
                case "+" :
                    operations += service.Add;
                    break;
                case "-":
                    operations += service.Sub;
                    break;
                case "*":
                    operations += service.Mult;
                    break;
                case "/":
                    if (secondOperand == 0) return false;
                    operations += service.Div;
                    break;
                default:
                    return false;
            }

            result = operations(firstOperand, secondOperand);
            return true;
        }
    }

    delegate double Operations(double x, double y);
}
