using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaler
{
    class Program
    {
        static void Main(string[] args)
        {
            //(op e1 e2)
            //(+ 1 2)
            //Console.WriteLine(Value("(+ 1 2)"));
            //Console.WriteLine(car("(+ 1 2)") + "|");
            Console.WriteLine(car("()") + "|");
            Console.Read();
        }

        static int Value(string exp)
        {
            return 0;
        }

        static string car(string exp)
        {
            if (string.IsNullOrEmpty(exp) || exp == "()")
                return string.Empty;
            else
                return exp.Substring(1, exp.IndexOf(' ') - 1);
        }

        static string cdr(string exp)
        {
            return string.Empty;
        }

        //static string Left(string exp)
        //{
        //    return "";
        //}

        //static string Right(string exp)
        //{
        //    return "";
        //}

        //static string Op(string exp)
        //{
        //    return "+";
        //}
    }
}
