using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/**
 * missing description
 */
class ToolPointerChecker
{

    static Regex functionPatternInH = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]\;$");
    static Regex staticFunctionPatternInC = new Regex(@"^.*static.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
    static Regex FunctionPatternInC = new Regex(@"^.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
    static Regex Allocation = new Regex(@"^.+[=].+malloc[(]|^.+[=].+alloc[(]|^.+[=].+calloc[(]");
    static Regex checkIfPoitnerIsntNull = new Regex(@"if.*\(.+null\)$|if.*\(!.+\)$|if.*\(.+NULL.*\)$");
    static Regex checkIfPointersArentNull = new Regex(@"if.*\(.+null\&\&\)$|if.*\(!.+\&\&\)$|if.*\(.+NULL\&\&.*\)$");
    static Regex FreePattern = new Regex(@"(?i)free\(.*\).");
    static Regex OpenBlockPattern = new Regex(@"{");
    static Regex CloseBlockPattern = new Regex(@"}");
    static Regex VariableEqual = new Regex(@"^.+\=.*[^()];$");
    static Regex FunctionCall = new Regex(@".+\=.*\(.+\);$|.*[^\s]\(.+\);$");
    static StreamWriter writeFile = new StreamWriter(@"C:\Users\Shenhav\Desktop\shenhavArmy\WarningsOnCode.txt");
    /// <summary>
    /// Main of the class
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        // TODO: support multiple pages
        string path = @"C:\Users\Shenhav\Desktop\shenhavArmy\tsetCCode\tsetCCode\test.c";
        writeFile.AutoFlush = true;
        CheckForAllocationErrorsAndFrees(path);
        CheckingFunctionParametersArentNull(path);
        CheckingFunctionDocumentation(staticFunctionPatternInC,path);
        Console.ReadLine();
    }
    /// <summary>
    /// the function goes threw the dictionary and checks if it matches the parameters to enter the dictionary
    /// if it does it adds it to the dictionary.
    /// </summary>
    /// <param name="dict"> the dictionary that saves all of the parameters that are being allocated .</param>
    /// <param name="param"> the parameter that is being equals to</param>
    /// <param name="newVariable">the variable that is being created</param>
    /// <returns>returns it it succeed</returns>
    public static bool AddToDictIfMatch(Dictionary<string,string>dict,string param,string newVariable)
    {
        bool found = false;
        Dictionary<string, string>.KeyCollection keys = dict.Keys;
        foreach (string key in keys)
        {
            if(key == param)
            {
                found = true;
                break;
            }
        }
        if(found)
        {
            dict.Add(newVariable, dict[param]);
        }
        return found;
    }
    /// <summary>
    /// remvoes all of the parameters from the dictionary
    /// </summary>
    /// <param name="dict"> the dictionary that saves the allocated parameters .</param>
    /// <param name="param"> the parameter that is being freed.</param>
    public static void RemoveAllMatchingValuesFromDict(Dictionary<string, string> dict, string param)
    {
        Dictionary<string, string>.KeyCollection keys = dict.Keys;
        Dictionary<string, string> tempDict=new Dictionary<string, string>();

        // add check if param is not in dict
        string value = dict[param];
        foreach (string key in keys)
        {
            if (dict[key] ==value )
            {
                tempDict.Add(key, value);
            }
        }
        Dictionary<string, string>.KeyCollection keysTemp = tempDict.Keys;
        foreach(string key in keysTemp)
        {
            dict.Remove(key);
        }
    }
    /// <summary>
    /// taking the second variable that isnt null in the array
    /// </summary>
    /// <param name="str"> the array that has the slpited parameter.</param>
    /// <returns> returns the second not null string in the array.</returns>
    public static string takeSecondNotNullString(string[] str)
    {
        int i;
        string result="";
        int count = 0;
        for(i=0;i<str.Length;i++)
        {
            if(str[i]!=""&& str[i] != " ")
            {
                count++;
            }
            if(count==2)
            {
                result = str[i];
                break;
            }
        }
        return result;
    }
    /// <summary>
    /// check if to add to the allocation dictionary the parameter.
    /// </summary>
    /// <param name="dict"> the aloocation dictionary.</param>
    /// <param name="s"> the string that is being checked.</param>
    public static void CheckIfAddToDict(Dictionary<string,string> dict,string s)
    {
        string[] split;
        string[] finalSplit;
        string parameter2;
        split = Regex.Split(s, @"\s\=\s|\s\=|\=\s|\=|\;");
        if ((split[0].IndexOf("\t*") != -1))
        {
            finalSplit = Regex.Split(split[0], @"\*");
            parameter2 = takeSecondNotNullString(finalSplit);
        }
        else if (split[0].IndexOf("*") != -1)
        {
            finalSplit = Regex.Split(split[0], @"\s\*\s|\s\*|\*\s|\*");
            parameter2 = takeSecondNotNullString(finalSplit);
        }
        else
        {
            finalSplit = Regex.Split(split[0], @"\s");
            parameter2 = takeSecondNotNullString(finalSplit);
        }
        AddToDictIfMatch(dict, split[1], parameter2);
    }
    /// <summary>
    /// check which of the pointers are being freed.
    /// </summary>
    /// <param name="dict"> the allocation dictionary.</param>
    public static void CheckWhatPointersArentFreed(Dictionary<string,string>dict)
    {
        Dictionary<string, string>.KeyCollection keys = dict.Keys;
        ArrayList al = new ArrayList();
        int i;
        foreach (string key in keys)
        {
            if(al.IndexOf(dict[key])<0)
            {
                al.Add(dict[key]);
            }
        }
        for(i=0;i<al.Count;i++)
        {
            Console.WriteLine("Pointer {0} isnt being free'd",al[i]);
            writeFile.WriteLine("Pointer {0} isnt being free'd", al[i]);
        }
    }
    /// <summary>
    /// skips the documentation in the code.
    /// </summary>
    /// <param name="sr"> the streamReader file</param>
    /// <param name="s"> the string which the function is at right now.</param>
    /// <param name="rowNumber"> the row number the function is at.</param>
    public static void SkipDocumentation(MyStream sr, ref string s, int rowNumber = 0)
    {
        while ((s.IndexOf("//") != -1)) //dry
        {
            rowNumber++;
            if (s != null)
                s = sr.ReadLine();

        }
        if ((s.IndexOf("/*") != -1))
        {
            while (!(s.IndexOf("*/") != -1))
            {
                rowNumber++;
                if (s != null)
                    s = sr.ReadLine();
            }
            if ((s.IndexOf("*/") != -1))
            {
                if (s != null)
                    s = sr.ReadLine();
            }
        }
    }
    /// <summary>
    /// checks for allocation errors and frees.
    /// </summary>
    public static void CheckForAllocationErrorsAndFrees(string path)
    {
        int rowNumber = 0;
        Dictionary<string, string> dict = new Dictionary<string, string>();
        string s;
        int temp = 0;
        string[] parameterName;
        
        MyStream sr = new MyStream(path, System.Text.Encoding.UTF8); // file might not be in ascii only 
        while ((s = sr.ReadLine()) != null)
        {
            rowNumber++;

            if(VariableEqual.IsMatch(s))
            {
                CheckIfAddToDict(dict, s);
            }
            if (Allocation.IsMatch(s))
            {
                parameterName = Regex.Split(s, @"\*\s|\s\=");
                dict.Add(parameterName[1], parameterName[1]);
                temp = rowNumber;
                rowNumber++;
                if (s != null)
                    s = sr.ReadLine();
                SkipDocumentation(sr, ref s);
                if (VariableEqual.IsMatch(s))
                {
                    CheckIfAddToDict(dict, s);
                }
                if (!checkIfPoitnerIsntNull.IsMatch(s))
                {
                    Console.WriteLine("not checking if allocation succeeded for parameter " + parameterName[1]);
                    writeFile.WriteLine("not checking if allocation succeeded for parameter " + parameterName[1]);
                }

            }
            if (FreePattern.IsMatch(s))
            {
                parameterName = Regex.Split(s, @"\(|\)");
                RemoveAllMatchingValuesFromDict(dict, parameterName[1]);
            }


        }

        CheckWhatPointersArentFreed(dict);
        sr.Close();

    }
    /// <summary>
    /// gives the length of a function
    /// </summary>
    /// <param name="sr"> the streamReader the function is at right now/param>
    /// <returns>the length of a function/returns>
    public static int FunctionLength(MyStream sr)
    {
        uint curPos = sr.Pos;
        int functionLength = 0;
        string s;
        Stack myStack = new Stack();
        s = sr.ReadLine();
        myStack.Push(s);
        while ((s = sr.ReadLine()) != null&&myStack.Count>0)
        {
            functionLength++;
            if (OpenBlockPattern.IsMatch(s))
            {
                myStack.Push(s);
            }
            if(CloseBlockPattern.IsMatch(s))
            {
                myStack.Pop();
            }
        }
        sr.Seek(curPos);
        myStack.Clear();
        return functionLength;
    }
    /// <summary>
    /// the function counts how many substrings are in a string.
    /// </summary>
    /// <param name="str">the string you check the substring on.</param>
    /// <param name="substr">the substring you check on the string.</param>
    /// <returns>it returns the amount of substring in a string by int</returns>
    public static int countSubstrInStr(string str, string substr)
    {
        int count = 0;
        int minIndex = str.IndexOf(substr,0);
        while (minIndex != -1)
        {
            minIndex = str.IndexOf(substr, minIndex + substr.Length);
            count++;
        }
        return count;
    }
    /// <summary>
    /// finds the parameter in the code line.
    /// </summary>
    /// <param name="s">the code line</param>
    /// <returns> returns an array of the parameters in the code line.</returns>
    public static string []findParameters(string s)
    {
        string[] tempSplit;
        string[] finalSplit;
        string tempSplit2;
        int i, j;
        tempSplit = Regex.Split(s, @"\(");
        tempSplit2 = tempSplit[1];
        tempSplit = Regex.Split(tempSplit2, @"\,|\)");
        string[] finalParameters = new string[tempSplit.Length - 1];
        char[] charsToTrim = { '*','&'};
        if(tempSplit2.Length>2)
        {
            for (i = 0; i < tempSplit.Length - 1; i++)
            {
                tempSplit2 = tempSplit[i];
                tempSplit2=tempSplit2.Replace('*',' ');
                tempSplit2 = tempSplit2.Trim();
                finalSplit = Regex.Split(tempSplit2, @"\s");
                if(finalSplit.Length==1)
                {
                    tempSplit2 = finalSplit[0];
                }
                else
                {
                    tempSplit2 = takeSecondNotNullString(finalSplit);
                }
                if(tempSplit2.IndexOf("&")!=-1||tempSplit2.IndexOf("*")!=-1)
                {
                    tempSplit2=tempSplit2.Trim(charsToTrim);
                }
                finalParameters[i] = tempSplit2;
            }
        }
        else
        {
            finalParameters = new string[0];
        }
        return finalParameters;
    }
    /// <summary>
    /// returns a string of the function Name from the code line.
    /// </summary>
    /// <param name="s">the code line</param>
    /// <returns> returns the function name</returns>
    public static string FunctionNameSplit(string s)
    {
        string functionName = "";
        string[] BracketSplit = Regex.Split(s, @"\(");
        if(BracketSplit[0].IndexOf("*")!=-1)
        {
            BracketSplit=Regex.Split(BracketSplit[0], @"\*");
            functionName=takeSecondNotNullString(BracketSplit);
           
        }
        else
        {
            BracketSplit = Regex.Split(BracketSplit[0], @"\s");
            for(int i=0;i<BracketSplit.Length;i++)
            {
                if (BracketSplit[i] != "static")
                {
                    functionName = BracketSplit[i + 1];
                    break;
                }

            }
        }
        return functionName;
    }
    /// <summary>
    /// checking the max parameters in the function in the code.
    /// </summary>
    /// <param name="dict">the dictionary that saves the parameters of the functions.</param>
    /// <returns>returns the maximum parameters</returns>
    public static int mostParametersInDictFor1Func(Dictionary<string, Dictionary<int, string>> dict)
    {
        int max = 0;
        Dictionary<string, Dictionary<int, string>>.KeyCollection keys = dict.Keys;
        int[] tempArr = new int[dict.Count];
        Array.Clear(tempArr, 0, tempArr.Length);
        foreach (string value in keys)
        {
            if (dict[value].Count > max)
                max = dict[value].Count;
        }
        return max;
    }

    //function too long.
    /// <summary>
    /// check if the function parameters are being checked if the are null.
    /// </summary>
    public static void CheckingFunctionParametersArentNull(string path)
    {
        string s;
        Dictionary<string, string> dict = new Dictionary<string, string>();
        Dictionary<string, Dictionary<int,string>> FinalDict = new Dictionary<string, Dictionary<int, string>>();
        int i, j;
        bool found;
        string functionName="";
        MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
        uint functionPos;
        string[] finalParameters;
        while ((s = sr.ReadLine()) != null)
        {
            while ((!FunctionPatternInC.IsMatch(s))&& ((s = sr.ReadLine()) != null))
            {
            }
            if (s == null)
            {
                break;
            }
            Dictionary<int, string> tempList = new Dictionary<int, string>();
            functionName = FunctionNameSplit(s);
            functionName = functionName.Trim();
            finalParameters = findParameters(s);
            functionPos = sr.Pos;
            int functionLength = FunctionLength(sr);
            for (i=0;i<finalParameters.Length;i++)
            {
                sr.Seek(functionPos);
                found = false;
                for (j = 0; j < functionLength; j++)
                {
                    if(checkIfPointersArentNull.IsMatch(s)&&s.IndexOf(finalParameters[i])!=-1)
                    {
                        found = true;
                        continue;
                    }
                    s = sr.ReadLine();
                    if((s.IndexOf(finalParameters[i])!=-1)&&checkIfPoitnerIsntNull.IsMatch(s))
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    
                    tempList.Add(i,finalParameters[i]);
                }
            }
            FinalDict.Add(functionName, tempList);
        }
        sr.Close();
        sr = new MyStream(path, System.Text.Encoding.UTF8); //call destruction
        functionPos = 0;
        found=false;
        string[] parameters;
        int max = mostParametersInDictFor1Func(FinalDict);
        Dictionary<string, Dictionary<int,string>>.KeyCollection keys = FinalDict.Keys;
        ArrayList functionNames = new ArrayList();
        ArrayList parameterNames = new ArrayList();
        ArrayList ifPatterns = new ArrayList();
        Dictionary<string, string> al = new Dictionary<string, string>();
        while ((s = sr.ReadLine()) != null)
        {
            found = false;
            if(FunctionCall.IsMatch(s))
            {
                foreach (var key in keys)
                {
                    if (s.IndexOf(key) != -1)
                    {
                        found = true;
                        functionName = key;
                    }
                }
            }
            if (checkIfPoitnerIsntNull.IsMatch(s)||checkIfPointersArentNull.IsMatch(s))
            {
                ifPatterns.Add(s);
                if (ifPatterns.Count > max)
                {
                    ifPatterns.RemoveAt(0);
                }
            }


            if (found)
            {
                parameters = findParameters(s);
                for (i = 0; i < parameters.Length; i++)
                {
                    foreach (string param in ifPatterns)
                    {
                        if ((param.IndexOf(parameters[i]) != -1))
                        {
                            FinalDict[functionName].Remove(i);
                        }
                    }
                }
            }
        }
        foreach(var dic in FinalDict.Values)
        {
            foreach (var val in dic.Values)
            {
                Console.WriteLine("Parameter {0} isnt being checked if he is NULL in function", val);
                writeFile.WriteLine("Parameter {0} isnt being checked if he is NULL in function", val);
            }
        }
        sr.Close();
            
    }
    /// <summary>
    /// checking if there is a good documentation for every function.
    /// </summary>
    /// <param name="functionPattern"></param>
    public static void CheckingFunctionDocumentation(Regex functionPattern,string path)
    {
        string s;
        string functionLine;
        string documetationString = "";
        string firstLineDocumentation="";
        bool exitFlag = false;
        MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
        uint documentPos=sr.Pos;
        uint functionPos;
        string[] functionParameters;
        string regexPattern="";
        bool found;
        while (!exitFlag&&(s = sr.ReadLine()) != null)
        {
            found = true;
            while (!exitFlag&&!functionPattern.IsMatch(s))
            {
                if(!found)
                {
                    s = sr.ReadLine();
                }
                found = false;
                firstLineDocumentation = "";
                if (s==null)
                {
                    exitFlag = true;
                }
                if(s.IndexOf("//")!=-1)
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
                        if(s!=null)
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
            if(!exitFlag)
            {
                functionParameters = findParameters(s);
                functionPos = sr.Pos;
                functionLine = s;
                regexPattern += @"(?s).*\@params.*\n";
                for (int i = 0; i < functionParameters.Length; i++)
                {
                    regexPattern += @".*" + functionParameters[i] + @".*\n";
                }
                regexPattern += @".*\@returns.*\n.*";
                Regex Documentation = new Regex(regexPattern);
                regexPattern = "";
                sr.Seek(documentPos);
                if (!(firstLineDocumentation.IndexOf("//") != -1) && !(firstLineDocumentation.IndexOf("/*") != -1))
                {
                    Console.WriteLine("no documentation for function {0}", functionLine);
                    writeFile.WriteLine("no documentation for function {0}", functionLine);
                    sr.Seek(functionPos);
                    documetationString = "";
                    continue;
                }
                s = sr.ReadLine();
                if ((firstLineDocumentation.IndexOf("/*") != -1))
                {
                    while (!(s.IndexOf("*/") != -1))
                    {
                        s = sr.ReadLine();
                        documetationString += s + "\n";
                    }

                }
                if (!Documentation.IsMatch(documetationString))
                {
                    Console.WriteLine("documentation is written but not in the recommendation in function line {0}", functionLine);
                    writeFile.WriteLine("documentation is written but not in the recommendation in function line {0}", functionLine);
                }

                else
                {
                    Console.WriteLine("GOOD");
                    writeFile.WriteLine("GOOD");
                }
                sr.Seek(functionPos);
                documetationString = "";
                firstLineDocumentation = "";
            }
        }
            
        Console.ReadLine();
    }
}
