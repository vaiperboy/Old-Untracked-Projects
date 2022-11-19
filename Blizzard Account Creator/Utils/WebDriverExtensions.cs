using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Drawing;
using System.IO;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Blizzard_Account_Creator.Exceptions;

namespace Blizzard_Account_Creator {
    public static class WebDriverExtensions {
        public static bool ElementExists(this IWebDriver driver, By by) {
            try {
                var elm = driver.FindElement(by);
                return elm.Displayed;
            }
            catch (NoSuchElementException) {
                return false;
            }
        }
        private static Dictionary<string, int> CountriesIndex = new Dictionary<string, int>() {
            { "kaz", 112},
            { "arg", 10},
            {"deu", 81 }
        };
        public static async Task<bool> FillRegistrationForm(this IWebDriver driver, Account account, bool startFromEmail = true) {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("var elms = document.getElementsByTagName('option');");
            try {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(40));
                if (startFromEmail) {
                    wait.Until(ExpectedConditions.ElementExists(By.Id("dob-field-inactive")));
                    addClass(js, "dob-field-inactive", "phantom");
                    removeClass(js, "dob-field-active", "phantom");
                }
                foreach (var keyValue in account.AsHeaders()) {
                    switch (keyValue.Key) {
                        case "country": {
                                if (!startFromEmail) break;
                                var countryId = "capture-country";
                                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(countryId)));
                                setIndex(js, countryId, CountriesIndex[account.Country.ToLower()]);
                                await Task.Delay(500);
                                break;
                            }
                        case "dobMonth": {
                                if (!startFromEmail) break;

                                string className = "step__input--date--mm";
                                wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName(className)));
                                driver.FindElement(By.ClassName(className)).SendKeys(keyValue.Value);
                                await Task.Delay(500);

                                break;
                            }
                        case "dobDay": {
                                if (!startFromEmail) break;

                                string className = "step__input--date--dd";
                                wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName(className)));
                                driver.FindElement(By.ClassName(className)).SendKeys(keyValue.Value);
                                await Task.Delay(500);

                                break;
                            }
                        case "dobYear": {
                                if (!startFromEmail) break;

                                string className = "step__input--date--yyyy";
                                wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName(className)));
                                driver.FindElement(By.ClassName(className)).SendKeys(keyValue.Value);
                                submitButton(driver, wait);
                                if (startFromEmail) {
                                    await Task.Delay(5000);
                                    while (!driver.ElementExists(By.CssSelector(".show, #capture-first-name"))) {
                                        submitButton(driver, wait);
                                        await Task.Delay(5000);
                                    }
                                }
                                await Task.Delay(500);
                                if (startFromEmail) return true;
                                break;
                            }
                        case "capture-email": {
                                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(keyValue.Key)));
                                driver.FindElement(By.Id(keyValue.Key)).SendKeys(keyValue.Value);
                                submitButton(driver, wait);
                                await Task.Delay(1500);
                                var checkClass = "step__checkbox";
                                //wait.Until(ExpectedConditions.ElementIsVisible(By.Id("legal-checkboxes")));
                                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("#legal-checkboxes, .step__field-errors-item")));
                                if (driver.ElementExists(By.ClassName("step__field-errors-item")))
                                    throw new EmailUsedException(keyValue.Value);
                                js.ExecuteScript($"document.getElementsByClassName('{checkClass}')[1].checked = true");
                                submitButton(driver, wait);
                                await Task.Delay(500);

                                break;
                            }
                        default: {
                                wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(keyValue.Key)));
                                driver.FindElement(By.Id(keyValue.Key)).SendKeys(keyValue.Value);
                                if (driver.ElementExists(By.ClassName("accept-cookies-button")))
                                    driver.FindElement(By.ClassName("accept-cookies-button")).Click();
                                if (keyValue.Key.Contains("last")) submitButton(driver, wait);
                                break;
                            }
                            
                    }
                }
            } catch (WebDriverTimeoutException) {
                return false;
            }
            return true;
        }

        public static void submitButton(IWebDriver driver, WebDriverWait wait) {
            try {
                wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("flow-form-submit-btn")));
                driver.FindElement(By.Id("flow-form-submit-btn")).Click();
            }
            catch (ElementClickInterceptedException) {
                driver.FindElement(By.ClassName("accept-cookies-button")).Click();
                driver.FindElement(By.Id("flow-form-submit-btn")).Click();
            }
        }

        //lambda will make my life easier but fuck it
        public static void setIndex(IJavaScriptExecutor js, string id, int idx)
            => js.ExecuteScript($"document.getElementById('{id}').selectedIndex = {idx};");

        private static void addClass(IJavaScriptExecutor js, string id, string className)
            => js.ExecuteScript($"document.getElementById('{id}').classList.add('{className}')");

        private static void removeClass(IJavaScriptExecutor js, string id, string className)
           => js.ExecuteScript($"document.getElementById('{id}').classList.remove('{className}')");

        public static void FillPhoneFormAndSubmit(this IWebDriver driver, Responses.GeneratePhoneResponse phone, WebDriverWait wait) {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var phoneBox = driver.FindElement(By.Id("phone"));
            phoneBox.SendKeys(phone.Phone);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("sms-verification-submit")));
            js.ExecuteScript("document.getElementById('sms-verification-submit').click()");
        }

        public static void FillVerificationFormAndSubmit(this IWebDriver driver, Responses.ActivationCodeResponse phone, WebDriverWait wait) {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var phoneBox = driver.FindElement(By.Id("code"));
            phoneBox.SendKeys(phone.Code);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("sms-protection-submit")));
            driver.FindElement(By.Id("sms-protection-submit")).Click();
            //js.ExecuteScript("document.getElementById('sms-protection-submit').click()");
        }

        public static Bitmap GetElementScreenShot(this IWebDriver driver, IWebElement element) {
            Screenshot sc = ((ITakesScreenshot)driver).GetScreenshot();
            var img = Image.FromStream(new MemoryStream(sc.AsByteArray)) as Bitmap;
            return img.Clone(new Rectangle(element.Location, element.Size), img.PixelFormat);
        }

        public static void SwitchToInnerMostFrame(this IWebDriver driver) {
            try {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
                while (driver.FindElements(By.TagName("iframe")) != default) {
                    wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("iframe")));
                    driver.SwitchTo().Frame(driver.FindElement(By.TagName("iframe")));
                }
            } catch (WebDriverTimeoutException) {

            }
        }

        public static void SwitchToIFrame(this IWebDriver driver, WebDriverWait wait, int index) {
            driver.SwitchTo().DefaultContent();
            for (int i = 0; i < index; i++) {
                wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("iframe")));
                driver.SwitchTo().Frame(driver.FindElement(By.TagName("iframe")));
            }
        }

        public static void ClickElement(this IWebDriver driver, Point point) {
            ((IJavaScriptExecutor)driver).ExecuteScript($"document.elementFromPoint({point.X}, {point.Y}).click();");
        }
    }
}
