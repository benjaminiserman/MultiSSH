using System;
using System.Collections.Generic;

namespace MultiSSH
{
    public static class InputHandler
    {
        public static void Input(Func<string> getString, Action<string> action)
        {
            while (true)
            {
                try
                {
                    action(getString());
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}... Please try again.");
                }
            }
        }

        public static string Input(Func<string> getString, Func<string, bool> check)
        {
            string s;
            while (!check(s = getString()))
            {
                Console.WriteLine("Invalid format... Please try again.");
            }
            return s;
        }

        public static bool InputYN(Func<string> getString)
        {
            while (true)
            {
                switch (getString().Trim().ToLower())
                {
                    case "y":
                    case "yes":
                    case "t":
                    case "true":
                        return true;
                    case "n":
                    case "no":
                    case "f":
                    case "false":
                        return false;
                    default:
                        Console.WriteLine("Invalid format... use y or n.");
                        continue;
                }
            }
        }

        public static IEnumerable<string> GetUntilBlank(Func<string> getLine)
        {
            while (true)
            {
                string s = getLine();
                if (string.IsNullOrWhiteSpace(s)) break;
                else yield return s;
            }
        }
    }
}