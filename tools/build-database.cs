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
            Console.WriteLine("Build Strings Database Tool v1.0\n");

            if (args.Length != 4)
            {
                PrintHelp();
                return;
            }

            var appID = args[0];
            if (appID != ToolsConstants.APP_ID_OPENIV)
            {
                Console.WriteLine("Error: Unknown Application ID - " + appID);
                PrintHelp();
                return;
            }

            var dbName = args[1];

            HashType hash;
            if (!Enum.TryParse(args[2], out hash))
            {
                Console.WriteLine("Error: Unknown Hash Algorithm - " + args[2]);
                PrintHelp();
                return;
            }

            var inputFolder = args[3];
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

            switch (appID)
            {
                case ToolsConstants.APP_ID_OPENIV:
                    {
                        BuildForOpenIV(dbName, hash, inputFolder);
                        break;
                    }
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("build-database.exe - build single file strings database");
            Console.WriteLine("Usage:");
            Console.WriteLine("    build-database.exe APPID DatabaseName Hash InputFolder");
            Console.WriteLine("");
            Console.WriteLine("        APPID - Application ID, only OPENIV is currently supported.");
            Console.WriteLine("        DatabaseName - Name of the output database, examples:");
            Console.WriteLine("             ArchiveItems");
            Console.WriteLine("             AudioTracks");
            Console.WriteLine("             TextKeys");
            Console.WriteLine("             etc...");
            Console.WriteLine("        Hash - Name of the Hash Algorithm:");
            Console.WriteLine("             Default - default hash");
            Console.WriteLine("             AWC - for AWC audio channel names");
            Console.WriteLine("        InputFolder - Folder with the source files");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Example:");
            Console.WriteLine(@"    build-database.exe OPENIV ArchiveItems Default RDR2\ArchiveItems");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        static void BuildForOpenIV(string databaseName, HashType hash, string inputFolder)
        {
            var makeHash = HashManager.GetHashFunction(hash);
            var files = Directory.GetFiles(inputFolder, "*.txt", SearchOption.AllDirectories);

            var sw = Stopwatch.StartNew();
            var linesData = new Dictionary<string, List<string>>();
            foreach (var fileName in files)
            {
                Console.WriteLine(fileName + " - Reading file...");
                var containerName = Path.GetFileNameWithoutExtension(fileName).ToUpper();
                var fileLines = File.ReadAllLines(fileName);
                List<string> containerLines;
                if (linesData.TryGetValue(containerName, out containerLines))
                {
                    var buffer = new List<string>(containerLines);
                    buffer.AddRange(fileLines);

                    containerLines.Clear();
                    containerLines.AddRange(buffer.Distinct());
                }
                else
                {
                    containerLines = new List<string>(fileLines);
                    linesData.Add(containerName, containerLines);
                }
            }
            sw.Stop();
            Console.WriteLine("Files reading done in " + sw.Elapsed.ToString());

            sw = Stopwatch.StartNew();
            var database = new Dictionary<UInt32, List<StringLine>>();
            foreach (var container in linesData)
            {
                Console.WriteLine(container.Key + " - Processing container...");

                foreach (var stringLine in container.Value)
                {
                    var hashValue = makeHash(stringLine);

                    List<StringLine> hashLine;
                    if (!database.TryGetValue(hashValue, out hashLine))
                    {
                        hashLine = new List<StringLine>();
                        database.Add(hashValue, hashLine);
                    }

                    hashLine.Add(new StringLine() { Container = container.Key, Value = stringLine });
                }
            }
            sw.Stop();
            Console.WriteLine("Containers processing done in " + sw.Elapsed.ToString());

            sw = Stopwatch.StartNew();
            var databaseLines = new List<string>();
            databaseLines.Add(string.Empty);
            foreach (var hashData in database)
            {
                var hashLines = hashData.Value;
                if (hashLines.Count > 1)
                {
                    hashLines = hashLines.Distinct(HashManager.GetStringLineComparer(hash)).ToList();
                }

                if (hashLines.Count > 1)
                {
                    hashLines = hashData.Value;
                    foreach (var line in hashLines)
                    {
                        databaseLines.Add(line.Container + "\\" + line.Value);
                    }
                }
                else
                {
                    databaseLines.Add(hashLines.First().Value);
                }
            }
            sw.Stop();
            Console.WriteLine("Strings processing done in " + sw.Elapsed.ToString());

            sw = Stopwatch.StartNew();
            databaseLines.Sort();
            File.WriteAllLines(databaseName + ".txt", databaseLines);
            Console.WriteLine("File saved in " + sw.Elapsed.ToString());
        }
    }
}
