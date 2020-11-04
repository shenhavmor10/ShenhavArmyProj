using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization;
using System.Net.Http.Headers;

namespace Client
{
    public class FunctionInfoJson
    {

        public string content;
        public ParametersType[] parameters;
        public string returnType;
        public string documentation;
    }
    public class ParametersType
    {
        public string parameterName;
        public string parameterType;
        public ParametersType(string parameterName, string parameterType)
        {
            this.parameterType = parameterType;
            this.parameterName = parameterName;
        }
    }
    class Client
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        //static void Main(string[] args)
        //{
        //    ConnectToServer();
        //}
        static async Task Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:8081/functions");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
            Dictionary<string, FunctionInfoJson> dict = JsonConvert.DeserializeObject<Dictionary<string, FunctionInfoJson>>(responseBody);
            Console.WriteLine(dict["static int  * main(int * podd,int ** odpdf,char a, char * retval)"].content);





            //WebRequest request = WebRequest.Create("http://127.0.0.1:5000/functions");
            //request.Credentials = CredentialCache.DefaultCredentials;

            //WebResponse response = request.GetResponse();
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            //Stream dataStream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(dataStream);
            //string responeFromServer = reader.ReadToEnd();
            //Console.WriteLine(responeFromServer);
            //response.Close();
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(
            //    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            //var result = await client.GetStringAsync("http://127.0.0.1:5000/functions");

            //Console.WriteLine(result);
            Console.ReadLine();
        }
        public static void ConnectToServer()
        {
            string textToSend = DateTime.Now.ToString();

            TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
            NetworkStream nwStream = client.GetStream();
            byte[] bytesToSend = UTF8Encoding.UTF8.GetBytes(textToSend);

            Console.WriteLine("Sending : "+textToSend);
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead=nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
            Console.WriteLine("Received : "+Encoding.UTF8.GetString(bytesToRead));
            client.Close();
            Console.ReadLine();            
            
            
        }
    }
}
