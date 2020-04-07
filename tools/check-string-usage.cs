using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;


namespace RageStringsDatabase
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Check String Usage Tool v1.0\n");

            if (args.Length != 3)
            {
                PrintHelp();
                return;
            }

            var inputFolder = args[0];
            if (!Path.IsPathRooted(inputFolder))
            {
                inputFolder = Path.Combine(Environment.CurrentDirectory, inputFolder);
            }

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine("Error: Input folder not found - " + inputFolder);
                PrintHelp();
                return;
            }

            HashType hash;
            if (!Enum.TryParse(args[1], out hash))
            {
                Console.WriteLine("Error: Unknown Hash Algorithm - " + args[2]);
                PrintHelp();
                return;
            }

            var inputLine = args[2];
            if (inputLine.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!Path.IsPathRooted(inputLine))
                {
                    inputLine = Path.Combine(Environment.CurrentDirectory, inputLine);
                }

                if (!File.Exists(inputLine))
                {
                    Console.WriteLine("Error: Input folder not found - " + inputFolder);
                    PrintHelp();
                    return;
                }

                var fileLines = new List<string>( File.ReadLines(inputLine) );
                CheckStrings(inputFolder, hash, fileLines);
            }
            else
            {
                CheckStrings(inputFolder, hash, new List<string>() { inputLine });
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("check-string-usage.exe - check if the specific string is used in the game by hash");
            Console.WriteLine("Usage:");
            Console.WriteLine("    check-string-usage.exe InputFolder Hash InputString");
            Console.WriteLine("");
            Console.WriteLine("        InputFolder - Folder with the .hash files");
            Console.WriteLine("        Hash - Name of the Hash Algorithm:");
            Console.WriteLine("             Default - default hash");
            Console.WriteLine("             AWC - for AWC audio channel names");
            Console.WriteLine("        InputString - String or file name. If .txt file name is specified tool will check all the strings in the file");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Example:");
            Console.WriteLine(@"    check-string-usage.exe RDR2\ArchiveItems Default rowboat");
            Console.WriteLine(@"    check-string-usage.exe RDR2\ArchiveItems Default TestList.txt");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        static void CheckStrings(string inputFolder, HashType hash, List<string> strings)
        {
            var makeHash = HashManager.GetHashFunction(hash);
            var files = Directory.GetFiles(inputFolder, "*.hashes", SearchOption.AllDirectories);

            var testData = new Dictionary<UInt32, string>();
            foreach (var line in strings)
            {
                testData.Add(makeHash(line), line);
            }

            foreach (var fileName in files)
            {
                var hashList = File.ReadAllLines(fileName).Select(s => UInt32.Parse(s)).ToDictionary(k => k, v => v);

                foreach (var test in testData)
                {
                    if (hashList.ContainsKey(test.Key))
                    {
                        Console.WriteLine(string.Format("\"{0}\" used in file {1}", test.Value, fileName));
                    }
                }
            }
        }
    }
}
