using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
class ToolPointerChecker2
{

    static Regex functionPattern = new Regex(@"^.+\s.+[(].*[)]$");
    static Regex Allocation = new Regex(@"^.+[=].+malloc[(]|^.+[=].+alloc[(]|^.+[=].+calloc[(]");
    static Regex checkIfPoitnerIsntNull = new Regex(@"if.*\(.+null\)$|if.*\(!.+\)$|if.*\(.+NULL.*\)$");
    static Regex FreePattern = new Regex(@"(?i)free\(.*\).");
    static Regex OpenBlockPattern = new Regex(@"{");
    static Regex CloseBlockPattern = new Regex(@"}");
    static int rowNumber = 0;
    static void Main(string[] args)
    {

        CheckForAllocationErrorsAndFrees();
        CheckingFunctionParametersArentNull();
    }

    public static MyStream NextFunction(MyStream sr, string s)
    {
        while ((!functionPattern.IsMatch(s)) && ((s = sr.ReadLine()) != null))
        {
            rowNumber++;
        }
        Console.WriteLine("\n" + s + "\n");
        return sr;
    }
    public static string ReverseString(string myStr)
    {
        char[] myArr = myStr.ToCharArray();
        Array.Reverse(myArr);
        return new string(myArr);
    }
    public static void CheckForAllocationErrorsAndFrees()
    {
        //Hashtable ht = new Hashtable();
        Dictionary<string, int> dict = new Dictionary<string, int>();
        string s;
        string path = @"C:\Users\shenhav.mor\source\repos\tsetCCode\tsetCCode\test.c";
        int temp = 0;
        string[] parameterName;
        MyStream sr = new MyStream(path, System.Text.Encoding.ASCII);
        while ((s = sr.ReadLine()) != null)
        {
            rowNumber++;
            if (Allocation.IsMatch(s))
            {
                parameterName = Regex.Split(s, @"\*\s|\s\=");
                Console.WriteLine(parameterName[1]);
                dict.Add(parameterName[1], rowNumber);
                // ht.Add(parameterName[1], rowNumber);
                temp = rowNumber;
                rowNumber++;
                s = sr.ReadLine();
                while ((s.IndexOf("//") != -1))
                {
                    rowNumber++;
                    s = sr.ReadLine();
                }
                if ((s.IndexOf("/*") != -1))
                {
                    while (!(s.IndexOf("*/") != -1))
                    {
                        rowNumber++;
                        s = sr.ReadLine();
                    }
                }
                if (!checkIfPoitnerIsntNull.IsMatch(s))
                {
                    Console.WriteLine("not checking if allocation succeeded in line " + temp);
                }

            }
            if (FreePattern.IsMatch(s))
            {
                parameterName = Regex.Split(s, @"\(|\)");
                Console.WriteLine(parameterName[1]);
                dict.Remove(parameterName[1]);
                Console.WriteLine("after remove");
                Console.WriteLine(dict.Count);
                //ht.Remove(parameterName[1]);
            }
            //sr = NextFunction(sr, s);

        }
        // if(ht.Count!=0)
        //  {

        // }
        Dictionary<string, int>.KeyCollection keys = dict.Keys;
        foreach (string key in keys)
        {
            Console.WriteLine("parameter {0} didnt free'd", key);
        }
        sr.Close();

    }
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
        return functionLength;
    }
    public static void CheckingFunctionParametersArentNull()
    {
        string s;
        string path = @"C:\Users\shenhav.mor\source\repos\tsetCCode\tsetCCode\test.c";
        string [] tempSplit;
        string[] finalSplit;
        string tempSplit2;
        int i, j;
        bool found = false;
        MyStream sr = new MyStream(path, System.Text.Encoding.ASCII);
        uint functionPos;
        while ((s = sr.ReadLine()) != null)
        {
            while ((!functionPattern.IsMatch(s)) && ((s = sr.ReadLine()) != null))
            {
                rowNumber++;
            }
            Console.WriteLine("Function : {0}",s);
            tempSplit = Regex.Split(s, @"\(");
            tempSplit2 = tempSplit[1];
            tempSplit = Regex.Split(tempSplit2, @"\,|\)");
            string[] finalParameters = new string [tempSplit.Length-1];
            Console.WriteLine("length "+tempSplit.Length);
            for (i=0;i<tempSplit.Length-1;i++)
            {
                tempSplit2 = tempSplit[i];
                if(tempSplit2.IndexOf("*")!=-1)
                {
                    finalSplit = Regex.Split(tempSplit2, @"\*\s");
                }
                else
                {
                    finalSplit = Regex.Split(tempSplit2, @"\s");
                }
                tempSplit2 = finalSplit[1];
                finalParameters[i] = tempSplit2;

            }

            int functionLength = FunctionLength(sr);
            Console.WriteLine("parameter length {0}",finalParameters.Length);
            s = sr.ReadLine();
            Console.WriteLine("next string "+s);
            functionPos = sr.Pos;
            Console.WriteLine("Function pos {0}",functionPos);
            for (i=0;i<finalParameters.Length;i++)
            {
                sr.Seek(functionPos);
                Console.WriteLine("Function pos {0}", sr.Pos);
                found = false;
                for (j = 0; j < functionLength; j++)
                {
                    s = sr.ReadLine();
                    if((s.IndexOf(finalParameters[i])!=-1)&&checkIfPoitnerIsntNull.IsMatch(s))
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    Console.WriteLine("Parameter {0} inst being checked if he is null", finalParameters[i]);
                }
            }
        }
    }
}
