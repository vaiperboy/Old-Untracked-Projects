using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Console = Colorful.Console;
using Flurl.Http;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Discord_Bots {
    public static class TokenLoader {
        public static class Local { 

            public static List<Token> getTokens(out bool isJson, string path = null) {
                isJson = false;
                var jsonOut = new JObject();
                if (string.IsNullOrEmpty(path))
                    path = AppDomain.CurrentDomain.BaseDirectory;
                var tokens = new List<Token>();
                //DirectoryInfo info = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                DirectoryInfo info = new DirectoryInfo(path);
                Console.Clear();
                Console.WriteLine("Token .txt loader (make sure .txt is in same folder)", Color.Gold);
                //var jsonFiles = info.GetFiles("*.json");
                var txtFiles = info.GetFiles("*.txt");
                var files = txtFiles;
                if (files.Length > 0) {

                    printFiles:
                    for (int i = 1; i <= files.Length; i++) {
                        Console.WriteLine($"{i.ToString()}. {files[i - 1].Name}");
                    }

                    askNum:
                    Console.Write($"\n\nSelect the .txt to load tokens from [1-{files.Length}]: ");
                    Console.WriteLine("Input numbers only!", Color.Red);
                    Console.Write(">");

                    var isNum = uint.TryParse(Console.ReadLine(), out uint input);
                    while (!isNum) {
                        Console.WriteLine("Input positive numbers only!", Color.Red);
                        goto askNum;
                    }
                    while (input > files.Length) {
                        Console.WriteLine("Make sure number is within range!", Color.Red);
                        goto printFiles;
                    }
                    var name = files[input - 1].FullName;

                    var allTokens = File.ReadAllText(name).ReplaceWhiteSpace("\n");
                    foreach (string current in allTokens.Split('\n')) {
                        if (current.Length > 0) {
                            var tmp = new Token();
                            tmp.token = current.Split(':')[0];
                            if (current.Contains(':'))
                                tmp.Outh = current.Split(':')[1];
                            tokens.Add(tmp);
                        }
                    }
                }
                else {
                    Console.WriteLine("no .txt file found....\nplease put and reload app", Color.Red);
                    Console.ReadLine();
                }
                return tokens;
            }

        }
    }
}
