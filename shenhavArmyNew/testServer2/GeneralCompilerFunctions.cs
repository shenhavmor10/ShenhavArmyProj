
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections;
using System;
using System.Security.Policy;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.ComponentModel;
using System.Linq;
using ClassesSolution;

namespace testServer2
{
    public class GeneralCompilerFunctions
    {
        //All Patterns That is being searched in the code.
        static Regex OpenBlockPattern = new Regex(@".*{.*");
        static Regex CloseBlockPattern = new Regex(@".*}.*");
        static Regex functionPatternInH = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]\;$");
        static Regex staticFunctionPatternInC = new Regex(@"^.*static.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        static Regex FunctionPatternInC = new Regex(@"^([^ ]+\s)?[^ ]+\s(.*\s)?[^ ]+\([^()]*\)$");
        static Regex StructPattern = new Regex(@".*struct(\s.+{$|[^\s]+$|.*{.+;$)");
        static Regex TypedefOneLine = new Regex(@"^.*typedef(\sstruct)?\s.+\s.+;$");
        static Regex VariableDecleration = new Regex(@"^(?!.*return)(?=(\s)?[^\s()]+\s((\*)*(\s))?[^\s()=]+(\s?=.+;|[^()=]*;)$)");
        static Regex VariableEquation = new Regex(@"^(?!.*return)(?=(\s)?([^\s()]+\s)?((\*)*(\s))?[^\s()]+(\s)?=(\s)?[A-Za-z][^\s()]*;$)");
        static Regex DefineDecleration = new Regex(@"^(\s)?#define ([^ ]+) [^\d][^ ()]*( [^ ()]+)?$");
        //include <NAME>
        static Regex IncludeTrianglesPattern = new Regex(@"^(\s)?#include.{0,2}<.+>$");
        static Regex IncludeRegularPattern = new Regex(@"^(\s)?#include\s{0,2}"".+\""$");
        static Regex IncludePathPattern = new Regex(@"^(\s)?#include\s{0,2}"".+\""$");
        //chars to trim.
        static char[] CharsToTrim = { '&', '*', '\t', ' ', ';', '{', '}' };
        static bool CompileError = false;
        static ArrayList ignoreVarialbesType = new ArrayList();
        /// Function - CreateMd5
        /// <summary>
        /// Function gets a string as an input and turns it to an MD5.
        /// </summary>
        /// <param name="input"> this paramter is the string that is being changed to an MD5 format.</param>
        /// <returns>MD5 format type string.</returns>
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        /// Function - NextScopeLength
        /// <summary>
        ///  in order to find function length or struct length or "next scope" this function can be used.
        /// </summary>
        /// <param name="sr"> type MyStream buffer for the file. </param>
        /// <param name="s"> refference of the current code line type string. </param>
        /// <param name="count"> refference of the length of the scope type int. </param>
        /// <param name="Seek"> bool type parameter for returning the buffer to where it started from
        ///                     or to keep the buffer where it is after the scope ends. </param>
        /// <returns></returns>
        public static bool NextScopeLength(MyStream sr, ref string s, ref int count,bool Seek)
        {
            //stack to count the blocks.
            Stack myStack = new Stack();
            //saving the current position of the buffer.
            uint curPos = sr.Pos;
            string ScopeName = new string(s.ToCharArray());
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
            //checking the bool for seeking.
            if(Seek)
            {
                sr.Seek(curPos);
                s = ScopeName;
            }
            myStack.Clear();
            return found;
        }
        /// Function - CheckIfStringInHash
        /// <summary>
        /// function checks if in the given hashtable it can find a key 
        /// that is equal to the s parameter.
        /// </summary>
        /// <param name="a"> Hashtable </param>
        /// <param name="s"> string </param>
        /// <returns> returns if the string is in the hash type bool</returns>
        public static bool CheckIfStringInHash(Hashtable a, string s)
        {
            bool found = false;
            string result = s.Trim(CharsToTrim);
            if (a.ContainsKey(CreateMD5(result)))
            {
                found = true;
            }
            return found;
        }
        /// Function - skipDocumentation
        /// <summary>
        /// skips the documentation in c file in a buffer. 
        /// </summary>
        /// <param name="sr"> buffer type MyStream</param>
        /// <param name="s"> string </param>
        /// <returns> returns the amount of rows the documentation was.</returns>
        public static int skipDocumentation(MyStream sr,string s)
        {
            int count = 0;
            uint pos = sr.Pos;
            if(s.IndexOf("//")!=-1)
            {
                while((s.IndexOf("//")!=-1))
                {
                    pos = sr.Pos;
                    count++;
                    s = sr.ReadLine();
                }
                sr.Seek(pos);
                count--;
            }
            if(s.IndexOf("/*")!=-1)
            {
                while(!(s.IndexOf("*/") != -1))
                {
                    count++;
                    s = sr.ReadLine();
                }
            }
            return count;
        }
        /// Function - KeywordsAmountOnVariableDeclaration
        /// <summary>
        /// Checks how many words are in the variable declaration before the name of the variable.
        /// </summary>
        /// <param name="s"> variable declaration line type string.</param>
        /// <returns> returns type int word count.</returns>
        public static int KeywordsAmountOnVariableDeclaration(string s)
        {
            int count = 0;
            int pos = s.IndexOf(' ');
            bool endLoop = false;
            while (pos > 0 && !endLoop)
            {
                if (s.IndexOf('=') != -1)
                {
                    endLoop = true;
                }
                count++;
                if (s.IndexOf('*') != -1)
                    count = count - 1;

                pos = s.IndexOf(' ', pos + 1);
            }
            return count;
        }
        //the checks that are being mad in syntax Check are being written here.
        /// Function - IsExistInArrayList
        /// <summary>
        /// Checks if the string can be found in the ArrayList.
        /// </summary>
        /// <param name="a"> ArrayList </param>
        /// <param name="name"> the string that is being checked.</param>
        /// <returns> returns the ArrayList Node.</returns>
        public static ParametersType IsExistInArrayList(ArrayList a, string name)
        {
            ParametersType result = null;
            bool found = false;
            for (int i = 0; i < a.Count&&!found; i++)
            {
                if (((ParametersType)a[i]).parameterName == name)
                {
                    result = (ParametersType)a[i];
                    found = true;
                }
            }
            return result;
        }
        /// Function getParameterNameFromLine
        /// <summary>
        /// Gets a code line of a variable Declaration and gives back the node of the variable (name,type).
        /// </summary>
        /// <param name="line"> Code line type string.</param>
        /// <returns> returns type - ParametersType the variable that was being declared.</returns>
        public static ParametersType getParameterNameFromLine(string line)
        {
            string name;
            string type=line;
            int lastIndex;
            name = line.Substring(line.LastIndexOf(" ") + 1, (line.Length - (line.LastIndexOf(" ") + 1)));
            if (name == "")
            {
                name = line.Substring(line.LastIndexOf("*"), (line.Length - (line.LastIndexOf("*") + 1)));
            }
            lastIndex = line.LastIndexOf(" ");
            if (lastIndex != -1)
            {
                type = line.Remove(lastIndex).Trim();
            }
            foreach (string var in ignoreVarialbesType)
            {
                if (type.IndexOf(var) != -1)
                {
                    type = type.Replace(var, "");
                    type = type.Trim();
                }
            }
            ParametersType result = new ParametersType(name, type);
            return result;
        }
        /// Function - VariableDeclarationHandler
        /// <summary>
        /// Handling the variable declaration part in the function "ChecksInSyntaxCheck" by
        /// checking the whole variable declaration syntax.. (Checks the keywords and if the variable is being equal so checks
        /// the types of both of variables are equal and adding the new variable to "blocksAndNames" ArrayList).
        /// </summary>
        /// <param name="s"> refference of the string s (code line).</param>
        /// <param name="pos"></param>
        /// <param name="keywords"> Hashtable type that stores all keywords in the code.</param>
        /// <param name="blocksAndNames"> ArrayList Type that stores all variables in the scopes.</param>
        /// <param name="IsScope"> Variable that checks if the function is being called inside a scope or 
        ///                        outside a scope.</param>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <returns></returns>
        public static bool VariableDeclarationHandler(ref string s,ref int pos,Hashtable keywords,int threadNumber, ArrayList blocksAndNames,bool IsScope,MyStream sr)
        {
            bool DifferentTypes = true;
            int loopCount;
            string temp = "";
            int j;
            bool found = true;
            char[] trimChars = { '\t', ' ',';' };
            bool isExist = false;
            
            loopCount = KeywordsAmountOnVariableDeclaration(s);
            for (j = 0; j < loopCount; j++)
            {
                //checks if the keywords in the declaration is exist.
                found = found && CheckIfStringInHash(keywords, s.Substring(pos, s.Substring(pos, s.Length - pos).IndexOf(' ')).Trim(CharsToTrim));
                pos = s.IndexOf(' ', pos + 1) + 1;
            }
            if (loopCount == 0)
            {
                //gets in the if section only if there is only 1 keyword.
                found = found && CheckIfStringInHash(keywords, s.Substring(pos, s.Substring(pos, s.Length - pos).IndexOf(' ')).Trim(CharsToTrim));
            }
            if (s.IndexOf("struct") != -1)
            {
                //gets in if the variable type includes a struct without a typedef.
                pos = s.IndexOf("struct");
                temp = s.Substring(pos, s.IndexOf(" ", pos + 7) - pos);
                found = CheckIfStringInHash(keywords, temp.Trim(CharsToTrim));
            }
            string name;
            int lastIndex;
            ParametersType result;
            //if the line has equation in the declaration.
            if (s.IndexOf("=") != -1)
            {
                temp = Regex.Split(s, "=")[0];
                temp = temp.Trim(trimChars);
                result = getParameterNameFromLine(temp);
            }
            //only declaration.
            else
            {
                temp = s;
                temp = temp.Trim(trimChars);
                result = getParameterNameFromLine(temp);
            }
            name = result.parameterName;
            temp = result.parameterType;
            temp = temp.Replace(" ", "");
            // checks if there is already the same name in the same scope.
            if (IsExistInArrayList(((ArrayList)blocksAndNames[blocksAndNames.Count - 1]), name) != null)
            {
                isExist = true;
                Server.Program.CloseConnection(threadNumber, ("you have used the same name for multiple variables in row " + sr.curRow + ". name - " + name));
                CompileError = true;
            }
            else
            {
                ((ArrayList)blocksAndNames[blocksAndNames.Count - 1]).Add(new ParametersType(name, temp));
            }
            //if the declaration is also a equation.
            if(VariableEquation.IsMatch(s))
            {
                DifferentTypes=VariableEquationHandler(sr, s, blocksAndNames,threadNumber);
            }
            if(!DifferentTypes)
            {
                Server.Program.CloseConnection(threadNumber, s + " types of both variables are different in row : " + sr.curRow);
                CompileError = true;
            }
            
            return found;
        }
        /// Function - getVariableTypeParameterFromArrayList
        /// <summary>
        /// Get the whole parameterType node out of the ArrayList.
        /// </summary>
        /// <param name="blocksAndNames"> ArrayList type.</param>
        /// <param name="name"> the name to get the whole node from.</param>
        /// <returns> returns parameterType type of the node named like "name".</returns>
        public static ParametersType getVariableTypeParameterFromArrayList(ArrayList blocksAndNames,string name)
        {
            bool endLoop = false;
            ParametersType result =null;
            for(int i=blocksAndNames.Count;i>0&&!endLoop;i--)
            {
                if ((result=IsExistInArrayList((ArrayList)blocksAndNames[i-1], name))!=null)
                {
                    endLoop = true;
                }
            }
            return result;
        }
        /// Function - VariableEquationHandler
        /// <summary>
        /// Handling the variable equation part in the function "ChecksInSyntaxCheck" by
        /// make sure every variable is exist in the code and that their type of the equation
        /// is the same.
        /// </summary>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="s"> the code line type string.</param>
        /// <param name="blocksAndNames"> ArrayList of variables.</param>
        /// <returns>returns if the variable equation is good.</returns>
        public static bool VariableEquationHandler(MyStream sr,string s, ArrayList blocksAndNames,int threadNumber)
        {
            char[] trimChars = { '\t', ' '};
            bool found = true;
            //splits the equation to 2 lines before the '=' and after it.
            string temp = Regex.Split(s, "=")[0].Trim(trimChars);
            //takes the first param name.
            ParametersType result =getParameterNameFromLine(temp);
            string varName1=result.parameterName;
            temp = Regex.Split(s, "=")[1];
            char[] searchingChars = { ';'};
            //takes the second param name.
            string varName2 = temp.Substring(0, temp.IndexOfAny(searchingChars));
            varName2 = varName2.Trim(trimChars);
            //takes the whole parameterType type by the function - "getVariableTypeParameterFromArrayList".
            ParametersType var1 = getVariableTypeParameterFromArrayList(blocksAndNames, varName1.Trim('*'));
            ParametersType var2 = getVariableTypeParameterFromArrayList(blocksAndNames, varName2.Trim('*'));
            //make sures the variable 2 is exist.
            if(var2==null)
            {
                Server.Program.CloseConnection(threadNumber,"There is no parameter named " + varName2 + " in row : " + sr.curRow);
                CompileError = true;
                found = false;
            }
            //checks if their type is the same.
            if(found&&var1.parameterType!=var2.parameterType)
            {
                found = false;
            }
            return found;
        }
        /// Function - ChecksInSyntaxCheck
        /// <summary>
        /// this function take cares of the whole syntax check of the program.
        /// it uses the functions mentioned in the documentations before.
        /// it take cares scopes and no scopes with the parameter IsScope type bool.
        /// </summary>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="s"> code line type string.</param>
        /// <param name="IsScope"> bool type IsScope.</param>
        /// <param name="keywords"> keywords type Hashtable that conatins the code keywords.</param>
        /// <param name="blocksAndNames"> blocksAndNames type ArrayList that conatins the code variables in the scope.</param>
        /// <param name="parameters"> parameters type ArrayList conatins the function parameters.</param>
        /// <param name="functionLength"> scopeLength type int default is 0 if the code line is outside any scopes.</param>
        public static void ChecksInSyntaxCheck(MyStream sr,string s, bool IsScope, Hashtable keywords,int threadNumber,ArrayList blocksAndNames,ArrayList parameters=null, int functionLength = 0)
        {
            //adds the parameters of the function to the current ArrayList of variables.
            if(parameters!=null)
            {
                blocksAndNames.Add(new ArrayList());
                ((ArrayList)blocksAndNames[1]).AddRange(parameters);
            }
            if(StructPattern.IsMatch(s))
            {
                //Add struct keywords to the keywords Hashtable.
                AddStructNames(sr, s, keywords);
            }
            if (s.Trim('\t').IndexOf("{")!=-1)
            {
                s = sr.ReadLine();
            }
            //how to convert to array list
            bool keywordCheck = true;
            bool DifferentTypesCheck = true;
            bool equationTypeCheck = true;
            int pos = 0;
            int i;
            ArrayList keywordResults = new ArrayList();
            for (i = 0; i < functionLength+1; i++)
            {
                if(s.Trim('\t')=="")
                {
                    s = sr.ReadLine();
                }
                //take cares to all of those situations.
                if (StructPattern.IsMatch(s) || TypedefOneLine.IsMatch(s))
                {
                    keywordResults = AddStructNames(sr, s, keywords);
                }
                if (VariableDecleration.IsMatch(s) && !(s.IndexOf("typedef") != -1))
                {
                    keywordCheck = VariableDeclarationHandler(ref s, ref pos, keywords,threadNumber, blocksAndNames, IsScope, sr);
                }
                else if(VariableEquation.IsMatch(s))
                {
                    DifferentTypesCheck = VariableEquationHandler(sr,s, blocksAndNames,threadNumber);
                }
                s = s.Trim();
                //checks if any of the error bools is on.
                if (!keywordCheck)
                {
                    string error=(s + " keyword does not exist. row : "+sr.curRow);
                    Console.WriteLine(sr.curRow);
                    Console.WriteLine(error);
                    Server.Program.CloseConnection(threadNumber, error);
                    CompileError = true;

                }
                if(!DifferentTypesCheck)
                {
                    string error=(s + " types of both variables are different in row : "+sr.curRow);
                    Server.Program.CloseConnection(threadNumber, error);
                    CompileError = true;
                }
                pos = 0;
                //resets the error bools.
                keywordCheck = DifferentTypesCheck = true; 
                if (s.IndexOf("//")!=-1||s.IndexOf("/*")!=-1)
                {
                    //skips documentation if needed.
                    i+=skipDocumentation(sr, s);
                }
                //adds a new ArrayList inside the keywordsAndNames ArrayList for the scope that has started.
                if(OpenBlockPattern.IsMatch(s))
                {
                    blocksAndNames.Add(new ArrayList());
                }
                if(CloseBlockPattern.IsMatch(s))
                {
                    try
                    {
                        //close the last scope that just closed.
                        blocksAndNames.RemoveAt(blocksAndNames.Count - 1);

                    }
                    catch(Exception e)
                    {
                        //bad scoping causes the function to remove from an ArrayList something while its already 0.
                        Server.Program.CloseConnection(threadNumber,"bad scoping in function in row "+ sr.curRow);
                        CompileError = true;
                    }
                }
                //if the code line is in a scope or if its not the last line in the scope continute to the next line.
                if (IsScope&&i!=functionLength)
                {
                    s = sr.ReadLine();
                }
            }
            //if that was a scope it removes all the keywords of the scope.
            if (IsScope)
            {
                for (i = 0; i < keywordResults.Count; i++)
                {
                    keywords.Remove(keywordResults[i]);
                }
            }
        }
        /// Function - SyntaxCheck
        /// <summary>
        /// that function uses the Function "ChecksInSyntaxCheck" if that is in a scope
        /// or outside a scope according to the situation.
        /// </summary>
        /// <param name="path"> The path of the c code type string.</param>
        /// <param name="keywords"> keywords type Hashtable that conatins the code keywords.</param>
        public static bool SyntaxCheck(string path, Hashtable keywords,int threadNumber)
        {
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            //in order to delete struct keywords when they come in a function at the end of the function.
            ArrayList parameters=new ArrayList();
            ArrayList blocksAndNames = new ArrayList();
            //adds an ArrayList inside blocksAndNames ArrayList for the action outside the scopes.
            blocksAndNames.Add(new ArrayList());
            string s;
            int scopeLength = 0;
            while ((s = sr.ReadLine()) != null&&!CompileError)
            {
                scopeLength = 0;
                //handling the scopes.
                if(OpenBlockPattern.IsMatch(s))
                {
                    NextScopeLength(sr, ref s, ref scopeLength, true);
                    ChecksInSyntaxCheck(sr, s, true, keywords,threadNumber, blocksAndNames, parameters, scopeLength+1);
                    parameters.Clear();
                }
                // if there is a function it saves its parameters.
                else if(FunctionPatternInC.IsMatch(s))
                {
                    parameters.AddRange(GeneralRestApiServerMethods.FindParameters(s));
                }
                //handling outside the scopes.
                else
                {
                    ChecksInSyntaxCheck(sr, s, false, keywords,threadNumber, blocksAndNames); 
                }

            }
            return CompileError;

        }
        /// Function - AddToHashFromFile
        /// <summary>
        /// Adds from a file splited by "splitBy" parameter to the Hashtable fromt he path.
        /// </summary>
        /// <param name="path"> The path for the file.</param>
        /// <param name="a"> Hashtable to store the keywords.</param>
        /// <param name="splitBy"> String that the file needs to split by.</param>
        public static void AddToHashFromFile(string path, Hashtable a, string splitBy)
        {
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            string temp = sr.ReadLine();
            string[] tempArr = Regex.Split(temp, splitBy);
            ICollection keys = a.Keys;
            for (int i = 0; i < tempArr.Length; i++)
            {
                a.Add(CreateMD5(tempArr[i]), tempArr[i]);
            }
            sr.Close();
        }
        //path to the code;
        /// Function - PreprocessorActions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"> The path for the C code.</param>
        /// <param name="threadNumber"> thread number is a parameter to make sure when the main file is in.
        ///                             number 0 means its the main file any other number means it  is currently
        ///                             reads a file that is not the main. (Import).</param>
        /// <param name="keywords"> Hashtable to store the keywords.</param>
        /// <param name="includes"> Hashtable to store the includes.</param>
        /// <param name="defines"> Dictionary to store the defines . (key - new keyword, value - old Definition)</param>
        /// <param name="pathes"> Paths for all the places where the imports might be.</param>
        public static void PreprocessorActions(string path, int threadNumber, Hashtable keywords, Hashtable includes,Dictionary<string,string> defines,string [] pathes)
        {
            bool endLoop = false;
            MyStream sr = null;
            //try to open the buffer.
            try
            {
                sr = new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Second exception caught.", e);
                endLoop = true;
            }

            string s;
            string firstNewVariableWord;
            string secondNewVariableWord;
            string[] newkeyWords;
            string newKeyword;
            string defineOriginalWord;
            while (!endLoop && (s = sr.ReadLine()) != null)
            {

                if (DefineDecleration.IsMatch(s))
                {
                    //getting both of the names in two variables.
                    firstNewVariableWord = s.Substring(s.IndexOf(' '), s.Length - s.IndexOf(' '));
                    firstNewVariableWord = firstNewVariableWord.Trim();
                    firstNewVariableWord = firstNewVariableWord.Substring(firstNewVariableWord.IndexOf(' '), firstNewVariableWord.Length - firstNewVariableWord.IndexOf(' '));
                    firstNewVariableWord = firstNewVariableWord.Trim();
                    firstNewVariableWord = firstNewVariableWord.Trim(CharsToTrim);
                    //old definition.
                    defineOriginalWord = firstNewVariableWord;
                    
                    if (firstNewVariableWord.IndexOf(" ") != -1)
                    {
                        //checks if the definition exists.
                        if(keywords.ContainsKey(CreateMD5(firstNewVariableWord)))
                        {
                            //new keyword
                            newKeyword = Regex.Split(s, " ")[1];
                            newKeyword = newKeyword.Trim();
                            //make sure that the keyword isn't already existed.
                            if (!keywords.ContainsKey(CreateMD5(newKeyword)))
                            {
                                //adds her if not
                                keywords.Add(CreateMD5(newKeyword), newKeyword);
                                //adds the new definition.
                                defines.Add(newKeyword, defineOriginalWord);
                                //types the dont mind the variable are being ingored. for an example static : so if
                                //the type for example is static and i define a new static definition it will add it to
                                //the ignoreVariablesType.
                                if (ignoreVarialbesType.Contains(defineOriginalWord))
                                {
                                    ignoreVarialbesType.Add(newKeyword);
                                }
                            }
                        }
                        else
                        {
                            //splits when there are 2 types that you define for an example "unsinged int"
                            //so what this section is doing is checking if both of the types exist.
                            newkeyWords = Regex.Split(firstNewVariableWord, " ");
                            secondNewVariableWord = newkeyWords[1];
                            firstNewVariableWord = newkeyWords[0];
                            //checks both types.
                            if (CheckIfStringInHash(keywords, firstNewVariableWord) && CheckIfStringInHash(keywords, secondNewVariableWord))
                            {
                                newKeyword = Regex.Split(s, " ")[1];
                                newKeyword = newKeyword.Trim();
                                //creates the keywords if they dont exist.
                                if (!keywords.ContainsKey(CreateMD5(newKeyword)))
                                {
                                    keywords.Add(CreateMD5(newKeyword), newKeyword);
                                    defines.Add(newKeyword, defineOriginalWord);
                                    Console.WriteLine("new Keywords :" + newkeyWords[0]);
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        //if there is only one type in the old definition.
                        if (CheckIfStringInHash(keywords, firstNewVariableWord))
                        {
                            newKeyword = Regex.Split(s, " ")[1];
                            newKeyword = newKeyword.Trim();
                            if (!keywords.ContainsKey(CreateMD5(newKeyword)))
                            {
                                keywords.Add(CreateMD5(newKeyword), newKeyword);
                                defines.Add(newKeyword, defineOriginalWord);
                                Console.WriteLine("new : "+newKeyword);
                            }
                        }
                    }

                }
                //Handling almost the same patterns as the syntaxCheck function.
                if (StructPattern.IsMatch(s) && threadNumber != 0)
                {
                    AddStructNames(sr, s, keywords);
                }
                else if (TypedefOneLine.IsMatch(s) && threadNumber != 0)
                {
                    AddStructNames(sr, s, keywords);
                }
                //if the code line is an include it creates a thread and enters to the defines , structs and more to 
                //the Hashtables and Dictionaries.
                else if (IncludeTrianglesPattern.IsMatch(s) || IncludeRegularPattern.IsMatch(s))
                {
                    string currentPath="";
                    string result;
                    if (s.IndexOf("<") != -1 && s.IndexOf(">") != -1)
                    {
                        result = CutBetween2Strings(s, "<", ">");
                    }
                    else
                    {
                        result = CutBetween2Strings(s, "\"", "\"");
                    }
                    Console.WriteLine(result);
                    //only enters an include if it didnt already included him.
                    if (!includes.Contains(CreateMD5(result)))
                    {
                        includes.Add(CreateMD5(result), result);
                        Thread thread;
                        //if the include includes a path inside of it.
                        if (result.IndexOf("\\") != -1)
                        {
                            //opens the thread (thread number +1).
                            thread = new Thread(() => PreprocessorActions(result, threadNumber + 1, keywords, includes,defines, pathes));
                        }
                        //if it does not include an exact path.
                        else
                        {
                            //runs on the pathes that the import files might be in.
                            for(int i=0;i<pathes.Length;i++)
                            {
                                //checks if the file exists in one of those folders.
                                if(File.Exists(pathes[i]+"\\"+result))
                                {
                                    currentPath = pathes[i];
                                    break;                                
                                }
                            }    
                            //creats a thread.
                            thread = new Thread(() => PreprocessorActions(currentPath+"\\" + result, threadNumber + 1, keywords, includes,defines, pathes));
                        }
                        thread.Start();
                        thread.Join();
                        Console.WriteLine("thread " + threadNumber + "stopped");
                    }
                }

            }
            if (sr != null)
            {
                sr.Close();
            }
        }
        /// Function - CutBetween2Strings
        /// <summary>
        /// Cut a string s from the first parameter to the second parameter.
        /// </summary>
        /// <param name="s"> string s that the function cuts.</param>
        /// <param name="first"> first cut.</param>
        /// <param name="second"> second cut.</param>
        /// <returns> returns the cutted string.</returns>
        public static string CutBetween2Strings(string s, string first, string second)
        {
            int pFrom = s.IndexOf(first) + first.Length;
            int pTo = s.LastIndexOf(second);
            string result = s.Substring(pFrom, pTo - pFrom);
            result = result.Trim();
            return result;
        }
        /// Function - AddStructNames
        /// <summary>
        /// adds all the names of the struct or typedef struct to the keywords Hashtable.
        /// </summary>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="s"> struct first line</param>
        /// <param name="keywords"> Hashtable to store the keywords.</param>
        /// <returns> returns an ArrayList with the keywords.</returns>
        public static ArrayList AddStructNames(MyStream sr, string s, Hashtable keywords)
        {
            ArrayList results = new ArrayList();
            int count = 0;
            int temp;
            string[] tempSplit;
            string tempString;
            string tempNewVariableName;
            //if thats a typedef.
            if (s.IndexOf("typedef") != -1)
            {
                //if thats not a typedef declaration.
                if (!TypedefOneLine.IsMatch(s))
                {
                    if (NextScopeLength(sr, ref s, ref count,false))
                    {
                        s = s.Trim(CharsToTrim);

                        tempSplit = Regex.Split(s, @",");
                        for (int i = 0; i < tempSplit.Length; i++)
                        {
                            tempSplit[i] = tempSplit[i].Trim(CharsToTrim);
                            if (!keywords.ContainsKey(CreateMD5(tempSplit[i])))
                            {
                                //adds the keywords.
                                keywords.Add(CreateMD5(tempSplit[i]), tempSplit[i]);
                                results.Add(CreateMD5(tempSplit[i]));
                            }

                        }
                    }
                }
                else
                {
                    //if thats one line of typedef.
                    temp = s.IndexOf(" ") + 1;
                    tempString = tempNewVariableName = s.Substring(temp);
                    tempString = tempString.TrimEnd(' ').Remove(tempString.LastIndexOf(' ') + 1);
                    tempString = tempString.Trim(CharsToTrim);
                    tempNewVariableName = CutBetween2Strings(s, tempString, ";");
                    tempNewVariableName = tempNewVariableName.Trim(CharsToTrim);
                    if (keywords.Contains(CreateMD5(tempString)) && !keywords.Contains(CreateMD5(tempNewVariableName)))
                    {
                        //adds the keyword for the line.
                        keywords.Add(CreateMD5(tempNewVariableName), tempNewVariableName);
                        results.Add(CreateMD5(tempNewVariableName));
                    }
                }
            }
            //if thats a regular struct.
            else
            {
                s = s.Trim(CharsToTrim);
                if (!keywords.Contains(CreateMD5(s)))
                {
                    //adds the new keyword.
                    keywords.Add(CreateMD5(s), s);
                    results.Add(CreateMD5(s));
                }


            }
            //returns the ArrayList.
            return results;


        }
        /// Function - printArrayList
        /// <summary>
        /// prints an arrayList.
        /// </summary>
        /// <param name="a"> Hashtable A to print</param>
        public static void printArrayList(Hashtable a)
        {
            ICollection keys = a.Keys;
            foreach (string key in keys)
            {
                Console.WriteLine(a[key]);
            }
        }
        /// Function - AddToArrayListFromFile
        /// <summary>
        /// Adds from a file the names that split by "split by" in the path "path"
        /// </summary>
        /// <param name="path"> path of the c code type string.</param>
        /// <param name="a"> ArrayList </param>
        /// <param name="splitBy"> What to split by type string.</param>
        public static void AddToArrayListFromFile(string path,ArrayList a,string splitBy)
        {
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            string s = sr.ReadLine();
            string [] temp = Regex.Split(s, splitBy);
            foreach (string i in temp)
            {
                a.Add(i);
            }
            sr.Close();
        }
        /// Function - initializeKeywordsAndSyntext
        /// <summary>
        /// Function initialize all of the functions that needs to come before the syntax Check.
        /// </summary>
        /// <param name="ansiPath">  Path for the file the conatins the ansi c keywords.</param>
        /// <param name="cFilePath"> Path for the c file that contains the file.</param>
        /// <param name="CSyntextPath"> Path of all the ansi c syntext file (not sure yet if it is needed.)</param>
        /// <param name="ignoreVariablesTypesPath"> Path for all the variables types that i ignore in the checking.</param>
        /// <param name="keywords"> Hashtable to store the keywords.</param>
        /// <param name="includes"> Hashtable to store the includes.</param>
        /// <param name="defines"> Dictionary to store the defines.</param>
        /// <param name="pathes"> Paths for all the places where the imports might be.</param>
        public static void initializeKeywordsAndSyntext(string ansiPath, string cFilePath, string CSyntextPath, string ignoreVariablesTypesPath, Hashtable keywords, Hashtable includes,Dictionary<string,string> defines,string [] pathes)
        {
            //ansiC File To Keywords ArrayList.
            AddToHashFromFile(ansiPath, keywords, ",");
            AddToArrayListFromFile(ignoreVariablesTypesPath, ignoreVarialbesType, ",");
            //C Syntext File To Syntext ArrayList.
            //AddToListFromFile(CSyntextPath, syntext, " ");
            PreprocessorActions(cFilePath, 0, keywords, includes,defines,pathes);
        }
    }
}





