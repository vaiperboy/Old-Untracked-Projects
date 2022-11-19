using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using OpenQA.Selenium.Support.UI;
using System;
using OpenQA.Selenium;
using System.Text;

namespace Blizzard_Account_Creator {
    public static class ExtensionMethods {

    private static Regex regex = new Regex(@"[0-9]{6}");
        public static byte[] ToBytes(this Bitmap image) {
            using (var ms = new MemoryStream()) {
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public static string FlattenException(this Exception exception) {
            var stringBuilder = new StringBuilder();

            while (exception != null) {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return stringBuilder.ToString();
        }

        public static Bitmap CropImage(this Bitmap image, Rectangle rect) {
            using (var bmpImage = new Bitmap(image)) {
                return bmpImage.Clone(rect, bmpImage.PixelFormat);
            }
        }

        public static Point CombinePoints(this Point point1, Point point2) {
            return new Point(point1.X + point2.X, point1.Y + point2.Y);
        }

        public static string ExtractCode(this string input) {
            Match match = regex.Match(input);
            return match.Success ? match.Value : string.Empty;
        }

        public static string ReplaceByKeyValuePair(this string input, Dictionary<string, string> dict) {
            foreach (var item in dict) {
                input = input.Replace(item.Key, item.Value);
            } return input;
        }

        public static bool UntilEither(this IWebDriver driver, By elm1, By elm2) {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(condition => {
                try {
                    return driver.FindElement(elm1).Displayed || driver.FindElement(elm2).Displayed;
                }
                catch (StaleElementReferenceException) {
                    return false;
                }
                catch (NoSuchElementException) {
                    return false;
                }
            });
            return false;
        }

        public static Point Point(this Rectangle rect)
            => new Point(rect.X, rect.Y);
    }
}
