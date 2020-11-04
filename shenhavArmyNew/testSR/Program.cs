using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassesSolution;

namespace testServer2
{
    class Class1
    {
        public static void Main(string[] args)
        {
            MyStream sr = new MyStream(@"C:\Users\Shenhav\Desktop\shenhavArmyNew\tsetCCode\tsetCCode\test.c", System.Text.Encoding.UTF8);
            string s;
            s = sr.ReadLine();
            for (int i = 0; i < 5; i++)
            {
                s = sr.ReadLine();
            }
            uint pos = sr.Pos;
            Console.WriteLine(sr.curRow);
            Console.WriteLine(s);
            for (int i = 0; i < 5; i++)
            {
                s = sr.ReadLine();
            }
            Console.WriteLine(sr.curRow);
            Console.WriteLine(s);
            sr.Seek(pos);
            
            s = sr.ReadLine();
            Console.WriteLine(sr.curRow);
            Console.WriteLine(s);
            Console.ReadLine();
        }
    }
}
