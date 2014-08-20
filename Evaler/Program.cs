using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaler
{
    class Program
    {
        static List<string> Operators = new List<string>() { "+", "-", ">", "<", "=", "and", "or", "not", "car", "cdr", "cons", "quote" };
        static List<string> Structs = new List<string>() { "if", "cond" };

        static void Main(string[] args)
        {
            Eval();
        }

        static void Eval()
        {
            while (true)
            {
                Console.Write(">");
                string exp = Console.ReadLine();

                Dictionary<string, string> env = new Dictionary<string, string>();

                env.Add("a", "111");
                env.Add("b", "111");
                env.Add("c", "111");
                env.Add("d", "111");

                Console.WriteLine(Value(exp, env));
                if (exp == "(exit)")
                    break;
            }
        }

        static string Value(string exp, Dictionary<string, string> env)
        {
            switch (Type(exp))
            {
                case "bool":
                case "num":
                    return exp;
                case "atom":
                    return env.ContainsKey(exp) ? env[exp] : "error";
                case "exp":
                    return CalcExp(car(exp), cdr(exp), env);
                case "struct":
                    return CalcStruct(exp, env);
                case "call":
                    return CalcCall(exp, env);
                case "lambda":
                    return CalcLambda(exp, env);
                default:
                    return "error";
            }
        }

        private static string CalcCall(string exp, Dictionary<string, string> env)
        {
            string func = car(exp);
            string args = cdr(exp);

            LoadArgs(second(func), args, env);

            return Value(third(func), env);
        }

        private static void LoadArgs(string pars, string vals, Dictionary<string, string> env)
        {
            if (pars != "()")
            {
                env.Add(car(pars), Value(car(vals), env));
                LoadArgs(cdr(pars), cdr(vals), env);
            }
        }

        static string CalcLambda(string exp, Dictionary<string, string> env)
        {
            return Value(third(exp), env);
        }

        static string CalcStruct(string exp, Dictionary<string, string> env)
        {
            switch (car(exp))
            {
                case "if":
                    return EvalIf(exp, env);
                case "cond":
                    return EvalCond(exp, env);
                default:
                    return "error";
            }
        }

        static string CalcExp(string op, string opd, Dictionary<string, string> env)
        {
            switch (op)
            {
                case "+":
                case "-":
                    return ArithOp(op, Value(car(opd), env), Value(second(opd), env));
                case ">":
                case "<":
                case "=":
                    return CompOp(op, Value(car(opd), env), Value(second(opd), env));
                case "and":
                case "or":
                case "not":
                    return LogicOp(op, opd, env);
                case "quote":
                    return car(opd);
                default:
                    return "error";
            }
        }

        static string second(string exp)
        {
            return car(cdr(exp));
        }

        static string third(string exp)
        {
            return car(cdr(cdr(exp)));
        }

        //private static string EvalLambda(string exp)
        //{
        //    Dictionary<string, string> env = new Dictionary<string, string>();
        //    LoadArgs(car(cdr(car(exp))), cdr(exp), env);

        //    string calExp = car(cdr(cdr(car(exp))));
        //    foreach (string arg in env.Keys)
        //        calExp = calExp.Replace(arg, env[arg]);

        //    return Value(calExp);
        //}



        private static string EvalCond(string exp, Dictionary<string, string> env)
        {
            return EvalCondsRecursive(cdr(exp), env);
        }

        private static string EvalCondsRecursive(string conds, Dictionary<string, string> env)
        {
            if (conds == "()" || string.IsNullOrEmpty(conds))
                return string.Empty;
            else
            {
                string exp = car(conds);

                if (car(exp) == "else")
                    return Value(car(cdr(exp)), env);
                else
                    return Value(car(exp), env) == "#t" ? Value(car(cdr(exp)), env) : EvalCondsRecursive(cdr(conds), env);
            }
        }

        private static string EvalIf(string exp, Dictionary<string, string> env)
        {
            string cond = car(cdr(exp));

            return Value(cond, env) == "#t" ?
                   Value(car(cdr(cdr(exp))), env) :
                   Value(car(cdr(cdr(cdr(exp)))), env);
        }

        static string cons(string val1, string val2)
        {
            return string.Empty;
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
                    return (v1 > v2) ? "#t" : "#f";
                else if (op == "<")
                    return v1 < v2 ? "#t" : "#f";
                else if (op == "=")
                    return v1 == v2 ? "#t" : "#f";
            }

            return "#f";
        }

        static string LogicOp(string op, string opd, Dictionary<string, string> env)
        {
            if (op == "not")
                return Value(car(opd), env) == "#t" ? "#f" : "#t";
            else if (op == "and" || op == "or")
            {
                string val1 = Value(car(opd), env), val2 = Value(second(opd), env);
                switch (op)
                {
                    case "and":
                        return val1 == "#t" && val2 == "#t" ? "#t" : "#f";
                    case "or":
                        return val1 == "#t" || val2 == "#t" ? "#t" : "#f";
                }
            }

            return "#f";
        }

        static string Type(string exp)
        {
            int num = 0;

            if (exp == "#t" || exp == "#f")
                return "bool";
            else if (int.TryParse(exp, out num))
                return "num";
            else if (!exp.StartsWith("("))
                return "atom";
            else if (exp.StartsWith("(") && Operators.Contains(car(exp)))
                return "exp";
            else if (exp.StartsWith("(") && Structs.Contains(car(exp)))
                return "struct";
            else if (exp.StartsWith("(") && car(exp) == "lambda")
                return "lambda";
            else if (exp.StartsWith("((lambda"))
                return "call";
            else
                return "unknown";
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
            //public static void TestCar()
            //{
            //    string exp;
            //    exp = "()";
            //    //exp = "(aaa)";
            //    //exp = "(())";
            //    //exp = "(a b)";
            //    //exp = "((a) b)";
            //    //exp = "(a b c)";
            //    //exp = "((a) b c)";
            //    //exp = "(() b c)";
            //    //exp = "(()())";
            //    //exp = "(())";
            //    //Console.WriteLine(cdr(exp) + "|");
            //    Console.WriteLine(car(exp) + "|");
            //}

            //public static void TestValue()
            //{
            //    string exp =
            //        //"(+ 1 2)";
            //        //"(+ (+ 1 2) 2)";
            //        //"(+ (+ 1 2) (+ 3 2))";
            //    "(+ (+ 1 2) (- 3 2))";
            //    Console.WriteLine(Value(exp) + "|");
            //}

            //public static void TestBool()
            //{
            //    //string exp = "true";
            //    //string exp = "false";
            //    //string exp = "(< 3 4)";
            //    string exp = "(> 3 4)";
            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestLogic()
            //{
            //    //string exp = "(and (> 2 1) (< 3 4))";
            //    //string exp = "(& (> (+ 1 2) 1) (< 3 4))";
            //    //string exp = "(! (> 3 2))";
            //    string exp = "(! (> 3 4))";
            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestArith()
            //{
            //    //string exp = "aaa";
            //    string exp = "(- aaa bbb)";
            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestString()
            //{
            //    //string exp = "aaa";
            //    string exp = "(+ aaa bbb)";
            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestListOp()
            //{
            //    //string exp = "(car a b c)";
            //    string exp = "(cdr (cdr (a b c)))";
            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestList()
            //{
            //    string exp = "(a b c)";
            //    //string exp = "(car (cdr (a b c)))";
            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestCons()
            //{
            //    //string exp = "(cons a (b))";
            //    //string exp = "(cons a (car (a b)))";
            //    string exp = "(cons 1 (b))";

            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestIf()
            //{
            //    //string exp = "(if (> 5 2) (+ 2 3) (- 3 1))";
            //    string exp = "(if (< 1 2) (if (< 3 4) 3 4) b)";

            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestCond()
            //{
            //    //string exp = "(if (> 5 2) (+ 2 3) (- 3 1))";
            //    //string exp = "(cond ((> 1 2) a) ((> 5 3) b))";
            //    string exp = "(cond ((< 3 2) a) (else (+ 1 2)))";

            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestLambda()
            //{
            //    //((lambda a b) (+ a b))
            //    //string exp = "(((lambda a b) (+ a b)) 1 2)";
            //    //string exp = "((lambda (a) (+ a a)) 3)";
            //    //string exp = "((lambda (a b) (+ a b)) 3 4)";
            //    //string exp = "((lambda (a b c) (- (+ a b) c)) 3 4 5)";
            //    string exp = "((lambda (a) a) 3)";


            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            //public static void TestQuote()
            //{
            //    //string exp = "(car (quote (a b)))";
            //    string exp = "(cdr (quote (a b)))";

            //    Console.WriteLine(Program.Value(exp) + "|");
            //}

            public static void Run()
            {
                //TestValue();
                //TestListOp();
                //TestLogic();
                //TestArith();
                //TestBool();
                //TestList();
                //TestCons();
                //TestIf();
                //TestLambda();
                //TestQuote();

                Console.Read();
            }
        }
    }
}