using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;

namespace ikout_assister {
    public static class CardManager {
        public static async Task<Card> MatchCard(Image<Bgr, byte> image, string suitsPath, string ranksPath, bool searchForJoker = false) {
            var suits = Directory.GetFiles(suitsPath).Where(x => !x.ToLower().Contains('j')).ToArray();
            Dictionary<string, double> dict = new Dictionary<string, double>();
            var isolated = Imaging.IsolateRankSuit(image);
            Card card = new Card();
            if (searchForJoker) { //special case for jokers
                dict = Helper.CompareImages(image, Directory.GetFiles(suitsPath).Where(x => x.ToLower().Contains('j')).ToArray());
                if (dict.First().Value > .4) {
                    int blackPixelsCount = CvInvoke.CountNonZero(image.Copy().InRange(new Bgr(24, 28, 33), new Bgr(40, 39, 50)));
                    int redPixelsCount = CvInvoke.CountNonZero(image.Copy().InRange(new Bgr(0, 0, 216), new Bgr(255, 255, 216)));
                    string joker = blackPixelsCount > redPixelsCount ? "JB" : "JR";
                    return new Card(joker, Faces.Joker);
                }
            }
            if (isolated.All(x => x != default)) {
                dict = Helper.CompareImages(isolated[0], suits, new Gray(197));
                Faces matchingSuit = Faces.Spades;
                if (dict.First().Value < .7) return card; // exit if its less than minResult or if its not joker and no rank
                else matchingSuit = (Faces)dict.First().Key[0];
                var trueSuit = dict.First().Key;
                var suitSimilarity = dict.First().Value;
                var ranks = Directory.GetFiles(ranksPath);
                dict = Helper.CompareImages(isolated.Last(), ranks, new Gray(197));
                if (dict.First().Value < .63) return card; // exit if its less than minResult

                var matchingRank = dict.First().Key;

                card = new Card(matchingRank, matchingSuit) { SuitSimilarity = suitSimilarity, TrueSuitName = trueSuit };
            }
            return card;
        }

        public static async Task<Card> GetHokam(Image<Bgr, byte> inputImage, string hokamsPath) {
            var img = inputImage.Copy().Convert<Gray, byte>()
                .ThresholdBinary(new Gray(118), new Gray(255));
            var conts = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(img, conts, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
            var imgToDraw = inputImage.Copy();
            var card = new Card();
            for (int i = 0; i < conts.Size; i++) {
                var rotatedRect = CvInvoke.MinAreaRect(conts[i]);
                var rect = rotatedRect.MinAreaRect();
                if (Imaging.GetCornersCount(conts[i]) == 5
                    && rotatedRect.Angle == 0
                    && rect.Width.inRange(135, 250)
                    && rect.Height.inRange(90, 200)) {
                    img.ROI = rotatedRect.MinAreaRect();
                    var hokamBox = img.Copy();
                    img.ROI = Rectangle.Empty;
                    VectorOfVectorOfPoint hokamConts = new VectorOfVectorOfPoint();
                    CvInvoke.FindContours(hokamBox, hokamConts, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                    for (int j = 0; j < hokamConts.Size; j++) {
                        var hokamRotatedRect = CvInvoke.MinAreaRect(hokamConts[j]);
                        if (hokamRotatedRect.Angle == -45) {
                            var suits = Directory.GetFiles(hokamsPath);
                            Dictionary<string, double> dict = new Dictionary<string, double>();
                            hokamBox.ROI = hokamRotatedRect.MinAreaRect();
                            foreach (var suit in suits) {
                                var suitImg = new Image<Gray, byte>((Bitmap)Bitmap.FromFile(suit))
                                    .Convert<Gray, byte>()
                                    .ThresholdBinary(new Gray(197), new Gray(255));
                                var currentSuitImg = hokamBox.Copy(); //suit img
                                var rate = Imaging.GetSimilarityRate(currentSuitImg, suitImg);
                                dict.Add(Path.GetFileNameWithoutExtension(suit), rate);
                            }
                            dict = dict
                                .OrderByDescending(x => x.Value)
                                .ToDictionary(x => x.Key, x => x.Value); //first num is largest
                            return new Card(_face: (Faces)dict.First().Key[0]);
                        }
                    }
                }
            }
            return card;
        }

        public static void RemoveCard(List<Card> source, Card card) {
            for (int i =0; i < source.Count; i++) {
                if (source[i].Face == card.Face) {
                    if (source[i].Value == card.Value) source.RemoveAt(i);
                }
            }
        }

        public static List<Card> PopulateCards(int players = 4) {
            var cards = new List<Card>();
            if (players == 4) {
                for (int i = 6; i <= 14; i++) {
                    if (i > 6) { //clubs and diamonds
                        cards.Add(new Card(i, _face: Faces.Diamonds));
                        cards.Add(new Card(i, _face: Faces.Clubs));
                    }
                    cards.Add(new Card(i, _face: Faces.Hearts));
                    cards.Add(new Card(i, _face: Faces.Spades));
                }
                cards.Add(new Card("JB", Faces.Joker));
                cards.Add(new Card("JR", Faces.Joker));
            }
            return cards;
        }
    }
}
