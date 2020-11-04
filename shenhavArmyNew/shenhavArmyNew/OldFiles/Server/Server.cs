using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using Platformaa;
using System.IO;
using Newtonsoft.Json;

namespace Server
{
    class Server
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        static void Main(string[] args)
        {
            string path = "C:\\Users\\Shenhav\\source\\repos\\tsetCCode\\tsetCCode\\test.c";
            //Program.findAllFunctions(path,);
            SetupServer();
        }


        public static void SetupServer()
        {
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            Console.WriteLine("Listening...");
            listener.Start();

            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("accept");
            NetworkStream nwStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            
            string path;
            string[] tempSplit;
            int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
            string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine(dataReceived);
            string dataJson="";
            if (dataReceived.IndexOf("GET")!=-1)
            {

                tempSplit = Regex.Split(dataReceived, @"\/|\s");
                path = tempSplit[2];
                switch(path)
                {
                    case "functions":
                        var data = new
                        {
                            BasicInformation = new
                            {
                                BranchName = "ABC",
                                DateFrom = "20180905",
                                DateTo = "20180905"
                            },
                            Details = "",
                            Header = new
                            {
                                Company = "C001",
                                BranchCode = "ABC123"
                            }
                        };

                        dataJson = JsonConvert.SerializeObject(data);

                        break;
                    default:
                        Console.WriteLine();
                        break;

                }
                byte[] DataSend= Encoding.UTF8.GetBytes(dataJson);
                nwStream.Write(DataSend, 0, DataSend.Length);
                Console.WriteLine("DataSend : "+DataSend);


            }
            if(dataReceived.IndexOf("POST")!=-1)
            {

            }



            Console.WriteLine("DataJson : " + dataJson);
            client.Close();
            listener.Stop();
            Console.ReadLine();
        }
    }
}
