using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Evaler
{
    class Program
    {
        static List<string> Operators = new List<string>() { "+", "-", "*","/",">", "<", "=", 
            "and", "or", "not", "car", "cdr", "cons", "quote" };

        static List<string> Structs = new List<string>() { "if", "cond" };
        static List<string> Commands = new List<string>() { "help", "exit", "env", "save", "load" };

        static Dictionary<string, string> env = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            Repl();
        }

        static void Repl()
        {
            Console.WriteLine("Welcome to Evaler :)\r\n");

            while (true)
            {
                Console.Write("> ");

                string exp = Console.ReadLine();

                if (exp.StartsWith("(") && Commands.Contains(car(exp)))
                {
                    ExecCmd(car(exp), second(exp));
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

        private static void ExecCmd(string cmd, string args)
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
                    LoadScript(args);
                    break;
                case "help":
                    Console.WriteLine("need to be done :)");
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
            LoadScript("env.dat");
        }

        private static void LoadScript(string path)
        {
            string[] dat = File.ReadAllLines(path);
            foreach (var s in dat)
                Interp(s, env);
        }

        private static void PrintEnv()
        {
            foreach (var item in env)
                Console.WriteLine("  {0}:{1}", item.Key, item.Value);
        }

        private static string SerialEnv(Dictionary<string, string> env)
        {
            StringBuilder cont = new StringBuilder();

            cont.Append("(");
            foreach (var k in env)
                cont.Append("(" + k.Key + " " + k.Value + ")");
            cont.Append(")");

            return cont.ToString();
        }

        private static Dictionary<string, string> DeserialEnv(string cont)
        {
            Dictionary<string, string> env = new Dictionary<string, string>();
            RcursiveDeserial(cont, env);
            return env;
        }

        private static void RcursiveDeserial(string cont, Dictionary<string, string> env)
        {
            string pair = car(cont);
            if (pair != "null")
            {
                env.Add(car(pair), car(cdr(pair)));
                RcursiveDeserial(cdr(cont), env);
            }
        }

        static string Interp(string exp, Dictionary<string, string> env)
        {
            switch (Type(exp))
            {
                case "bool":
                case "num":
                    return exp;
                case "var":
                    return env.ContainsKey(exp) ? env[exp] :
                           string.Format("variable {0} not bound\r\n", exp);
                case "exp":
                    return InterpExp(car(exp), cdr(exp), env);
                case "struct":
                    return InterpStruct(exp, env);
                case "lambda":
                    return InterpLambda(exp, env);
                case "apply":
                    return Apply(exp, env);
                case "define":
                    return InterpDef(exp, env);
                default:
                    return string.Format("fail to interp {0}\r\n", exp);
            }
        }

        private static string InterpLambda(string exp, Dictionary<string, string> env)
        {
            return "(" + exp + " " + SerialEnv(env) + ")";
        }

        private static string InterpDef(string exp, Dictionary<string, string> env)
        {
            if (Type(second(exp)) == "var")
            {
                string var = second(exp);
                string val = Interp(third(exp), env);

                if (!env.ContainsKey(var)) env.Add(var, val);
                else env[var] = val;
            }
            else
            {
                string func = car(second(exp));

                if (!env.ContainsKey(func)) env.Add(func,
                    "((lambda " + cdr(second(exp)) + " " + third(exp) + ") ())");
                else env[func] = third(exp);
            }

            return string.Empty;
        }

        private static string Apply(string exp, Dictionary<string, string> env)
        {
            string e1 = Interp(car(exp), env);
            string e2 = Interp(car(cdr(exp)), env);

            Dictionary<string, string> contex = DeserialEnv(car(cdr(e1)));
            ExtEnv(contex, car(second(car(e1))), e2);
            return Interp(third(car(e1)), contex);
        }

        private static void ExtEnv(string key, string val)
        {
            ExtEnv(env, key, val);
        }

        private static void ExtEnv(Dictionary<string, string> env, string key, string val)
        {
            if (!env.ContainsKey(key)) env.Add(key, val);
            else env[key] = val;
        }

        static string InterpStruct(string exp, Dictionary<string, string> env)
        {
            switch (car(exp))
            {
                case "if":
                    return EvalIf(exp, env);
                case "cond":
                    return EvalCond(exp, env);
                default:
                    return string.Format("unknown struct {0}\r\n", car(exp));
            }
        }

        static string InterpExp(string op, string opd, Dictionary<string, string> env)
        {
            switch (op)
            {
                case "+":
                case "-":
                case "*":
                case "/":
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
                    return Interp(car(exp), env) == "#t" ?
                        Interp(car(cdr(exp)), env) :
                        EvalCondsRecursive(cdr(conds), env);
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
            if (Type(val1) == "num" && Type(val2) == "num")
            {
                int v1 = int.Parse(val1), v2 = int.Parse(val2);
                switch (op)
                {
                    case "+":
                        return (v1 + v2).ToString();
                    case "-":
                        return (v1 - v2).ToString();
                    case "*":
                        return (v1 * v2).ToString();
                    case "/":
                        return (v1 / v2).ToString();
                    default:
                        return string.Empty;
                }
            }
            else
            {
                if (op == "+")
                    return val1 + val2;

                return string.Empty;
            }
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
            else if (IsAtom(exp))
                return "var";
            else if (Operators.Contains(car(exp)))
                return "exp";
            else if (Structs.Contains(car(exp)))
                return "struct";
            else if (car(exp) == "lambda")
                return "lambda";
            else if (car(exp) == "define")
                return "define";
            else if (car(exp) != "null" && (cdr(cdr(exp)) == "()"))
                return "apply";
            else
                return string.Format("unknown type {0}\r\n", exp);
        }

        static bool IsList(string exp)
        {
            return exp.StartsWith("(") && exp.EndsWith(")");
        }

        static bool IsAtom(string exp)
        {
            return !IsList(exp) && !IsKeyword(exp);
        }

        static bool IsKeyword(string exp)
        {
            return Operators.Contains(exp) || Structs.Contains(exp) || Commands.Contains(exp);
        }

        static string car(string exp)
        {
            if (exp == "()" || string.IsNullOrEmpty(exp))
                return "null";
            else if (exp.Length >= 1 && exp[1] == '(')
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