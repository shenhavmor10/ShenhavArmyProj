using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
namespace Platformaa
{
    public class Program
    {
        static Regex OpenBlockPattern = new Regex(@"{");
        static Regex CloseBlockPattern = new Regex(@"}");
        static Regex functionPatternInH = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]\;$");
        static Regex staticFunctionPatternInC = new Regex(@"^.*static.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        static Regex FunctionPatternInC = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        static Regex StructPattern = new Regex(@".*struct(\s.+{$|[^\s]+$|.*{.+;$)");
        static Regex TypedefOneLine = new Regex(@"^typedef\sstruct\s.+\s.+;$");
        static Regex VariableDecleration = new Regex(@"[A-Za-z/d]+\s[A-Za-z/d]+(=[^()].+|[^=][^()]+);$");

        static string filePath = @"C:\Users\Shenhav\Desktop\shenhavArmyNew\tsetCCode\tsetCCode\test.c";
        static string ansiCFile = @"C:\Users\Shenhav\Desktop\shenhavArmyNew\Ansikeywords.txt";
        static string CSyntextFile = @"C:\Users\Shenhav\Desktop\shenhavArmyNew\CSyntext.txt";
        static ArrayList keywords = new ArrayList();
        static ArrayList syntext = new ArrayList();
        static void Main(string[] args)
        {
            Console.ReadLine();
        }
    }
        
}


