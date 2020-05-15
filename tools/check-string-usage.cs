using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;


namespace RageStringsDatabase
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Check String Usage Tool v1.1\n");

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
                CheckStringsForHashCollisions(fileLines, hash);
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

        static void CheckStringsForHashCollisions(List<string> strings, HashType hash)
        {
            var makeHash = HashManager.GetHashFunction(hash);
            var lineComparer = HashManager.GetStringComparer(hash);
            var hashMap = new Dictionary<UInt32, string>();
            var buffer = new List<string>();

            foreach (var line in strings)
            {
                var lineHash = makeHash(line);
                if (hashMap.ContainsKey(lineHash))
                {
                    var testLine = hashMap[lineHash];
                    if (!lineComparer.Equals(line, testLine))
                    {
                        buffer.Add(string.Format("INPUT_COLLISION\t0x{0:X8}\t{1}\t{2}", lineHash, testLine, line));
                    }
                }
                else
                {
                    hashMap.Add(lineHash, line);
                }
            }

            if (buffer.Any())
            {
                Console.WriteLine("#INPUT_COLLISION\tHash\tLine_1\tLine_2");
                buffer.Sort();
                buffer.ForEach(s => Console.WriteLine(s));
                Console.WriteLine("");
            }
        }

        static void CheckStrings(string inputFolder, HashType hash, List<string> strings)
        {
            var converter = new UInt32Converter();
            var makeHash = HashManager.GetHashFunction(hash);
            var lineComparer = HashManager.GetStringComparer(hash);
            var files = Directory.GetFiles(inputFolder, "*.hashes", SearchOption.AllDirectories);
            var testData = strings.Select(s => new KeyValuePair<UInt32, string>(makeHash(s), s)).ToList();

            var bufferCollisions = new List<string>();
            var bufferNew = new List<string>();

            foreach (var fileName in files)
            {
                var hashList = File.ReadAllLines(fileName).Select(s => (UInt32)converter.ConvertFromString(s)).ToDictionary(k => k, v => v);
                var lines = File.ReadAllLines(Path.ChangeExtension(fileName, ".txt")).ToDictionary(k => makeHash(k), v => v);
                var localFileName = fileName.Remove(0, inputFolder.Length + 1);

                foreach (var test in testData)
                {
                    if (hashList.ContainsKey(test.Key))
                    {
                        string currentValue;
                        if (lines.TryGetValue(test.Key, out currentValue))
                        {
                            if (!lineComparer.Equals(currentValue, test.Value))
                            {
                                bufferCollisions.Add(string.Format("COLLISION\t{0}\t{1}\t{2}", localFileName, currentValue, test.Value));
                            }
                        }
                        else
                        {
                            bufferNew.Add(string.Format("NEW\t{0}\t{1}", localFileName, test.Value));
                        }
                    }
                }
            }

            if (bufferCollisions.Any())
            {
                Console.WriteLine("#COLLISION\tFile_name\tCurrent_line\tInput_line");
                bufferCollisions.Sort();
                bufferCollisions.ForEach(s => Console.WriteLine(s));
                Console.WriteLine("");
            }

            if (bufferNew.Any())
            {
                Console.WriteLine("#NEW\tFile_name\tInput_line");
                bufferNew.Sort();
                bufferNew.ForEach(s => Console.WriteLine(s));
                Console.WriteLine("");
            }
        }
    }
}
