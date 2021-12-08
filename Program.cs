namespace MultiSSH;
using System;
using System.Collections.Generic;
using System.Text;

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
                x = split.Length switch
                {
                    > 5 => throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 0-4 arguments"),
                    1 => ScopeHandler.Define(readLine, prompt),
                    _ => ScopeHandler.Define(split),
                };

                Log($"{x} entries created.", prompt);
                break;
            case "exclude":
            case "exc": // exc, exc a scope b
                x = split.Length switch
                {
                    1 => ScopeHandler.Exclude(readLine, prompt),
                    3 or 4 => ScopeHandler.Exclude(split),
                    _ => throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 2-4 arguments"),
                };
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
                currentScope = split.Length != 2
                    ? throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes 1 argument")
                    : scopes.ContainsKey(split[1]) 
                        ? split[1] 
                        : throw new ArgumentException($"Specified scope {split[1]} does not exist!");
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
            case "command":
                if (split.Length < 4) throw new ArgumentException($"Improper number of arguments. {split[0].ToUpper()} takes at least two arguments.");

                StringBuilder sb = new();
                foreach (var y in split[3..]) sb.Append($"{y} ");
                sb.Remove(sb.Length - 1, 1);
                string command = sb.ToString();

                foreach (string scope in scopes[currentScope])
                {
                    var c = new Connection(scope, split[1], split[2], 2220);
                    c.Connect();
                    c.RunCommand(command);

                    (string text, _) = c.ReadText(out byte[] _, out byte[] _);
                    writeLine($"{scope}: {text}");
                }

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
