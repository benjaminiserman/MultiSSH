using System;
using System.Collections.Generic;

namespace MultiSSH;

public static class UrlExpander
{
    public static List<string> Expand(string input, int iStart = 0, List<string> list = null)
    {
        int openIndex = -1, closeIndex = -1, dashIndex = -1, start = 0, end = 0;

        int i = iStart;

        for (; i < input.Length; i++) // find start
        {
            if (input[i] == '(' && (i == 0 || input[i - 1] != '\\')) // '\\' to skip canceled parentheses
            {
                openIndex = i++;
                break;
            }
        }

        for (; i < input.Length; i++) // find dash
        {
            if (char.IsDigit(input[i]))
            {
                start *= 10;
                start += input[i] - '0';
            }
            else
            {
                if (input[i] != '-') throw new FormatException("URL is missing dash.");
                dashIndex = i++;
                break;
            }
        }

        for (; i < input.Length; i++) // find end
        {
            if (char.IsDigit(input[i]))
            {
                end *= 10;
                end += input[i] - '0';
            }
            else
            {
                if (input[i] != ')') throw new FormatException("URL is missing closing parenthesis.");
                closeIndex = i++;
                break;
            }
        }

        if (start > end) // flip if backwards
        {
            int swap = start;
            start = end;
            end = swap;
        }

        list ??= new();

        if (openIndex == -1)
        {
            list.Add(input);
            return list;
        }

        bool shouldContinue = false;
        for (; i < input.Length; i++)
        {
            if (input[i] == '(' && input[i - 1] != '\\') // '\\' to skip canceled parentheses
            {
                shouldContinue = true;
                break;
            }
        }

        if (shouldContinue)
        {
            for (int j = start; j <= end; j++)
            {
                string s = $"{input[..openIndex]}{j}{input[(closeIndex + 1)..]}";
                list.AddRange(Expand(s, closeIndex + 1 - (input.Length - s.Length)));
            }
        }
        else
        {
            for (int j = start; j <= end; j++)
            {
                list.Add($"{input[..openIndex]}{j}{(closeIndex < input.Length - 1 ? input[(closeIndex + 1)..] : string.Empty)}");
            }
        }

        return list;
    }
}
