using System;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Platformaa;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace testServer
{
    class Program
    {
        static Regex OpenBlockPattern = new Regex(@"{");
        static Regex CloseBlockPattern = new Regex(@"}");
        static Regex functionPatternInH = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]\;$");
        static Regex staticFunctionPatternInC = new Regex(@"^.*static.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        static Regex FunctionPatternInC = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        static string filePath = @"C:\\Users\\shenhav.mor\\Desktop\\shenhavArmy\\shenhavArmy\\tsetCCode\\tsetCCode\\test.c";
        public class SyncServer
        {
            public SyncServer()
            {
                var listener = new HttpListener();

                listener.Prefixes.Add("http://localhost:8081/");
                listener.Prefixes.Add("http://127.0.0.1:8081/");

                listener.Start();

                while (true)
                {
                    try
                    {
                        var context = listener.GetContext(); //Block until a connection comes in
                        context.Response.StatusCode = 200;
                        context.Response.SendChunked = true;
                        context.Response.ContentType = "application/json";
                        string dataJson = "";
                        Console.WriteLine(context.Request.Headers);
                        char[] trimChars = { '/', ' ' };
                        int totalTime = 0;
                        string path = "";
                        if (context.Request.HttpMethod == "GET")
                        {
                            path = context.Request.RawUrl;
                            path = path.Trim(trimChars);
                            Console.WriteLine(path);
                            switch (path)
                            {
                                case "functions":
                                    Console.WriteLine("dddd");
                                    dataJson = findAllFunctionNamesAndCode(filePath, FunctionPatternInC);
                                    Console.WriteLine(dataJson);
                                    break;
                                default:
                                    Console.WriteLine();
                                    break;

                            }
                            var bytes = Encoding.UTF8.GetBytes(dataJson);
                            Stream OutputStream = context.Response.OutputStream;
                            OutputStream.Write(bytes, 0, bytes.Length);
                            OutputStream.Close();

                        }
                    }


                    catch (Exception)
                    {
                        // Client disconnected or some other error - ignored for this example
                    }
                }
            }
        }
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
        public static string FunctionCode(MyStream sr)
        {
            uint curPos = sr.Pos;
            int functionLength = 0;
            string s;
            string finalCode = "";
            Stack myStack = new Stack();
            s = sr.ReadLine();
            myStack.Push(s);
            while ((s = sr.ReadLine()) != null && myStack.Count > 0)
            {
                finalCode += s + "\n\r";
                functionLength++;
                if (OpenBlockPattern.IsMatch(s))
                {
                    myStack.Push(s);
                }
                if (CloseBlockPattern.IsMatch(s))
                {
                    myStack.Pop();
                }
                //here will be where i will store the function code.

            }
            myStack.Clear();
            return finalCode;
        }
        public static string findFunction(MyStream sr, Regex pattern)
        {
            bool found = false;
            string s = sr.ReadLine();
            while ((!pattern.IsMatch(s)) && ((s = sr.ReadLine()) != null)) ;
            return s;
        }
        public static void findAllFunctionNames(string path, Regex pattern)
        {
            string s = "";
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            while (s != null)
            {
                s = findFunction(sr, pattern);
                //enter function to where i store it.
                //add it to where i store the function code.
            }
            sr.Close();
        }
        public static string takeSecondNotNullString(string[] str)
        {
            int i;
            string result = "";
            int count = 0;
            for (i = 0; i < str.Length; i++)
            {
                if (str[i] != "" && str[i] != " ")
                {
                    count++;
                }
                if (count == 2)
                {
                    result = str[i];
                    break;
                }
            }
            return result;
        }
        public static ParametersType[] findParameters2(string s)
        {
            string[] tempSplit;
            string[] finalSplit;
            string tempSplit2;
            string finalType;
            int i, j;
            tempSplit = Regex.Split(s, @"\(");
            tempSplit2 = tempSplit[1];
            tempSplit = Regex.Split(tempSplit2, @"\,|\)");
            ParametersType[] finalParameters = new ParametersType[tempSplit.Length - 1];
            char[] charsToTrim = { '*', '&' };
            if (tempSplit2.Length > 2)
            {
                for (i = 0; i < tempSplit.Length - 1; i++)
                {
                    tempSplit2 = tempSplit[i];
                    if (tempSplit2.IndexOf("*") != -1)
                    {
                        finalSplit = Regex.Split(tempSplit2, @"\*");
                    }
                    else
                    {
                        finalSplit = Regex.Split(tempSplit2, @"\s");

                    }

                    if (finalSplit.Length == 1)
                    {
                        tempSplit2 = finalSplit[0];
                    }
                    else
                    {
                        tempSplit2 = takeSecondNotNullString(finalSplit);
                    }
                    if (tempSplit2.IndexOf("&") != -1 || tempSplit2.IndexOf("*") != -1)
                    {
                        tempSplit2 = tempSplit2.Trim(charsToTrim);
                    }
                    //trimEnd
                    tempSplit[i] = tempSplit[i].Substring(0, tempSplit[i].Length - (tempSplit2.Length));
                    finalType = tempSplit[i].Replace(" ", "");
                    tempSplit2 = tempSplit2.Replace(" ", "");
                    finalParameters[i] = new ParametersType(tempSplit2, finalType);

                }
            }
            else
            {
                finalParameters = new ParametersType[0];
            }
            return finalParameters;
        }
        public static string findDocumentation(MyStream sr, uint documentation, string firstLineDocumentation, uint functionPos)
        {
            string documetationString = firstLineDocumentation + "\n\r";
            sr.Seek(documentation);
            string s = sr.ReadLine();
            documetationString += s + "\n\r";
            if (!(firstLineDocumentation.IndexOf("//") != -1) && !(firstLineDocumentation.IndexOf("/*") != -1))
            {
                documetationString = "No documentation for this function";
            }
            if ((firstLineDocumentation.IndexOf("/*") != -1))
            {
                while (!(s.IndexOf("*/") != -1))
                {
                    s = sr.ReadLine();
                    documetationString += s + "\n\r";
                }

            }
            sr.Seek(functionPos);
            return documetationString;

        }
        public static string findAllFunctionNamesAndCode(string path, Regex pattern)
        {
            string s = "";
            string fName;
            string[] temp;
            string returnType = "";
            bool exitFlag = false;
            bool found;
            string firstLineDocumentation = "";
            uint curPos;
            Dictionary<string, FunctionInfoJson> tempDict = new Dictionary<string, FunctionInfoJson>();
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            uint documentPos = sr.Pos;
            while (s != null)
            {
                while (!exitFlag && !FunctionPatternInC.IsMatch(s))
                {
                    if (s != null)
                    {
                        s = sr.ReadLine();
                    }
                    firstLineDocumentation = "";
                    if (s == null)
                    {
                        exitFlag = true;
                    }
                    if (s.IndexOf("//") != -1)
                    {
                        documentPos = sr.Pos;
                        firstLineDocumentation = s;
                    }
                    while ((s.IndexOf("//") != -1))
                    {
                        if (s != null)
                            s = sr.ReadLine();
                    }
                    if ((s.IndexOf("/*") != -1))
                    {
                        documentPos = sr.Pos;
                        firstLineDocumentation = s;
                        while (!(s.IndexOf("*/") != -1))
                        {
                            if (s != null)
                                s = sr.ReadLine();
                        }
                        if ((s.IndexOf("*/") != -1))
                        {
                            if (s != null)
                                s = sr.ReadLine();
                        }
                    }
                    if (s == null)
                    {
                        exitFlag = true;
                    }
                }
                if (s == null)
                {
                    exitFlag = true;
                }
                if (!exitFlag)
                {
                    fName = s;
                    if (fName != null)
                    {
                        temp = Regex.Split(fName, @"\*|\s");
                        if (fName.IndexOf("static") != -1)
                        {
                            returnType = takeSecondNotNullString(temp);
                        }
                        else
                        {
                            returnType = temp[0];
                        }

                        returnType = returnType.Trim();
                        //enter function to where i store it. 
                        FunctionInfoJson tempStorage = new FunctionInfoJson();
                        tempStorage.content = FunctionCode(sr, ref s);
                        tempStorage.parameters = findParameters2(fName);
                        tempStorage.returnType = returnType;
                        curPos = sr.Pos;
                        tempStorage.documentation = findDocumentation(sr, documentPos, firstLineDocumentation, curPos);
                        tempDict.Add(fName, tempStorage);
                    }
                    else
                    {
                        exitFlag = true;
                    }
                }


                //add it to where i store the function code.
            }
            string finalJson = JsonConvert.SerializeObject(dict);
            sr.Close();
            return finalJson;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("enter pattern for variable structure");
            string VarPattern=Console.ReadLine();
            Regex VarRegexPattern = new Regex(@"{0}", VarPattern);


            new SyncServer();
        }

    }
}
