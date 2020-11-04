using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Server;
using Newtonsoft.Json;
using ClassesSolution;

namespace testServer2
{
    class MainProgram
    {
        //paths for all files.
        static bool compileError = false;
        static string ignoreVariablesTypesPath = @"..\..\..\ignoreVariablesType.txt";
        //static string filePath = @"C:\Users\Shenhav\Desktop\Check\checkOne.c";
        static string ansiCFile = @"..\..\..\Ansikeywords.txt";
        static string CSyntextFile = @"..\..\..\CSyntext.txt";
        static ArrayList currentDataList = new ArrayList();
        static int threadNumber = 0;
        static Dictionary<string, Dictionary<string, Object>> final_json = new Dictionary<string, Dictionary<string, Object>>();
        //static string librariesPath = @"C:\Users\Shenhav\Desktop\Check";
        //global variable declaration.

        //static ArrayList syntext = new ArrayList(); dont know if needed.

        /// Function - GetFinalJson
        /// <summary>
        /// Function returns the final json.
        /// </summary>
        /// <returns>final json type Dictionary<string,Dictionary<string,Object>> </returns>
        public static Dictionary<string,Dictionary<string,Object>> GetFinalJson()
        {
            return final_json;
        }
        /// Function - RunAllChecks
        /// <summary>
        /// Thread starts all checks.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pathes"></param>
        public static void RunAllChecks(string filePath, string [] pathes)
        {
            //variable declaration.
            Hashtable keywords = new Hashtable();
            Hashtable includes = new Hashtable();
            Dictionary<string, string> defines = new Dictionary<string, string>();
            Hashtable variables = new Hashtable();
            Console.WriteLine(filePath);
            //initialize 
            GeneralCompilerFunctions.initializeKeywordsAndSyntext(ansiCFile, filePath, CSyntextFile, ignoreVariablesTypesPath, keywords, includes, defines, pathes);
            Console.WriteLine("after initialize");
            GeneralCompilerFunctions.printArrayList(keywords);
            Console.WriteLine(keywords.Count);
            //Syntax Check.
            compileError=GeneralCompilerFunctions.SyntaxCheck(filePath, keywords,threadNumber);
            if(!compileError)
            {
                GeneralCompilerFunctions.printArrayList(keywords);
                Console.WriteLine(keywords.Count);

                //just tests.
                GeneralRestApiServerMethods.CreateFinalJson(filePath, includes, defines, final_json);
                string dataJson = JsonConvert.SerializeObject(final_json[filePath]["function"]);
                Console.WriteLine(filePath + "json : \n" + dataJson);
                /*Dictionary<string, FunctionInfoJson> checkIt = JsonConvert.DeserializeObject<Dictionary<string, FunctionInfoJson>>(dataJson);
                Console.WriteLine(checkIt["void spoi()"].documentation);*/
            }

            
            
        }
        static void Main(string[] args)
        {
            //open Rest API.
            Thread restApi = new Thread(()=>new SyncServer());
            restApi.Start();
            Console.WriteLine("started rest api");
            string filePath, projectPath, librariesPath, librariesPath2;
            //Initialize all the things that needs to come before the syntax check.
            Thread serverThread;
            //start server socket.
            serverThread = new Thread(() => Server.Program.ExecuteServer(11111));
            serverThread.Start();
            Console.WriteLine("started socket for client listen");
            while(true)
            {
                //checks if something got added to the server list by the gui. if it did 
                //it copies it to the main current list and start to run all the checks on the paths
                //got by the gui (the data inside the List is the user paths.).
                ArrayList list = Server.Program.GetThreadsData();
                if (list.Count > currentDataList.Count)
                {
                    //adds to the current data list the original server data list last node.
                    currentDataList.Add(list[currentDataList.Count]);
                    Console.WriteLine(currentDataList[currentDataList.Count - 1]);
                    string[] paths = Regex.Split((string)currentDataList[currentDataList.Count - 1], ",");
                    filePath = paths[0];
                    Console.WriteLine(filePath);
                    projectPath = paths[1];
                    librariesPath = paths[2];
                    librariesPath2 = paths[3];
                    string[] pathes = { projectPath, librariesPath, librariesPath2 };
                    
                    Thread thread = new Thread(() => RunAllChecks(filePath, pathes));
                    thread.Start();
                }
                else
                {
                    Thread.Sleep(1000);
                }
                
            }
            
            
            
            
            Console.ReadLine();
            
        }
    }
}
