 using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using Emgu.CV.Util;
using System.Windows.Forms;

namespace ikout_assister {
    public static class ExtensionMethods {

        public static Rectangle RectangleForScreen(this Rectangle rect) {
            Rectangle result = rect;
            result.Location = PointForScreen(rect.Point());
            result.Size = SizeForScreen(rect.Size);
            return result;
        }

        public static Size SizeForScreen(this Size size, Size sourceSize = default, Size destSize = default) {
            if (sourceSize == default)
                sourceSize = Settings.Sizes.SCREEN_WORKING_ON_DIMENSIONS;
            if (destSize == default)
                destSize = Screen.PrimaryScreen.Bounds.Size;
            Size resultSize = new Size {
                Width = (int)Math.Floor((double)destSize.Width / sourceSize.Width * size.Width),
                Height = (int)Math.Floor((double)destSize.Height / sourceSize.Height * size.Height)
            };
            return resultSize;
        }

        public static Point PointForScreen(this Point point, Size from = default, Size to = default) {
            if (to == default)
                to = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            if (from == default)
                from = Settings.Sizes.SCREEN_WORKING_ON_DIMENSIONS;
            return new Point((point.X * to.Width) / from.Width, (point.Y * to.Height) / from.Height);
        }
        public static PointF[] OrderVertices(this PointF[] input) {
            //TOP-LEFT TOP-RIGHT BOTTOM-LEFT BOTTOM-RIGHT
            var ordered = input
                .OrderBy(x => x.Y)
                .ToArray();
            return ordered;
        }

        public static Point Center(this Rectangle rect) {
            return new Point(rect.Left + rect.Width / 2,
                             rect.Top + rect.Height / 2);
        }

        public static VectorOfVectorOfPoint filterOutVectorsofVectors(this VectorOfVectorOfPoint contours, int minWidth, int maxWidth, int minHeight, int maxHeight) {
            VectorOfVectorOfPoint outputVector = new VectorOfVectorOfPoint();
            for (int i = 0; i < contours.Size; i++) {
                var boundingRect = CvInvoke.BoundingRectangle(contours[i]);
                if (boundingRect.Width.inRange(minWidth, maxWidth) &&
                    boundingRect.Height.inRange(minHeight, maxHeight)) {
                    outputVector.Push(contours[i]);
                }
            }
            return outputVector;
        }

        public static Bitmap AddImages(this Bitmap[] source) {
            var bitmap = new Bitmap(source.Sum(x => x.Width), source.Sum(x => x.Height));
            var point = new Point(0, 0);
            using (var g = Graphics.FromImage(bitmap)) {
               for (int i = 0; i < source.Length; i++) {
                    g.DrawImage(source[i], point);
                    point.X += source[i].Width+1;
                    //point.Y += source[i].Height+1;
                }
            }
            return bitmap;
        }

        public static Bitmap CropImage(this Bitmap source, Rectangle section) {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap)) {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }

        public static Image CropImage(this Image source, Rectangle section) {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap)) {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }

        public static bool inRange(this int input, int min, int max) {
            return input >= min && input < max;
        }

        public static bool inRange(this float input, int min, int max) {
            return input >= min && input < max;
        }

        public static bool inRange(this double input, double min, double max) {
            return input >= min && input < max;
        }

        public static byte[] Bytes(this Bitmap img) {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static Point Point(this Rectangle rect) {
            return new Point(
                (int)rect.X,
                (int)rect.Y);
        }


        public static Bitmap Shape(this Bitmap bitmap, Rectangle rect) {
            Bitmap target = new Bitmap(bitmap, rect.Width, rect.Height);
            using (Graphics g = Graphics.FromImage(bitmap)) {
                g.DrawImage(bitmap, new Rectangle(10, 80, target.Width, target.Height),
                                 rect,
                                 GraphicsUnit.Pixel);
            }
            return target;
        }
    }
}
