using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Drawing;

namespace Blizzard_Account_Creator {
    public class Helper {
        private static Random random = new Random();

        public static string RandomString(int length) {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToLower();
            var output = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            //ensure at least one letter and number are returned
            return output.Any(char.IsDigit) && output.Any(char.IsNumber)
                ? output : RandomString(length);
        }

        public static void AddHeaders(HttpRequestMessage msg) {
            msg.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            msg.Headers.TryAddWithoutValidation("Host", "eu.battle.net");
            msg.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            msg.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            msg.Headers.TryAddWithoutValidation("Connection", "keep-alive");
            msg.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
            msg.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
            msg.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
            msg.Headers.TryAddWithoutValidation("Sec-Fetch-User", "?1");
            msg.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            msg.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36");
            msg.Headers.TryAddWithoutValidation("Cache-Control", "max-age=0");
            msg.Headers.TryAddWithoutValidation("origin", "https://eu.battle.net");
            msg.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
            msg.Headers.TryAddWithoutValidation("Cotent-Length", msg.Content.ReadAsStringAsync().Result.Length.ToString());
        }

        public static void DeleteIfExists(string file) {
            if (File.Exists(file)) File.Delete(file);
        }

        private static readonly object locker = new object();
        public async static void RemoveEmail(string path, string email) {
            if (File.Exists(path)) { 
                lock (locker) {
                    string[] lines = File.ReadAllLines(path);
                    int prevLines = lines.Length;
                    lines = lines
                        .Where(x => !x.Split(':')[0].Equals(email, StringComparison.OrdinalIgnoreCase)).ToArray();
                    File.WriteAllLines(path, lines);
                }
            }
        }
        

       public static List<Rectangle> DivideScreenHorizontally(Size screen, int howMany, Size windowSize = default) {
            if (windowSize == default) windowSize = new Size(640, 640); //gotta love hardcoding stuff
            var result = new List<Rectangle>();
            Point currentPoint = new Point(0, 0);
            for (int i = 0; i < howMany; i++) {
                var rect = new Rectangle();
                rect.X = currentPoint.X;
                rect.Y = currentPoint.Y;
                rect.Width = windowSize.Width;
                rect.Height = windowSize.Height;
                currentPoint.X += windowSize.Width;
                if (currentPoint.X + windowSize.Width > screen.Width) {
                    //move to next row of screen
                    if (currentPoint.Y + windowSize.Height <= screen.Height)
                        currentPoint = new Point(0, currentPoint.Y + windowSize.Height);
                    else currentPoint = new Point(0, 0);
                }
                result.Add(rect);
            }
            return result;
        }

        public static DateTime[] GetDateTimes(IEnumerable<DateTime> dates, TimeSpan duration) {
            duration = TimeSpan.FromMilliseconds(duration.Milliseconds + 1000); //ALWAYS + 1 second
            return dates
                .Where(x => x.Add(duration) >= DateTime.Now)
                .ToArray();
        }
    }
}
