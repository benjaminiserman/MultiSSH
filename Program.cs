using System;
using System.Collections.Generic;

namespace MultiSSH
{
    class Program
    {
        public static Dictionary<string, List<string>> scopes = new();
        public static Dictionary<string, string> variables = new();
        public static string currentScope = "default";
        public static bool mainPrompt = true;  

        static void Main()
        {
            Console.WriteLine("Enter commands:");

            while (true)
            {
                try
                {
                    Command(Console.ReadLine, Console.WriteLine, mainPrompt);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }

        static void Command(Func<string> readLine, Action<object> writeLine, bool prompt)
        {
            string s = readLine();
            string[] split = s.Split();
            int x;
            string scopeUsed;

            switch (split[0].ToLower())
            {
                case "define":
                case "def":
                    if (split.Length > 5) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 0-4 arguments");
                    else if (split.Length == 1) x = ScopeHandler.Define(readLine, prompt);
                    else x = ScopeHandler.Define(split);
                    Log($"{x} entries created.", prompt);
                    break;
                case "exclude":
                case "exc": // exc, exc a scope b
                    if (split.Length == 1) x = ScopeHandler.Exclude(readLine, prompt);
                    else if (split.Length is 3 or 4) x = ScopeHandler.Exclude(split); 
                    else throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 2-4 arguments");
                    Log($"{x} entries removed.", prompt);
                    break;
                case "move":
                case "mv":
                    if (split.Length != 3) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 2 arguments");
                    ScopeHandler.Move(split[1], split[2]);
                    Log($"Scope {split[1]} moved to {split[2]}", prompt);
                    break;
                case "clone":
                case "cp":
                    if (split.Length != 3) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 2 arguments");
                    ScopeHandler.Clone(split[1], split[2]);
                    Log($"Scope {split[1]} cloned to {split[2]}", prompt);
                    break;
                case "delete":
                case "del":
                case "rm":
                    if (split.Length != 2) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 1 argument");
                    ScopeHandler.Remove(split[1]);
                    Log($"Scope {split[1]} removed", prompt);
                    break;
                case "print":
                case "echo":
                case "say":
                case "write":
                    if (split.Length > 2) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 0-1 arguments");
                    scopeUsed = split.Length == 1 ? currentScope : split[1];
                    ScopeHandler.Print(scopeUsed, writeLine);
                    break;
                case "reset":
                    if (split.Length > 1) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 0 arguments");
                    scopes.Clear();
                    variables.Clear();
                    Log($"Variables and scopes cleared.", prompt);
                    break;
                case "scope":
                    if (split.Length != 2) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 1 argument");
                    else if (scopes.ContainsKey(split[1])) currentScope = split[1];
                    else throw new ArgumentException($"Specified scope {split[1]} does not exist!");
                    break;
                case "clear":
                case "clr":
                    if (split.Length > 1) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 0 arguments");
                    Console.Clear();
                    break;
                case "silence":
                case "sil":
                    if (split.Length > 1) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 0 arguments");
                    mainPrompt = !mainPrompt;
                    break;
                case "length":
                case "len":
                    if (split.Length > 2) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 0-1 arguments");
                    scopeUsed = split.Length == 1 ? currentScope : split[1];
                    if (!scopes.ContainsKey(scopeUsed)) throw new ArgumentException($"Specified scope {scopeUsed} does not exist!");
                    else writeLine($"Scope {scopeUsed} has {scopes[scopeUsed].Count} entries.");
                    break;
                case "list":
                case "ls":
                    if (split.Length > 1) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 0 arguments");
                    writeLine($"There are currently {scopes.Count} scopes:");
                    foreach (var kvp in scopes) writeLine($"    {kvp.Key} ({kvp.Value.Count} entries)");
                    break;
                default:
                    throw new MissingMethodException("No such command exists.");
            }
        }

        public static void Log(object x, bool prompt)
        {
            if (prompt) Console.WriteLine(x);
        }
    }
}
