using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using ClassesSolution;
namespace Client
{
    class Client
    {
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
        static async Task Main(string[] args)
        {
            //Communicating with rest api server
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //Functions GET.
            HttpResponseMessage response = await client.GetAsync(@"http://127.0.0.1:8081/functions?filePath=C:\Users\Shenhav\Desktop\ShenhavArmyGit\shenhavArmyNew\tsetCCode\tsetCCode\test.c");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            //Deserialize.
            Dictionary<string, FunctionInfoJson> dict = JsonConvert.DeserializeObject<Dictionary<string, FunctionInfoJson>>(responseBody);
            //Checking if it works (it does).
            Console.WriteLine(dict["void spoi()"].documentation);
            //Code Info GET.
            response = await client.GetAsync("http://127.0.0.1:8081/codeInfo");
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();
            //Deserialize.
            CodeInfoJson code = JsonConvert.DeserializeObject<CodeInfoJson>(responseBody);
            //Checking if it works (it does).
            Console.WriteLine(code.definesAmount);
            Console.ReadLine();
        }
    }
}
