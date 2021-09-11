using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using PrettyPrompt.Consoles;

namespace SharpC2
{
    public static class Extensions
    {
        public static void PrintMessage(this IConsole console, string message)
        {
            var currentColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            console.WriteLine(Environment.NewLine + $"[+] {message}" + Environment.NewLine);
            Console.ForegroundColor = currentColour;
        }
        
        public static void PrintWarning(this IConsole console, string warning)
        {
            var currentColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            console.WriteLine(Environment.NewLine + $"[!] {warning}" + Environment.NewLine);
            Console.ForegroundColor = currentColour;
        }
        
        public static void PrintError(this IConsole console, string error)
        {
            var currentColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            console.WriteLine(Environment.NewLine + $"[x] {error}" + Environment.NewLine);
            Console.ForegroundColor = currentColour;
        }
        
        public static IEnumerable<string> GetPartialPath(string path)
        {
            // could be something like C:\Users or /Users
            var index = path.LastIndexOf(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/');
            index++;

            var partial = path[..index];

            if (!Directory.Exists(partial)) return Array.Empty<string>();

            return Directory.EnumerateFileSystemEntries(partial)
                .Where(p => p.StartsWith(path)).ToArray();
        }
    }
}