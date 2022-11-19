using System;
using System.IO;
using System.Linq;


namespace COD_Discord_Bot {
    class Helper {
        public async static void RemoveEmail(string path, string email) {
            if (File.Exists(path)) {
                try {
                    string[] lines = await File.ReadAllLinesAsync(path);
                    int prevLines = lines.Length;
                    lines = lines
                        .Where(x => !x.Split(':')[0].Equals(email, StringComparison.OrdinalIgnoreCase)).ToArray();
                    await File.WriteAllLinesAsync(path, lines);
                }
                catch (Exception) {

                }
            }
        }
    }
}
