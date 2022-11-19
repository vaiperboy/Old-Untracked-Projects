using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Linq;

namespace ikout_assister {
    abstract class Helper {

        public static Dictionary<string, double> CompareImages(Image<Bgr, byte> img, string[] files) {
            var dict = new Dictionary<string, double>();
            var enhanced = img.Copy().Convert<Gray, byte>();
            foreach (var file in files) {
                var rankImg = new Image<Gray, byte>((Bitmap)Bitmap.FromFile(file));
                var rate = Imaging.GetSimilarityRate(enhanced, rankImg);
                dict.Add(Path.GetFileNameWithoutExtension(file), rate);
            }
            return dict
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value); ;
        }

        public static Dictionary<string, double> CompareImages(Image<Bgr, byte> img, string[] files, Gray threshold) {
            var dict = new Dictionary<string, double>();
            var enhanced = img.Copy().Convert<Gray, byte>().ThresholdBinary(threshold, new Gray(255));
            foreach (var file in files) {
                var rankImg = new Image<Gray, byte>((Bitmap)Bitmap.FromFile(file)).ThresholdBinary(new Gray(197), new Gray(255));
                var rate = Imaging.GetSimilarityRate(enhanced, rankImg);
                dict.Add(Path.GetFileNameWithoutExtension(file), rate);
            }
            return dict
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value); ;
        }

        public static bool RectanlgesInteresct(RotatedRect rect1, RotatedRect rect2) {
            if (rect1.Size == default || rect2.Size == default) return false; //stop wasting checks
            Mat intersected = new Mat();
            CvInvoke.RotatedRectangleIntersection(rect1, rect2, intersected);
            return !intersected.IsEmpty;
        }

        public static Bitmap TakeSnap(Point point, Size size) {
            point = point.PointForScreen();
            size = size.SizeForScreen();
            Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (Graphics graph = Graphics.FromImage(bitmap)) {
                graph.CopyFromScreen(point.X, point.Y, 0, 0, size, CopyPixelOperation.SourceCopy);
                return bitmap;
            }
        }

        public static Bitmap TakeSnap(Rectangle rect) {
            Rectangle enhancedRect = rect.RectangleForScreen();
            Bitmap bitmap = new Bitmap(enhancedRect.Width, enhancedRect.Height, PixelFormat.Format32bppArgb);
            using (Graphics graph = Graphics.FromImage(bitmap)) {
                graph.CopyFromScreen(enhancedRect.X, enhancedRect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
                return bitmap;
            }
        }

        public static Mat DrawRect(Mat input, RotatedRect rect, MCvScalar color = default(MCvScalar),
        int thickness = 1, LineType lineType = LineType.EightConnected, int shift = 0) {
            var v = rect.GetVertices();

            var prevPoint = v[0];
            var firstPoint = prevPoint;
            var nextPoint = prevPoint;
            var lastPoint = nextPoint;


            for (var i = 1; i < v.Length; i++) {
                nextPoint = v[i];
                CvInvoke.Line(input, Point.Round(prevPoint), Point.Round(nextPoint), color, thickness, lineType, shift);
                prevPoint = nextPoint;
                lastPoint = prevPoint;
            }
            CvInvoke.Line(input, Point.Round(lastPoint), Point.Round(firstPoint), color, thickness, lineType, shift);
            return input;
        }

        public static List<bool> GetHash(Bitmap bmpSource) {
            List<bool> lResult = new List<bool>();
            //create new image with 16x16 pixel
            //Bitmap bmpMin = new Bitmap(bmpSource, new Size(16, 16));
            var bmpMin = bmpSource;
            for (int j = 0; j < bmpMin.Height; j++) {
                for (int i = 0; i < bmpMin.Width; i++) {
                    //reduce colors to true / false                
                    lResult.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f);
                }
            }
            return lResult;
        }
    }
}
