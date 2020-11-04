using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using ClassesSolution;
using System.Text.RegularExpressions;
using System.IO;

namespace Client
{
    
    class Client
    {
        static string documentationPath = @"..\..\..\Documentation.txt";
        //This whole project is for testing a "tool".
        //Server info declaration.
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        /// Function - Main
        /// <summary>
        /// Handles the info recieving from the rest api server (Platform).
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string createParameters(ParametersType [] parameters)
        {
            string documentation = "";
            for(int i=0;i<parameters.Length;i++)
            {
                documentation += "* " + parameters[i].parameterName + " - \n";
            }
            return documentation;
        }
        public static void ExecuteServer(int port)
        {
            // Establish the local endpoint  
            // for the socket. Dns.GetHostName 
            // returns the name of the host  
            // running the application. 
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);
            Socket listener = new Socket(ipAddr.AddressFamily,
                                     SocketType.Stream, ProtocolType.Tcp);
            // Using Bind() method we associate a 
            // network address to the Server Socket 
            // All client that will connect to this  
            // Server Socket must know this network 
            // Address 
            listener.Bind(localEndPoint);

            // Using Listen() method we create  
            // the Client list that will want 
            // to connect to Server 
            // Creation TCP/IP Socket using  
            // Socket Class Costructor 

            while (true)
            {
                listener.Listen(10);
                Console.WriteLine("Waiting connection ... ");
                // Suspend while waiting for 
                // incoming connection Using  
                // Accept() method the server  
                // will accept connection of client 
                Socket clientSocket = listener.Accept();
                Console.WriteLine("Accepted");

                /*try
                {
                    clientSocket = listener.Accept();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }*/
                byte[] bytes = new Byte[1024];

                string data;
                int numByte = clientSocket.Receive(bytes);

                data = Encoding.ASCII.GetString(bytes,
                                            0, numByte);
                Task mytask=GetFromRestApi(data.Split(',')[0], data.Split(',')[1]);

                if (data.IndexOf("<EOF>") > -1)
                    break;

                byte[] message = Encoding.ASCII.GetBytes("Finish");
                clientSocket.Send(message);
                clientSocket.Close();
            }



        }
        static async Task GetFromRestApi(string sourcePath,string destPath)
        {
            //Communicating with rest api server
            HttpClient client = new HttpClient();
            string regexPattern = "";
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //Functions GET.
            HttpResponseMessage response = await client.GetAsync(string.Format("http://127.0.0.1:8081/functions?filePath={0}",sourcePath));
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            //Deserialize.
            Dictionary<string, FunctionInfoJson> dict = JsonConvert.DeserializeObject<Dictionary<string, FunctionInfoJson>>(responseBody);
            //Checking if it works (it does).
            string documentationTemplate = new MyStream(documentationPath, System.Text.Encoding.UTF8).ReadToEnd();
            string tempDocumentation = "";
            MyStream myStream = new MyStream(sourcePath, System.Text.Encoding.UTF8);
            StreamWriter destStream = new StreamWriter(destPath);
            string newFile=myStream.ReadToEnd();
            foreach (string key in dict.Keys)
            {
                ParametersType[] parameters = (ParametersType[])dict[key].parameters;
                regexPattern += @"(?s).*\@params.*\n";
                for (int i = 0; i < parameters.Length; i++)
                {
                    regexPattern += @".*" + parameters[i].parameterName + @".*\n";
                }
                regexPattern += @".*\@returns.*\n.*";
                Regex documentation = new Regex(regexPattern);
                if (!documentation.IsMatch(dict[key].documentation))
                {
                    tempDocumentation = createParameters(parameters);
                    string newDocumentation = string.Format(tempDocumentation, tempDocumentation);
                    if(dict[key].documentation!="")
                    {
                        newFile.Replace(dict[key].documentation, newDocumentation);
                    }
                    else
                    {
                        newFile.Replace(key, newDocumentation + '\n' + key);
                    }
                    Console.WriteLine(dict[key].documentation);


                }


            }
            destStream.Write(newFile);
            /*Console.WriteLine(dict["void spoi()"].documentation);
            //Code Info GET.
            response = await client.GetAsync("http://127.0.0.1:8081/codeInfo");
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();
            //Deserialize.
            CodeInfoJson code = JsonConvert.DeserializeObject<CodeInfoJson>(responseBody);
            //Checking if it works (it does).
            Console.WriteLine(code.definesAmount);
            Console.ReadLine();*/
        }
        static async Task Main(string[] args)
        {
            
        }
    }
}
