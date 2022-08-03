using System;
using System.IO;
using System.Threading.Tasks;

namespace Medior.Utilities
{
    internal static class FileHelper
    {
        public static async Task ReplaceLineInFile(string filePath, string matchPattern, string replaceLineWith, int maxMatches = 1)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var matchCount = 0;
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(matchPattern, StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = replaceLineWith;
                    matchCount++;
                }
                if (matchCount >= maxMatches)
                {
                    break;
                }
            }
            await File.WriteAllLinesAsync(filePath, lines);
        }
    }
}
