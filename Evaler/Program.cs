using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Evaler
{
    class Program
    {
        static List<string> Operators = new List<string>() { "+", "-", ">", "<", "=", "and", "or", "not", "car", "cdr", "cons", "quote" };
        static List<string> Structs = new List<string>() { "if", "cond" };
        static List<string> Cmds = new List<string>() { "exit", "env", "save", "load" };

        static Dictionary<string, string> env = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            Repl();
        }

        static void Repl()
        {
            Console.WriteLine("Welcome to Evaler !\r\n");

            while (true)
            {
                Console.Write("> ");

                string exp = Console.ReadLine();

                if (exp.StartsWith("(") && Cmds.Contains(car(exp)))
                {
                    ExecCmd(car(exp));
                    continue;
                }

                while (exp.StartsWith("(") && !exp.EndsWith(")"))
                {
                    if (!exp.EndsWith(" "))
                        exp += " ";
                    exp += Console.ReadLine();
                }

                if (!string.IsNullOrEmpty(exp))
                {
                    string val = Interp(exp, env);

                    if (!string.IsNullOrEmpty(val))
                        Console.WriteLine(val);
                }
            }
        }

        private static void ExecCmd(string cmd)
        {
            switch (cmd)
            {
                case "exit":
                    Environment.Exit(0);
                    break;
                case "env":
                    PrintEnv();
                    break;
                case "save":
                    SaveEnv();
                    break;
                case "load":
                    LoadEnv();
                    break;
            }
        }

        private static void SaveEnv()
        {
            StringBuilder data = new StringBuilder();
            foreach (var item in env)
                data.AppendLine(item.Key + "|" + item.Value);

            File.WriteAllText("env.dat", data.ToString());
        }

        private static void LoadEnv()
        {
            string path = "env.dat";
            string[] dat = File.ReadAllLines(path);

            foreach (var s in dat)
            {
                string[] items = s.Split('|');

                string atom = items[0];
                string val = items[1];

                if (!env.ContainsKey(atom)) env.Add(atom, val);
                else env[atom] = val;
            }
        }

        private static void PrintEnv()
        {
            foreach (var item in env)
                Console.WriteLine("  {0}:{1}", item.Key, item.Value);
        }

        static string Interp(string exp, Dictionary<string, string> env)
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
                case "eval":
                    return "";
                case "call":
                    return CalcCall(exp, env);
                case "lambda":
                    return CalcLambda(exp, env);
                case "def":
                    return InterDefine(exp, env);
                default:
                    return "error";
            }
        }

        private static string InterDefine(string exp, Dictionary<string, string> env)
        {
            if (Type(second(exp)) == "atom")
            {
                string atom = second(exp);
                string val = Interp(third(exp), env);

                if (!env.ContainsKey(atom)) env.Add(atom, val);
                else env[atom] = val;
            }
            else
            {
                string func = car(second(exp));

                if (!env.ContainsKey(func)) env.Add(func, "(lambda " + cdr(second(exp)) + " " + third(exp) + ")");
                else env[func] = third(exp);
            }

            return string.Empty;
        }

        private static string CalcCall(string exp, Dictionary<string, string> env)
        {
            if (exp.StartsWith("((lambda"))
            {
                string func = car(exp);
                string args = cdr(exp);

                LoadArgs(second(func), args, env);

                return Interp(third(func), env);
            }
            else
            {
                string func = "(" + env[car(exp)] + " " + cdr(exp).TrimStart('(');
                return CalcCall(func, env);
            }
        }

        private static void LoadArgs(string pars, string vals, Dictionary<string, string> env)
        {
            if (pars != "()")
            {
                string atom = car(pars);
                string val = Interp(car(vals), env);

                if (!env.ContainsKey(atom)) env.Add(atom, val);
                else env[atom] = val;

                LoadArgs(cdr(pars), cdr(vals), env);
            }
        }

        static string CalcLambda(string exp, Dictionary<string, string> env)
        {
            return Interp(third(exp), env);
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
                    return ArithOp(op, Interp(car(opd), env), Interp(second(opd), env));
                case ">":
                case "<":
                case "=":
                    return CompOp(op, Interp(car(opd), env), Interp(second(opd), env));
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
                    return Interp(car(cdr(exp)), env);
                else
                    return Interp(car(exp), env) == "#t" ? Interp(car(cdr(exp)), env) : EvalCondsRecursive(cdr(conds), env);
            }
        }

        private static string EvalIf(string exp, Dictionary<string, string> env)
        {
            string cond = car(cdr(exp));

            return Interp(cond, env) == "#t" ?
                   Interp(car(cdr(cdr(exp))), env) :
                   Interp(car(cdr(cdr(cdr(exp)))), env);
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
                return Interp(car(opd), env) == "#t" ? "#f" : "#t";
            else if (op == "and" || op == "or")
            {
                string val1 = Interp(car(opd), env), val2 = Interp(second(opd), env);
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
            else if (exp.StartsWith("(") && car(exp) == "def")
                return "def";
            else if (exp.StartsWith("((lambda") || exp.StartsWith("(") && env.ContainsKey(car(exp)))
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

    }
}