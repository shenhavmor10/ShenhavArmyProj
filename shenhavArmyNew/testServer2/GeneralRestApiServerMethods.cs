using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ClassesSolution;
using System.Security;
using Server;
using System.IO;
using System;
using System.Security.Cryptography;

namespace testServer2
{

    class GeneralRestApiServerMethods
    {
        //Patterns Declaration.
        static Regex OpenBlockPattern = new Regex(@".*{.*");
        static Regex CloseBlockPattern = new Regex(@".*}.*");
        static Regex FunctionPatternInC = new Regex(@"^([^ ]+\s)?[^ ]+\s(.*\s)?[^ ]+\([^()]*\)$");
        
        /// Function - FunctionCode
        /// <summary>
        /// function gets a buffer and a refference to the code line and returns all the function code.
        /// in the scope.
        /// </summary>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="s"> code line type string.</param>
        /// <returns> returns the whole function code. </returns>
        public static string FunctionCode(MyStream sr, ref string s)
        {
            uint curPos = sr.Pos;
            int functionLength = 0;
            string finalCode = "";
            Stack myStack = new Stack();
            s = sr.ReadLine();
            myStack.Push(s);
            while ((s != null && myStack.Count > 0))
            {
                s = sr.ReadLine();
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
            }
            myStack.Clear();
            return finalCode;
        }
        /// Function - FunctionLength
        /// <summary>
        /// gets the function line and buffer returns the function length.
        /// </summary>
        /// <param name="sr"> Buffer type MyStream.</param>
        /// <param name="s"> Code line type string</param>
        /// <returns> returns the function length type int.</returns>
        public static int FunctionLength(MyStream sr, string s)
        {
            int count = 0;
            uint curPos = sr.Pos;
            Stack myStack = new Stack();
            s = sr.ReadLine();
            myStack.Push(s);
            bool found = false;
            while ((s != null && myStack.Count > 0))
            {
                count++;
                s = sr.ReadLine();
                if (s.IndexOf("{") != -1)
                {
                    myStack.Push(s);
                }
                if (s.IndexOf("}") != -1)
                {
                    myStack.Pop();
                }

            }
            if (myStack.Count == 0)
            {
                found = true;
            }
            count = count - 1;
            myStack.Clear();
            //returns the buffer to the start of the function.
            sr.Seek(curPos);
            return count;

        }
        /// Function - findFunction
        /// <summary>
        /// find the next function in the code and returns the function line.
        /// </summary>
        /// <param name="sr"> Buffer type MyStream.</param>
        /// <param name="pattern"> Regex Pattern for the function.</param>
        /// <returns></returns>
        public static string findFunction(MyStream sr, Regex pattern)
        {
            bool found = false;
            string s = sr.ReadLine();
            while ((!pattern.IsMatch(s)) && ((s = sr.ReadLine()) != null)) ;
            return s;
        }
        /// Function - findAllFunctionNames
        /// <summary>
        /// find all the function names in the code.
        /// </summary>
        /// <param name="path"> Path for the code.</param>
        /// <param name="pattern"> Pattern for the function.</param>
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
        /// Function - takeSecondNotNullString
        /// <summary>
        /// take the second not null string in the string array.
        /// </summary>
        /// <param name="str"> array after split type string array.</param>
        /// <returns> returns the string that is in the second place that isnt null in the array . type string.</returns>
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
        /// Function - FindParameters
        /// <summary>
        /// get a string of a function and returns all the parameters of the function in a parameter type array.
        /// </summary>
        /// <param name="s"> function line type string.</param>
        /// <returns> returns all of the parameters in an array type ParemetrsType.</returns>
        public static ParametersType[] FindParameters(string s)
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
        /// Function - FindDocumentation
        /// <summary>
        /// Finds the documentation of a function.
        /// </summary>
        /// <param name="sr"> Buffer type MyStream.</param>
        /// <param name="documentation"> Position of the first documentation line type uint.</param>
        /// <param name="firstLineDocumentation"> First documentation line type string.</param>
        /// <param name="functionPos"> Position of the function type uint.</param>
        /// <returns> returns the documentation of the function included.</returns>
        public static string FindDocumentation(MyStream sr, uint documentation, string firstLineDocumentation, uint functionPos)
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
        /// Function - CreateFunctionsJsonFile
        /// <summary>
        /// create a json file for functions.
        /// </summary>
        /// <param name="path"> path of the code.</param>
        /// <param name="pattern"> function pattern type string</param>
        /// <returns> return a json for the functions get in "SyncServer".</returns>
        public static void CreateFinalJson(string filePath,Hashtable includes,Dictionary<string,string>defines, Dictionary<string, Dictionary<string, Object>>final_json)
        {
            CreateFunctionsJsonFile(filePath, FunctionPatternInC,final_json);
            CreateCodeJsonFile(filePath,includes,defines,final_json);
        }
        
        public static void CreateFunctionsJsonFile(string path, Regex pattern, Dictionary<string, Dictionary<string,Object>> final_json)
        {
            string s = "";
            string fName;
            string[] temp;
            string returnType = "";
            bool exitFlag = false;
            bool found;
            string firstLineDocumentation = "";
            uint curPos;
            Object tempDict = new Dictionary<string, FunctionInfoJson>();
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            uint documentPos = sr.Pos;
            while (s != null)
            {
                //saves the last documentation.
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
                        break;
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
                        Object tempStorage = new FunctionInfoJson();
                        GeneralCompilerFunctions.NextScopeLength(sr, ref s, ref ((FunctionInfoJson)tempStorage).codeLength, true);
                        ((FunctionInfoJson)tempStorage).content = FunctionCode(sr, ref s);
                        ((FunctionInfoJson)tempStorage).parameters = FindParameters(fName);
                        ((FunctionInfoJson)tempStorage).returnType = returnType;
                        curPos = sr.Pos;
                        ((FunctionInfoJson)tempStorage).documentation = FindDocumentation(sr, documentPos, firstLineDocumentation, curPos);
                       ((Dictionary<string,FunctionInfoJson>)tempDict).Add(fName, (FunctionInfoJson)tempStorage);
                    }
                    else
                    {
                        exitFlag = true;
                    }

                }
                //add it to where i store the function code.
            }
            //Serialize.
            Dictionary<string, Object> tempOuterDict=new Dictionary<string, Object>();
            tempOuterDict.Add("function", tempDict);
            final_json.Add(path, tempOuterDict);
            sr.Close();
        }
        /// Function - CreateCodeJsonFile
        /// <summary>
        /// Creates a Json file for the Code get.
        /// </summary>
        /// <param name="includes"> Hashtable includes for all of the includes in the code.</param>
        /// <param name="defines"> Dictionary of defines that has all defines in the code. 
        ///                        (Including all imports defines.)</param>
        /// <returns> returns a json file type string.</returns>
        public static void CreateCodeJsonFile(string path,Hashtable includes,Dictionary<string,string>defines, Dictionary<string, Dictionary<string, Object>> final_json)
        {
            CodeInfoJson code=new CodeInfoJson();
            code.includes = new string[includes.Values.Count];
            includes.Values.CopyTo(code.includes, 0);
            code.includesAmount = includes.Values.Count;
            code.defines = defines;
            code.definesAmount = defines.Count;
            //Serialize.
            final_json[path].Add("codeInfo",code);

        }
    }
}
