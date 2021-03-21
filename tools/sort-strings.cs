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
            Console.WriteLine("Sort Strings Tool v1.0\n");

            if (args.Length != 2)
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

            SortFolder(inputFolder, hash);
        }

        static void PrintHelp()
        {
            Console.WriteLine("sort-strings.exe - Sorts all .txt files in the input folder, uses hash to remove duplicates and detect collisions.");
            Console.WriteLine("Usage:");
            Console.WriteLine("    sort-strings.exe InputFolder Hash");
            Console.WriteLine("");
            Console.WriteLine("        InputFolder - Folder with the .txt files");
            Console.WriteLine("        Hash - Name of the Hash Algorithm:");
            Console.WriteLine("             Default - default hash");
            Console.WriteLine("             AWC - for AWC audio channel names");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Example:");
            Console.WriteLine(@"    sort-strings.exe RDR2\ArchiveItems Default");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        static void SortFolder(string inputFolder, HashType hash)
        {
            var makeHash = HashManager.GetHashFunction(hash);
            var files = Directory.GetFiles(inputFolder, "*.txt", SearchOption.AllDirectories);

            var sw = Stopwatch.StartNew();
            foreach (var fileName in files)
            {
                Console.WriteLine(fileName + " - Processing file...");
                var fileData = new Dictionary<UInt32, List<string>>();
                var fileLines = File.ReadAllLines(fileName);
                foreach (var line in fileLines)
                {
                    var hashValue = makeHash(line);

                    List<string> hashLine;
                    if (!fileData.TryGetValue(hashValue, out hashLine))
                    {
                        hashLine = new List<string>();
                        fileData.Add(hashValue, hashLine);
                    }

                    hashLine.Add(line);
                }

                var soretedLines = new List<string>();
                foreach (var line in fileData)
                {
                    if (line.Value.Count == 1)
                    {
                        soretedLines.Add(line.Value.First());
                    }
                    else
                    {
                        var test = line.Value.Distinct(HashManager.GetStringComparer(hash)).ToList();
                        if (test.Count == 1)
                        {
                            soretedLines.Add(test.First());
                        }
                        else
                        {
                            Console.WriteLine("Hash Collision found, hash = 0x" + line.Key.ToString("X8"));
                            foreach (var s in test)
                            {
                                Console.WriteLine(s);
                            }
                            Console.WriteLine("");
                            return;
                        }
                    }
                }

                soretedLines.Sort();
                File.WriteAllLines(fileName, soretedLines);
            }
            sw.Stop();
            Console.WriteLine("Files processing done in " + sw.Elapsed.ToString());
        }
    }
}
