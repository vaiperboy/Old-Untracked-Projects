using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Blizzard_Account_Creator.Utils;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Console = Colorful.Console;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Net;
using System.Text;
using OpenQA.Selenium.Chrome;

namespace Blizzard_Account_Creator {
    class AccountFactory {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly _2Captcha._2Captcha captcha;
        private PhoneFactory phoneFactory;
        private readonly ChromeDriver driver;
        //private readonly EdgeDriver driver;
        private readonly ProxyV2 proxy;
        private string extensionPath;
        private RectangleV2 rect;
        private int Process_Id;
        bool isUsingCaptchaSolver;
        public AccountFactory(_2Captcha._2Captcha captcha, PhoneFactory phone,
            ProxyV2 proxy = default, RectangleV2 placement = default, bool loadCaptchaSolver = false) {
            try {
                this.proxy = proxy;
                this.captcha = captcha;
                this.phoneFactory = phone;
                
                ChromeOptions options = new ChromeOptions();
                //EdgeOptions options = new EdgeOptions();
                if (proxy != default) {
                    if (!string.IsNullOrEmpty(proxy.Username)) { //auth
                        extensionPath = ProxyManager.AddProxy(options, proxy, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions"));
                    } else {
                        options.AddArgument($"--proxy-server=http://{proxy.Address}:{proxy.Port}");
                        //options.AddArgument($"--proxy-server=socks5://{proxy.Address}:{proxy.Port}");
                    }
                    //options.AddArgument("ignore-certificate-errors");
                    logger.Debug("Starting browser with proxy of " + proxy.ToString());
                }
                var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "webdrivers");
                var chromeDriverService = ChromeDriverService.CreateDefaultService(location);
                //var chromeDriverService = Microsoft.Edge.SeleniumTools.EdgeDriverService.CreateDefaultService(location);
                chromeDriverService.HideCommandPromptWindow = true;
                chromeDriverService.SuppressInitialDiagnosticInformation = true;
                options.AddArgument("--silent");
                options.AddArgument("log-level=3");
                if (placement != default) {
                    options.AddArgument($"--window-size={placement.Size.Width},{placement.Size.Height}");
                    options.AddArgument($"--window-position={placement.Location.X},{placement.Location.Y}");
                    placement.isBeingUsed = true;
                    this.rect = placement;
                }

                //detection START
                options.AddArgument("start-maximized");
                options.AddArgument("no-sandbox");
                options.AddAdditionalCapability("useAutomationExtension", false);
                options.AddArgument("--disable-blink-features");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                //options.AddExcludedArgument("enable-automation");
                options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.99 Safari/537.36");
                var parameters = new Dictionary<string, object> {
                    ["source"] = "Object.defineProperty(navigator, 'webdriver', { get: () => undefined })"
                };

                //detection STOP

                this.isUsingCaptchaSolver = loadCaptchaSolver;
                if (loadCaptchaSolver)
                    CaptchaFactory.AddCaptchaExtension(options, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "captcha solver extension"));

                this.Process_Id = chromeDriverService.ProcessId;
                this.driver = new ChromeDriver(chromeDriverService, options);
                CodePagesEncodingProvider.Instance.GetEncoding(437);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                //this.driver = new EdgeDriver(chromeDriverService, options, TimeSpan.FromSeconds(25));
                driver.ExecuteChromeCommand("Page.addScriptToEvaluateOnNewDocument", parameters);
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                string title = (string)js.ExecuteScript($"document.title = '{this.proxy.Address}:{this.proxy.Port}'");
            } catch (WebDriverException ex) {
                Console.WriteLine($"Couldn't start browser::: {ex.FlattenException()}", Color.Red);
                Console.ReadKey();
            }
        }


        public async Task<Responses.RequestResult> TryCreateAccount(Account account, TimeSpan accountTimeOut = default) {
            //ERR_PROXY_CONNECTION_FAILED
            if (accountTimeOut == default) accountTimeOut = TimeSpan.FromMinutes(10);
            var url = new Uri("https://eu.battle.net/account/creation/en-us/?style=&country=KAZ");
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            string title = (string)js.ExecuteScript($"document.title = '{this.proxy.Address}:{this.proxy.Port}'");
            driver.Navigate().GoToUrl(url.ToString());
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            logger.Debug("Going to page url " + url.ToString());
            if (driver.ElementExists(By.ClassName("error-code"))) {
                if (driver.FindElement(By.ClassName("error-code")).Text.Contains("ERR_PROXY_CONNECTION_FAILED")) {
                    this.proxy.IsWorking = false;
                    return new Responses.RequestResult(false, "Proxy is bad...");
                }
                return await TryCreateAccount(account, accountTimeOut);
            }
            try {
                title = (string)js.ExecuteScript($"document.title = '{this.proxy.Address}:{this.proxy.Port}'");
                var form = By.Id("flow-form");
                wait.Until(ExpectedConditions.ElementExists(form));
              var filled = await driver.FillRegistrationForm(account);
                if (filled) {
                    CheckPageError();
                    if (driver.ElementExists(By.Id("error"))) {
                        return await TryCreateAccount(account, accountTimeOut);
                    }
                    var captchaWait = new WebDriverWait(driver, this.isUsingCaptchaSolver ? TimeSpan.FromSeconds(190) : accountTimeOut);
                    captchaWait.Until(ExpectedConditions.ElementExists(By.CssSelector(".show, #capture-first-name, #capture-password")));
                    if (!driver.ElementExists(By.Id("capture-first-name")) && isUsingCaptchaSolver) {
                        driver.SwitchTo().Frame(driver.FindElement(By.ClassName("show")));

                        wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("captcha-solver-info")));
                        await Task.Delay(300);
                        driver.FindElement(By.ClassName("captcha-solver-info")).Click();
                        Console.WriteLine("Trying to solve captcha via solver...", Color.Yellow);
                        captchaWait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("captcha-solver-info")));
                        Console.WriteLine("Just solved captcha..", Color.Green);
                        driver.SwitchTo().ParentFrame();
                    } else if (driver.ElementExists(By.ClassName("show"))) {
                        Console.WriteLine("Waiting on captcha input from user...", Color.Yellow);
                    }
                    filled = await driver.FillRegistrationForm(account, false);
                    if (!filled) return new Responses.RequestResult(false, "fucking form");
                    string passId = "capture-password";

                    captchaWait.Until(ExpectedConditions.ElementExists(By.Id(passId)));
                    await Task.Delay(1000);
                    driver.FindElement(By.Id(passId)).SendKeys(account.Password);
                    await Task.Delay(1213);
                    WebDriverExtensions.submitButton(driver, wait);
                    await Task.Delay(1000);
                    //for the username
                    WebDriverExtensions.submitButton(driver, wait);
                    wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".step__field-errors-item, #step-meta-data")));
                    while (driver.ElementExists(By.ClassName("step__field-errors-item"))) {
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("suggest-battletag-btn")));
                        driver.FindElement(By.Id("suggest-battletag-btn")).Click();
                        await Task.Delay(1000);
                        WebDriverExtensions.submitButton(driver, wait);
                        await Task.Delay(3000);
                    }
                    await Task.Delay(1000);

                    var _driver = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                    _driver.IgnoreExceptionTypes(typeof(WebDriverTimeoutException));
                    string boxId = "step-meta-data";
                    _driver.Until(ExpectedConditions.ElementExists(By.Id(boxId)));
                    await Task.Delay(2000);
                    if (!driver.FindElement(By.Id("step-meta-data")).GetAttribute("data-step-id").Equals("create-success", StringComparison.OrdinalIgnoreCase))
                        return new Responses.RequestResult(false, "Account didn't create", account);
                    await Task.Delay(5000);
                    return new Responses.RequestResult(true, "Account created", account);
                } else return new Responses.RequestResult(false, "couldnt fill form");
            }
            catch (WebDriverTimeoutException ex) {
                return new Responses.RequestResult(false, $"[{ex.ToString()}] Was not able to create account" +
                    $"in {accountTimeOut.TotalSeconds.ToString()} seconds");
            }
            catch (WebException ex) {
                return new Responses.RequestResult(false, ex.ToString());
            }
        }

        public async Task<Responses.RequestResult> InputSecurity(Account acc, int maxRetries = 2) {
            Console.WriteLine("Inputting sec answer...", Color.Yellow);
            string res = string.Empty;
                await Task.Delay(2000);
                res = driver.PageSource.ToString();
                //string href = driver.FindElements(By.TagName("a"))[3].GetAttribute("href");
                string href = (string)((IJavaScriptExecutor)driver).ExecuteScript("var elms = document.getElementsByTagName('a');for (var i = 0; i < elms.length; i++) {if (elms[i].href.includes('/ticket-login')) {return elms[i].href;}}return '';");
                if (string.IsNullOrEmpty(href))
                    return new Responses.RequestResult(false, "couldnt find sec");

                int retries = 0;
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(200));

            while (retries++ <= maxRetries) {
                try {
                    driver.Navigate().GoToUrl(href);
                    //wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("account-overview")));
                    await Task.Delay(10000);
                    var nameId = "accountName";
                    wait.Until(ExpectedConditions.ElementExists(By.CssSelector("#redeem-code, #"+nameId)));
                    if (driver.ElementExists(By.Id(nameId))) {
                        wait.Until(ExpectedConditions.ElementExists(By.Id(nameId)));
                        driver.FindElement(By.Id(nameId)).SendKeys(acc.Email);
                        var pwId = "password";
                        wait.Until(ExpectedConditions.ElementExists(By.Id(pwId)));
                        driver.FindElement(By.Id(pwId)).SendKeys(acc.Password);
                        var submitId = "submit";
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(submitId)));
                        driver.FindElement(By.Id(submitId)).Click();
                        wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id(submitId)));
                    }
                   

                    driver.Navigate().GoToUrl("https://account.blizzard.com/security");
                    wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".card-header-link")));
                    int _retries = 0;
                    while (_retries++ <= maxRetries) {
                        try {
                            await Task.Delay(10000);
                            ((IJavaScriptExecutor)driver).ExecuteScript("document.getElementsByClassName('card-header-link float-md-right')[2].click()");
                            break;
                        } catch (WebDriverException) {
                            continue;
                        }
                    }
                    string questionId = "question-select";
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(questionId)));
                    await Task.Delay(2000);
                    wait.Until(ExpectedConditions.ElementExists(By.TagName("option")));
                    await Task.Delay(2000);
                    driver.FindElement(By.Id(questionId)).SendKeys("What" + Keys.Enter);
                    //WebDriverExtensions.setIndex((IJavaScriptExecutor)driver, "question-select", 1);

                    string answerId = "answer";
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(answerId)));
                    await Task.Delay(500);
                    driver.FindElement(By.Id(answerId)).SendKeys(acc.RecoveryAnswer);
                    string clickId = "sqa-submit";
                    wait.Until(ExpectedConditions.ElementExists(By.Id(clickId)));
                    driver.FindElement(By.Id(clickId)).Click();
                    await Task.Delay(1200);
                    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id(clickId)));
                    return new Responses.RequestResult(true, "account put answer");
                }
                catch (WebDriverTimeoutException ex) {
                    logger.Error(res + ex.ToString());
                    driver.Navigate().Refresh();      
                }
            }
            return new Responses.RequestResult(false, "didnt put answer");
        }



        private void CheckPageError() {
            while (driver.ElementExists(By.ClassName("error-code"))) {
                if (driver.FindElement(By.ClassName("error-code")).Text.Contains("ERR_EMPTY_RESPONSE")) {
                    this.driver.Navigate().Refresh();
                }
            }
        }

        public async Task<Responses.VerificationResult> VerifyAccount (Account acc) {
            driver.Navigate().GoToUrl("https://account.blizzard.com/");
            if (driver.ElementExists(By.ClassName("error-code"))) return await this.VerifyAccount(acc);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            var miniWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            try {
                //wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("security-link")));
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".security-link, .server-error")));
                if (driver.ElementExists(By.ClassName("server-error"))) {
                    logger.Debug("server error... waiting couple of secs");
                    await Task.Delay(5000);
                    return await VerifyAccount(acc);
                }
                var elms = driver.FindElements(By.ClassName("security-link"));
                if (elms.Count < 2)
                    return Responses.VerificationResult.Empty;
                elms[elms.Count - 1].Click();
            } catch (WebDriverTimeoutException) {
                return await this.VerifyAccount(acc);
            }
            try {
                var form = By.TagName("form");
                //wait.Until(ExpectedConditions.ElementExists(form));
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("form, .server-error")));
                if (driver.ElementExists(By.ClassName("server-error"))) {
                    logger.Debug("server error... waiting couple of secs");
                    await Task.Delay(5000);
                    return await VerifyAccount(acc);
                }
                var formElm = driver.FindElement(form);
                if (formElm != default) {
                    logger.Debug("Getting phone #...");
                    var phone = await phoneFactory.GetPhoneNumber(acc.Country.ToLower().StartsWith("ka") ? "kz" : "argentina");
                    if (!string.IsNullOrEmpty(phone.Phone)) {
                        logger.Debug("Generated phone # " + phone.ToString());
                        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("phone")));
                        driver.FillPhoneFormAndSubmit(phone, wait);
                        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".alert-message, #code, .server-error")));
                        if (driver.ElementExists(By.ClassName("alert-message"))) {
                            logger.Debug("alert-message: " + driver.FindElement(By.ClassName("alert-message")).Text);
                            Console.WriteLine("Trying phone verification again..", Color.Yellow);
                            await phoneFactory.DisposeNumber(phone);
                            return await VerifyAccount(acc);
                        }else if (driver.ElementExists(By.ClassName("server-error"))) {
                            logger.Debug("server error... waiting couple of secs");
                            await phoneFactory.DisposeNumber(phone);
                            await Task.Delay(5000);
                            return await VerifyAccount(acc);
                        }
                        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("code")));
                        Console.WriteLine("Waiting on sms code...", Color.Yellow);
                        var code = await phoneFactory.GetActivationCode(phone);
                        if (code.isSuccess) {
                            driver.FillVerificationFormAndSubmit(code, wait);
                            await Task.Delay(5000);
                            if (driver.PageSource.Contains("Invalid verification code.")) {
                                await phoneFactory.DisposeNumber(phone);
                                return await VerifyAccount(acc);
                            } else if (driver.PageSource.Contains("Your Battle.net Account is locked.")) {
                                await phoneFactory.DisposeNumber(phone);
                                return new Responses.VerificationResult(false, "battle.net locked", phone.Phone, code);
                            }
                               
                            wait.Until(ExpectedConditions.TextToBePresentInElementLocated(By.TagName("div"), "phone number has been successfully"));
                            return new Responses.VerificationResult(true, "verified", phone.Phone, code);
                            
                        } else if (code.Timeout) {
                            Console.WriteLine("Trying again for code..", Color.Yellow);
                            await phoneFactory.DisposeNumber(phone);
                            return await this.VerifyAccount(acc);
                        }
                    } else {
                        logger.Error("Couldn't get phone #... Retrying...");
                        Console.WriteLine("Getting phone # again...", Color.Yellow);
                        return await this.VerifyAccount(acc);
                    }
                } else {
                    logger.Error("Couldn't go to url...");
                    return Responses.VerificationResult.Empty;
                }
                return Responses.VerificationResult.Empty;
            }
            catch (Exception) {
                return Responses.VerificationResult.Empty;
            }
        }

        private async Task<bool> PlayCaptcha(WebDriverWait wait) {
            driver.SwitchToIFrame(wait, 3);
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("var elm = document.getElementsByClassName('fancy_btn')[0]; elm.click();");
            await Task.Delay(1500);
            js.ExecuteScript("var elm = document.getElementsByClassName('fancy_btn')[0]; elm.click();");
            await Task.Delay(500);
            return true;
        }

        public virtual void Dispose() {
            this.driver.Close();
            this.driver.Quit();
            logger.Debug("Deleteing extenion folder " + extensionPath);
            Helper.DeleteIfExists(extensionPath);
        }


        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public void Minimize() {
            //this.driver.Manage().Window.Minimize();
            this.driver.Manage().Window.Position = new Point(-8000, 0);
            //var process = Process.GetProcessById(this.Process_Id);
            //if (process != default) {
            //    ShowWindow(process.MainWindowHandle, 2);
            //}
            rect.isBeingUsed = false;
        }
    }
}
