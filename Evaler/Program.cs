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

        static string Value(string exp)
        {
            string type = Type(exp);

            switch (type)
            {
                case "num":
                    return exp;
                case "bool":
                    return exp;
                case "exp":
                    return Apply(car(exp), car(cdr(exp)), car(cdr(cdr(exp))));
                default:
                    return string.Empty;
            }
        }

        static string Apply(string op, string op1, string op2)
        {
            int val1 = 0, val2 = 0;
            if (op == "+" || op == "-" || op == ">" || op == "<")
            {
                val1 = int.Parse(Value(op1));
                val2 = int.Parse(Value(op2));
            }

            switch (op)
            {
                case "+":
                    return (val1 + val2).ToString();
                case "-":
                    return (val1 - val2).ToString();
                case ">":
                    return (val1 > val2) ? "true" : "false";
                case "<":
                    return (val1 < val2) ? "true" : "false";
                case "and":
                    return (Value(op1) == "true" && Value(op2) == "true") ? "true" : "false";
                case "or":
                    return (Value(op1) == "true" || Value(op2) == "true") ? "true" : "false";
                default:
                    return string.Empty;
            }
        }

        static string Type(string exp)
        {
            if (exp.StartsWith("("))
                return "exp";
            else if (exp == "+" || exp == "-")
                return "arop";
            else if (exp == "+" || exp == "-")
                return "lgop";
            else if (exp == "true" || exp == "false")
                return "bool";
            else
                return "num";
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
                    //"(+ (+ 1 2) (+ 3 2))";
                    "(+ (+ 1 2) (- 3 2))";
                Console.WriteLine(Program.Value(exp) + "|");
            }

            public static void TestBool()
            {
                //string exp = "true";
                //string exp = "false";
                string exp = "(< 3 4)";
                Console.WriteLine(Program.Value(exp) + "|");
            }

            public static void TestLogic()
            {
                //string exp = "(and (> 2 1) (< 3 4))";
                string exp = "(and (> (+ 1 2) 1) (< 3 4))";
                Console.WriteLine(Program.Value(exp) + "|");
            }

            public static void Run()
            {
                TestBool();
            }
        }
    }
}