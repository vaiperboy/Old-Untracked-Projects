using System;
using Console = Colorful.Console;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using Blizzard_Account_Creator.Utils;
using IniParser;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;
using NLog;
using Blizzard_Account_Creator.Exceptions;

namespace Blizzard_Account_Creator {
    class Program {
        private static ConcurrentQueue<Account> accsQueue = new ConcurrentQueue<Account>();
        private static RectanglesList ScreenPlacements;
        private static void loadLogger() {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            //config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }
        static async Task Main(string[] args) {
            loadLogger();
            //i'll work on this form later..
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            List<string> names = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "names.txt")).ToList();
            var config = Path.Combine(basePath, "config.ini");
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(config);
            string password = !bool.Parse(data["main"]["randomPasswords"]) ? data["main"]["password"] : string.Empty;
            var proxies = new Proxies();
            bool useProxies = bool.Parse(data["main"]["useProxy"]),
                verifyPhones = bool.Parse(data["main"]["verifyPhones"]);
            if (useProxies) {
                proxies = await ProxyManager.LoadProxies(Path.Combine(basePath, "proxies.txt"));
                Console.WriteLine($"Loaded {proxies.Count} proxies", Color.Green);
            } else Console.WriteLine("Not using proxies...", Color.Gold);
            bool shuffleProxies = bool.Parse(data["main"]["shuffleProxies"]);
            if (shuffleProxies) {
                proxies.Shuffle();
                Console.WriteLine("Shuffling proxies..", Color.Gold);
            }

            bool debugMode = bool.Parse(data["main"]["debugMode"]);

            //var _2captcha = new _2Captcha._2Captcha(data["captchas"]["2captcha"]);
            //var phoneFactory = new PhoneFactory(data["phones"]["getsms"], PhoneProviders.getsms, true);
            //var phoneFactory = new PhoneFactory(data["phones"]["smsActivate"], PhoneProviders.smsActivate, true);
            var phoneFactory = new PhoneFactory(data["phones"]["_5sim"], PhoneProviders._5sim, true);
            Console.WriteLine($"Using {phoneFactory.Provider} for SMS");
            if (phoneFactory.Balance == -1) {
                Console.WriteLine("Could not load phone API... check logs", Color.Red);
                return;
            }
            Console.WriteLine($"Loaded {phoneFactory.Provider} API. Balance: " + phoneFactory.Balance, Color.Green);
            bool loadCaptchaSolver = bool.Parse(data["main"]["loadCaptchaSolver"]);
            if (loadCaptchaSolver)
                Console.WriteLine("Using captcha solver...", Color.Gold);
            int toCreate = 0;
            do {
                Console.Write("(minimum 1) Accounts to create?>", Color.Yellow);
                int.TryParse(Console.ReadLine(), out toCreate);
            } while (toCreate < 1);
            var accs = new List<Account>();
            
            
            bool usingEmailsList = bool.Parse(data["main"]["useEmailsList"]);
            if (usingEmailsList) {
                AccountManager.InitAccs(accsQueue, names, Path.Combine(basePath, "emails.txt"), toCreate);
                //AccountManager.InitAccs(accsQueue, names, Path.Combine(basePath, "emails.txt"), toCreate, "ARG");
                //AccountManager.InitAccs(accsQueue, names, Path.Combine(basePath, "emails.txt"), toCreate, "DEU");
            }
            if (accsQueue.Count == 0) Console.WriteLine("Couldn't init accounts... check logs", Color.Red);

            string batchesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "batches"),
                nonVerifiedsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "non-phone verified batches");
            ReadWrite writer = new ReadWrite(batchesPath, nonVerifiedsPath, DateTime.Now);
            
            Random rand = new Random();
            int max_threads = int.Parse(data["main"]["maxThreads"]);
            if (max_threads > toCreate) max_threads = toCreate; //no point in ternary here
            bool enhancedMode = bool.Parse(data["main"]["enhancedMode"]);
            if (enhancedMode) {
                Console.WriteLine("Enhanced mode ON", Color.Gold);
                var split = data["main"]["sizeScreen"].Split('x');
                int width = int.Parse(split[0]), height = int.Parse(split[1]);
                var size = new Size(width, height);
                var rectList = Helper.DivideScreenHorizontally(size, max_threads);
                ScreenPlacements = new RectanglesList(rectList);
            }

            //
            System.Diagnostics.Process.Start("CMD.exe", "/C taskkill /F /IM chrome.exe /T");

            System.Diagnostics.Process.Start("CMD.exe", "/C taskkill /F /IM chromedriver.exe /T");
            await Task.Delay(500);
            //
            using (SemaphoreSlim semaphore = new SemaphoreSlim(max_threads, max_threads + 4)) {
                var tasks = new ConcurrentDictionary<int, Task>();
                int counter = 1;
                while (accsQueue.Count > 0) {
                    await semaphore.WaitAsync();
                    if (accsQueue.TryDequeue(out Account account)) {
                        tasks.TryAdd(counter, Task.Run(async () => {
                            var proxy = useProxies ? proxies.PickBestProxy() : null;
                            var accFactory = new AccountFactory(null, phoneFactory,
                              proxy, enhancedMode ? ScreenPlacements.PickBestPlacement() : default, loadCaptchaSolver);

                            Console.WriteLine("\nCreating account", Color.Yellow);
                            try {
                                var acc = await accFactory.TryCreateAccount(account);
                                //semaphore.Release();
                                if (acc.isSuccess) {
                                    if (usingEmailsList) Helper.RemoveEmail(Path.Combine(basePath, "emails.txt"), acc.Account.Email);
                                    Console.WriteLine("Account created #" + counter, Color.Green);
                                    if (debugMode) accFactory.Minimize();

                                    var sec = await accFactory.InputSecurity(account);
                                    semaphore.Release();
                                    if (sec.isSuccess) {
                                        //account created
                                        acc.Account.DateCreated = DateTime.Now.ToString("MM/dd/yyyy");
                                        acc.Account.ProxyCreated = proxy;
                                        
                                        if (!verifyPhones) {
                                            accs.Add(acc.Account);
                                            await writer.WriteAccount(accs);
                                        }
                                        else {
                                            await Task.Delay(3000);
                                            //verifying phone
                                            Console.WriteLine("Started verification service..", Color.Yellow);
                                            try {
                                                var t2 = accFactory.VerifyAccount(acc.Account);
                                                if (t2.Result.isSuccess) {
                                                    acc.Account.Phone = t2.Result.PhoneNo;
                                                    accFactory.Dispose();
                                                    Console.WriteLine($"Verified number for account #{counter}", Color.Green);
                                                } else {
                                                    Console.WriteLine($"[ERROR] Couldn't verify phone for account #{counter}...error: {t2.Result.Message}", Color.Red);
                                                }
                                            } catch (Exception) {
                                                Console.WriteLine($"[EXCEPTION] Couldn't verify phone for account #{counter}...check logs", Color.Red);
                                            } finally {
                                                //write accounts
                                                
                                                accs.Add(acc.Account);
                                                await writer.WriteAccount(accs);
                                            }
                                        }
                                        
                                    } else {
                                        Console.WriteLine($"Couldn't put sec question.. check logs", Color.Red);
                                    }
                                } else {
                                    semaphore.Release();
                                    Console.WriteLine("Couldn't create account...check logs", Color.Red);

                                    //accsQueue.Enqueue(account); //probably stupid
                                }
                            }
                            catch (EmailUsedException ex) {
                                semaphore.Release();
                                Console.WriteLine("Email used...", Color.Red);
                                Helper.RemoveEmail(Path.Combine(basePath, "emails.txt"), ex.Email);
                            }
                            catch (AggregateException) {
                                semaphore.Release();
                                Console.WriteLine("[exx] Couldn't create account... check logs", Color.Red);
                            }
                            finally {
                                accFactory.Dispose();
                            }
                            
                            
                        }));
                    } else break; //reached max accs?
                    counter++;
                }
                try {
                    await Task.WhenAll(tasks.Values.ToArray());
                } catch (Exception) {

                }
                
                Console.WriteLine("\n\n");
                Console.WriteLine(String.Concat(Enumerable.Repeat("-", 30)), Color.Green);
                Console.WriteLine("All done boss...", Color.Green);
            }
            Console.ReadKey();
        }
    }
}
