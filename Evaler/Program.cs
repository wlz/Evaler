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
                case "string":
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
            string val1 = Value(op1), val2 = Value(op2);

            switch (op)
            {
                case "+":
                case "-":
                    return ArithOp(op, val1, val2);
                case ">":
                case "<":
                    return CompOp(op, val1, val2);
                case "&":
                case "|":
                    return LogicOp(op, val1, val2);
                default:
                    return string.Empty;
            }
        }

        static string ArithOp(string op, string val1, string val2)
        {
            string result = string.Empty;

            if (Type(val1) == "num" && Type(val2) == "num")
            {
                if (op == "+")
                    return (int.Parse(val1) + int.Parse(val2)).ToString();
                else if (op == "-")
                    return (int.Parse(val1) - int.Parse(val2)).ToString();
            }
            else
            {
                if (op == "+")
                    return val1 + val2;
            }

            return string.Empty;
        }

        static string CompOp(string op, string val1, string val2)
        {
            if (Type(val1) == "num" && Type(val2) == "num")
            {
                int v1 = int.Parse(val1), v2 = int.Parse(val2);
                if (op == ">")
                    return (v1 > v2) ? "t" : "f";
                else if (op == "<")
                    return v1 < v2 ? "t" : "f";
            }

            return "f";
        }

        static string LogicOp(string op, string val1, string val2)
        {
            if (Type(val1) == "bool" && Type(val2) == "bool")
            {
                if (op == "&")
                    return (val1 == "t" && val2 == "t") ? "t" : "f";
                else if (op == "|")
                    return (val1 == "t" || val2 == "t") ? "t" : "f";
            }

            return "f";
        }

        static string Type(string exp)
        {
            int num = 0;

            if (exp.StartsWith("("))
                return "exp";
            else if (exp == "+" || exp == "-")
                return "arop";
            else if (exp == ">" || exp == "<" || exp == "&" || exp == "|")
                return "lgop";
            else if (exp == "t" || exp == "f")
                return "bool";
            else if (int.TryParse(exp, out  num))
                return "num";
            else
                return "string";
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
                //string exp = "(< 3 4)";
                string exp = "(> 3 4)";
                Console.WriteLine(Program.Value(exp) + "|");
            }

            public static void TestLogic()
            {
                //string exp = "(and (> 2 1) (< 3 4))";
                string exp = "(& (> (+ 1 2) 1) (< 3 4))";
                //string exp = "(& t t)";
                Console.WriteLine(Program.Value(exp) + "|");
            }

            public static void TestArith()
            {
                //string exp = "aaa";
                string exp = "(- aaa bbb)";
                Console.WriteLine(Program.Value(exp) + "|");
            }

            public static void TestString()
            {
                //string exp = "aaa";
                string exp = "(+ aaa bbb)";
                Console.WriteLine(Program.Value(exp) + "|");
            }

            public static void Run()
            {
                TestLogic();
                //TestArith();
                //TestBool();
            }
        }
    }
}