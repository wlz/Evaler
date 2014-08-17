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
            Test.Run();
            Console.Read();
        }

        static int Value(string exp)
        {
            string type = Type(exp);

            switch (type)
            {
                case "int":
                    return int.Parse(exp);
                case "exp":
                    return Apply(car(exp), car(cdr(exp)), car(cdr(cdr(exp))));
                default:
                    return 0;
            }
        }

        static int Apply(string op, string op1, string op2)
        {
            switch (op)
            {
                case "+":
                    return Value(op1) + Value(op2);
                case "-":
                    return Value(op1) - Value(op2);
                default:
                    return 0;
            }
        }

        static string Type(string exp)
        {
            if (exp.StartsWith("("))
                return "exp";
            //else if (exp == "t" || exp == "f")
            //    return "bool";
            else if (exp == "+" || exp == "-")
                return "op";
            else
                return "int";
        }

        static string car(string exp)
        {
            if (exp == "()" || string.IsNullOrEmpty(exp))
                return "null";
            else if (exp[1] == '(')
                return FirstList(exp);
            else
                return FirstItem(exp);
        }

        private static string FirstItem(string exp)
        {
            string result = string.Empty;

            for (int i = 1; i < exp.Length; i++)
            {
                if (exp[i] == '(' || exp[i] == ')' || exp[i] == ' ')
                {
                    result = exp.Substring(1, i - 1);
                    break;
                }
            }

            return result;
        }

        private static string FirstList(string exp)
        {
            string result = string.Empty;
            int cnt = 1;
            for (int i = 2; i < exp.Length; i++)
            {
                if (exp[i] == '(')
                    cnt++;

                if (exp[i] == ')')
                    cnt--;


                if (cnt == 0)
                {
                    result = exp.Substring(1, i);
                    break;
                }
            }

            return result;
        }

        static string cdr(string exp)
        {
            return car(exp) == "null" ?
                "()" :
                "(" + exp.Substring(car(exp).Length + 1).TrimStart();
        }

        class Test
        {
            public static void TestCar()
            {
                string exp;
                exp = "()";
                //exp = "(aaa)";
                //exp = "(())";
                //exp = "(a b)";
                //exp = "((a) b)";
                //exp = "(a b c)";
                //exp = "((a) b c)";
                //exp = "(() b c)";
                //exp = "(()())";
                //exp = "(())";
                //Console.WriteLine(cdr(exp) + "|");
                Console.WriteLine(car(exp) + "|");
            }

            public static void TestValue()
            {
                string exp =
                    //"(+ 1 2)";
                    //"(+ (+ 1 2) 2)";
                    "(+ (+ 1 2) (+ 3 2))";
                Console.WriteLine(Program.Value(exp) + "|");
            }

            public static void Run()
            {
                TestValue();
            }
        }
    }
}