using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace ikout_assister {
    public static class Imaging {


        public static int GetCornersCount(Image<Gray, byte> inputImage) {
            var conts = new VectorOfVectorOfPoint();
            int totalCorners = 0;
            CvInvoke.FindContours(inputImage, conts, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
            for (int i = 0; i < conts.Size; i++) {
                var length = CvInvoke.ArcLength(conts[i], true);
                VectorOfPoint corners = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(conts[i], corners, 0.01 * length, true);
                totalCorners += corners.Size;
            }
            return totalCorners;
        }

        public static int GetCornersCount(VectorOfPoint conts) {
            var length = CvInvoke.ArcLength(conts, true);
            VectorOfPoint corners = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(conts, corners, 0.04 * length, true);
            return corners.Size;
        }

        public static double GetSimilarityRate<TColor, TDepth>(Image<TColor, TDepth> _img1, Image<TColor, TDepth> _img2)
             where TColor : struct, IColor
             where TDepth : new() {
            //resizing to small to not ruin pixels
            Image<TColor, TDepth> img1 = _img1.Copy(),
                img2 = _img2.Copy();
            if (_img1.Height > _img2.Height ||
                _img1.Width > _img2.Width) {
                CvInvoke.Resize(_img1, img1, _img2.Size);
            } else if (_img2.Height > _img1.Height ||
                _img2.Width > _img1.Width) {
                CvInvoke.Resize(_img2, img2, _img1.Size);
            }
            var result = new Mat();
            CvInvoke.MatchTemplate(img1, img2, result, TemplateMatchingType.CcoeffNormed);
            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            return maxVal;
        }

        public static Image<Gray, byte> PreProcessImage(this Emgu.CV.Image<Bgr, byte> inputImage) {
            Image<Gray, byte> enhancedImage = inputImage.Copy().Convert<Gray, byte>();
            CvInvoke.Threshold(enhancedImage, enhancedImage, 210, 255, ThresholdType.Binary);
            CvInvoke.Threshold(enhancedImage, enhancedImage, 217, 217, ThresholdType.Otsu);
            CvInvoke.Canny(enhancedImage, enhancedImage, 0, 255);
            return enhancedImage;
        }


        public static Image<Gray, byte> PreProcessCard(this Emgu.CV.Image<Bgr, byte> inputImage) {
            Image<Gray, byte> enhancedImage = inputImage.Copy().Convert<Gray, byte>();
            CvInvoke.Threshold(enhancedImage, enhancedImage, 165, 255, ThresholdType.Binary);
            CvInvoke.Threshold(enhancedImage, enhancedImage, 217, 217, ThresholdType.Otsu);
            //CvInvoke.Canny(enhancedImage, enhancedImage, 0, 255);
            return enhancedImage;
        }

        public static Rectangle cardIsolater = new Rectangle(0, 0, 150, 154);

        public static Image<Bgr, byte>[] IsolateRankSuit(this Emgu.CV.Image<Bgr, byte> inputImage) {
            Image<Gray, byte> processed = inputImage.Convert<Gray, byte>()
                .ThresholdBinary(new Gray(155), new Gray(255));
            processed.ROI = cardIsolater;
            VectorOfVectorOfPoint conts = new VectorOfVectorOfPoint();
            int[,] hier = CvInvoke.FindContourTree(processed, conts, ChainApproxMethod.ChainApproxNone);
            Image<Bgr, byte>[] result = new Image<Bgr, byte>[2];
            Dictionary<RotatedRect, Image<Bgr, byte>> images = new Dictionary<RotatedRect, Image<Bgr, byte>>();
            for (int i = 0; i < conts.Size; i++) {
                if (hier[i, 3] != -1) { //exclude big ass contour
                    var rotatedRect = CvInvoke.MinAreaRect(conts[i]);
                    var rect = rotatedRect.MinAreaRect();
                    if (rect.Width > 10 && rect.Height > 20) {
                        inputImage.ROI = rect;
                        //if (rotatedRect.Angle == -45) result[0] = inputImage.Copy();
                        //else if (rotatedRect.Angle == 0) result[1] = inputImage.Copy();
                        images.Add(rotatedRect, inputImage.Copy());
                        inputImage.ROI = Rectangle.Empty;
                    }
                }
            }
            if (images.Count < 2) return result;
            var rankImg = images
                .OrderBy(x => x.Key.MinAreaRect().Y)
                .ThenByDescending(x => x.Key.Size.Width)
                .First()
                .Value;
            var suitImg = images
                .OrderBy(x => x.Key.MinAreaRect().Y)
                .ThenBy(x => x.Key.Size.Width)
                .Last()
                .Value;
            result[0] = suitImg;
            result[1] = rankImg;
            return result;
        }



        public static VectorOfVectorOfPoint ExtractCards(this Emgu.CV.Image<Gray, byte> enhancedImage) {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint(),
            output = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(enhancedImage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            for (int i = 0; i < contours.Size; i++) {
                var rect = CvInvoke.BoundingRectangle(contours[i]);
                int min, max;
                if (Screen.PrimaryScreen.Bounds.Width == 2560) {
                    min = 180;
                    max = 260;
                } else { min = 150; max = 200; }
                double minArea = 35000, maxArea = 50000;
                //if (rect.Width.inRange(min, max)) {
                if (CvInvoke.ContourArea(contours[i]).inRange(minArea, maxArea)) { 
                    var length = CvInvoke.ArcLength(contours[i], true);
                    VectorOfPoint corners = new VectorOfPoint();
                    CvInvoke.ApproxPolyDP(contours[i], corners, 0.04 * length, true);
                    if (corners.Size == 4)
                        output.Push(contours[i]);
                }
            }
            return output;
        }

        public static Image<TColor, TDepth> FourPointsTransform<TColor, TDepth>(Image<TColor, TDepth> frame, VectorOfPoint conts)
        where TColor : struct, IColor
        where TDepth : new() {
            var rotatedRect = CvInvoke.MinAreaRect(conts);
            var Max = new Size(300, 200);
            var srcVertices = rotatedRect.GetVertices().OrderVertices();
            var destVertices = new PointF[4] {
                    new PointF(0, 0), //TOP-LEFT
                    new PointF(Max.Width, 0), //TOP-RIGHT
                    new PointF(0, Max.Height), //BOTTOM-LEFT
                    new PointF(Max.Width, Max.Height) //BOTTOM-RIGHT
                };
            var card = new Image<TColor, TDepth>(Max.Width, Max.Height);
            var matrix = CvInvoke.GetPerspectiveTransform(srcVertices, destVertices);
            CvInvoke.WarpPerspective(frame, card, matrix, card.Size, warpType: Warp.FillOutliers);
            if (rotatedRect.Angle.inRange(-10, 0)) { //PLAYER 1
                CvInvoke.Flip(card, card, FlipType.Horizontal);
                //CvInvoke.Rotate(card, card, RotateFlags.Rotate180);

            }
            CvInvoke.Resize(card, card, Max, 1, 1);
            return card;
        }
    }
}
