using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SharpC2
{
    public static class Utilities
    {
        public static IEnumerable<string> GetPartialPath(string path)
        {
            // could be something like C:\Users or /Users
            var index = path.LastIndexOf(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/');

            if (index == -1)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    path = $"{path}:\\";
                }
            }
            else
            {
                index++;
                path = path[..index];
            }

            if (!Directory.Exists(path)) return Array.Empty<string>();

            return Directory.EnumerateFileSystemEntries(path)
                .Where(p => p.StartsWith(path)).ToArray();
        }
    }
}