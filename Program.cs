using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Linq;

namespace batchpsvpfsparser {
    class Program {
        static void Main(string[] args) {
            string appPath = Assembly.GetExecutingAssembly().Location;
            string appFolder = Path.GetDirectoryName(appPath);
            Console.WriteLine("Base folder: " + appFolder);

            if (File.Exists(appFolder + "\\" + "psvpfsparser.exe")) {
                Console.WriteLine("psvpfsparser.exe found.");
            } else {
                Console.WriteLine("psvpfsparser.exe not found. Will quit.");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }

            if (Directory.Exists("games")) {
                Console.WriteLine("Games folder found.");
            } else {
                Console.WriteLine("Games folder not found. Will quit.");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }

            string[] directories = Directory.GetDirectories(appFolder + "\\" + "games");
            string[] directoriesOnly = new string[directories.Length];
            for (int i = 0; i < directories.Length; i++) {
                directoriesOnly[i] = Path.GetFileName(directories[i]);
            }

            string[] TSVs = Directory.GetFiles(appFolder, "*.TSV");
            List<string> TSVcombined = new List<string>();
            if (TSVs.Length == 0) {
                Console.WriteLine("No TSV files found. Will quit.");
                Thread.Sleep(2000);
                Environment.Exit(0);
            } else {
                for (int i = 0; i < TSVs.Length; i++) {
                    string[] lines = File.ReadAllLines(TSVs[i]);
                    TSVcombined = TSVcombined.Concat(lines).ToList();
                }
            }
            string[] TSVcomb = TSVcombined.ToArray();
            for(int i = 0; i < TSVcomb.Length; i++) {
                Console.WriteLine(TSVcomb[i]);
            }
            Console.WriteLine("Loaded all TSVs into memory.");

            try {
                if (!Directory.Exists("output")) {
                    Directory.CreateDirectory("output");
                    Console.WriteLine("Created an output folder.");
                } else {
                    Console.WriteLine("An output folder already exists.");
                }
            } catch {
                Console.WriteLine("Couldn't create an output folder. Will quit.");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }

            try {
                if (!Directory.Exists("temp")) {
                    Directory.CreateDirectory("temp");
                    Console.WriteLine("Created an temp folder.");
                } else {
                    Console.WriteLine("A temp folder already exists.");
                    Console.WriteLine("Deleting the decrypted game files.");
                    DirectoryInfo di = new DirectoryInfo("temp");

                    foreach (FileInfo file in di.GetFiles()) {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories()) {
                        dir.Delete(true);
                    }
                }
            } catch {
                Console.WriteLine("Couldn't create a temp folder. Will quit.");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }

            Thread.Sleep(2000);
            string[] TSVcombIDs = new string[TSVcomb.Length];
            Console.WriteLine("Parsing TSVs for title IDs.");
            for (int i = 0; i < TSVcomb.Length; i++) {
                TSVcombIDs[i] = TSVcomb[i].Substring(0, 9);
                Console.WriteLine(TSVcombIDs[i]);
            }
            for (int i = 0; i < directoriesOnly.Length; i++) {
                Console.WriteLine("Looking for " + directoriesOnly[i] + " in TSVs.");
                int index = Array.IndexOf(TSVcombIDs, directoriesOnly[i]);
                string line = "";

                try {
                    line = TSVcomb[index];
                    Console.WriteLine(directoriesOnly[i] + " found in TSVs: "+ TSVcomb[index]);
                } catch {
                    Console.WriteLine("Couldn't find " + directoriesOnly[i] + " in TSVs.");
                    break;
                }
                string zrif = "";
                string lookFor = "KO5";
                string lookFor2 = "MISSING";
                if (line.Contains(lookFor)) {
                    int start = line.IndexOf(lookFor) + lookFor.Length;
                    int end = line.IndexOf("\t", start);
                    zrif = line.Substring(start, end - start);
                    zrif = "KO5" + zrif;
                    Console.WriteLine("zRIF for " + directoriesOnly[i] + " is: " + zrif);
                } else if (line.Contains(lookFor2)) {
                    Console.WriteLine(directoriesOnly[i] + " is missing its zRIF in TSVs.");
                    break;
                } else {
                    Console.WriteLine("Couldn't find anything related to zRIF for " + directoriesOnly[i] + " in TSVs.");
                    break;
                }


                Thread.Sleep(2000);
                
                Process process = new Process();
                process.StartInfo.FileName = appFolder + "\\" + "workaroundbatch.bat";
                process.StartInfo.Arguments = directories[i] + " " + zrif;
                Console.WriteLine(process.StartInfo.Arguments); Thread.Sleep(2000);
                Console.WriteLine(process.StartInfo.FileName + process.StartInfo.Arguments);
                Console.WriteLine("Decrypting " + directoriesOnly[i]);
                process.Start();
                process.WaitForExit();
                try {
                    Console.WriteLine("Renaming and moving the eboot.bin to output folder.");
                    File.Move("temp" + "\\" + "eboot.bin", "output" + "\\" + directoriesOnly[i] + ".bin");
                    Console.WriteLine("Deleting the decrypted game files.");
                    DirectoryInfo di = new DirectoryInfo("temp");

                    foreach (FileInfo file in di.GetFiles()) {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories()) {
                        dir.Delete(true);
                    }

                } catch (Exception x) {
                    Console.WriteLine(x.ToString());
                }
            }
            Directory.Delete("temp", true);
            Console.WriteLine("Deleting the temporary folder.");
            Console.WriteLine("All done! Press a key to exit.");
            Console.ReadKey();
        }
    }
}
