using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InputHandler;

namespace MultiSSH
{
    public static class ScopeHandler
    {
        static readonly char[] scopeIllegal = new[] { ',', ' ' };

        public static int Define(Func<string> readLine, bool prompt)
        {
            Program.Log("Define entries:", prompt);
            string scope = Program.currentScope;
            Add(scope);
            int total = 0;
            foreach (string s in Input.YieldUntilEmpty(readLine))
            {
                if (s[.."scope".Length] == "scope")
                {
                    scope = s[("scope".Length + 1)..];
                    Add(scope);
                }
                else
                {
                    var l = UrlExpander.Expand(s);
                    Program.scopes[scope].AddRange(l);
                    total += l.Count;
                }
            }

            return total;
        }

        public static int Define(string[] split)
        {
            switch (split.Length)
            {
                case 2:
                    Add(split[1]);
                    return 0;
                case 3:
                    var l = UrlExpander.Expand(split[2]);
                    Add(split[1], l);
                    return l.Count;
                case 4:
                {
                    if (split[2] == "scope") Clone(split[3], split[1]);
                    if (split[2] == "file") throw new NotImplementedException();
                    return Program.scopes[split[3]].Count;
                }
                default:
                    throw new NotImplementedException("Impossible error. Report this to a developer.");
            }
        }

        public static int Exclude(Func<string> readLine, bool prompt)
        {
            Program.Log("Exclude entries:", prompt);
            int total = 0;
            string scope = Program.currentScope;
            foreach (string s in Input.YieldUntilEmpty(readLine))
            {
                if (s[.."scope".Length] == "scope")
                {
                    string newScope = s[("scope".Length + 1)..];
                    if (Program.scopes.ContainsKey(newScope)) scope = newScope;
                }
                else
                {
                    foreach (string x in UrlExpander.Expand(s))
                    {
                        total += Program.scopes[scope].Remove(x) ? 1 : 0;
                    }
                }
            }

            return total;
        }

        public static int Exclude(string[] split)
        {
            if (split.Length == 3)
            {
                int total = 0;
                if (Program.scopes.ContainsKey(split[1]))
                {
                    foreach (string x in UrlExpander.Expand(split[2])) total += Program.scopes[split[1]].Remove(x) ? 1 : 0;
                    return total;
                }
                else throw new ArgumentException($"One of specified scopes ({split[1]}, {split[3]}) does not exist!");
            }
            else if (split.Length == 4)
            {
                if (split[2] == "scope")
                {
                    int total = 0;
                    if (Program.scopes.ContainsKey(split[1]) && Program.scopes.ContainsKey(split[3]))
                    {
                        foreach (string x in Program.scopes[split[3]]) total += Program.scopes[split[1]].Remove(x) ? 1 : 0;
                        return total;
                    }
                    else throw new ArgumentException($"One of specified scopes ({split[1]}, {split[3]}) does not exist!");
                }
                else if (split[2] == "file") throw new NotImplementedException();
                else throw new NotImplementedException();
            }
            else throw new NotImplementedException("Impossible error. Report this to a developer.");
        }

        public static void Move(string scope, string name)
        {
            Clone(scope, name);
            Remove(scope);
        }

        public static void Clone(string scope, string name)
        {
            if (Program.scopes.ContainsKey(scope))
            {
                Add(name);
                Program.scopes[name].AddRange(Program.scopes[scope]);
            }
            else throw new ArgumentException($"Specified scope {scope} does not exist!");
        }

        public static void Remove(string scope) => Program.scopes.Remove(scope);

        public static void Print(string scope, Action<object> writeLine)
        {
            if (Program.scopes.ContainsKey(scope))
            {
                writeLine($"Scope {scope} has values:");

                foreach (string s in Program.scopes[scope]) writeLine($"    {s}");
            }
            else throw new ArgumentException($"Specified scope {scope} does not exist!");
        }

        private static void Add(string scope) => Add(scope, new());

        private static void Add(string scope, List<string> list)
        {
            if (scope.Any(x => scopeIllegal.Contains(x))) throw new FormatException("Scope names cannot include commas.");
            if (!Program.scopes.ContainsKey(scope)) Program.scopes.Add(scope, list);
        }
    }
}
