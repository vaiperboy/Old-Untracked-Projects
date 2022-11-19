using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ikout_assister {
    public partial class MainForm : Form {
        private static List<Card> cards = new List<Card>();
        private static List<Round> rounds = new List<Round>();
        private readonly FormOverlay overlay = new FormOverlay();
        private KeyboardHook keyboardHook;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private readonly static string parentPath = @"./Cards",
            suitsPath = parentPath + "/isolated/suits",
            ranksPath = parentPath + "/isolated/ranks",
            hokamsPath = parentPath + "/isolated/hokam",
            loggingPath = parentPath + "/logging";
        private static DateTime startedTime;
        public MainForm() {
            InitializeComponent();
        }

        private void listChanged(object sender, NotifyCollectionChangedEventArgs args) {
            // list changed
        }

        private void Stop() {
            output.Invoke((MethodInvoker)(() => {
                overlay.Hide();
                output.Clear();
                rounds.Clear();
            }));
            
            //keyboardHook.Dispose();
        }

        private async Task Start() {
            displayCardCount.Enabled = displayCardList.Enabled = true;
            loadCards();
            if (cards.Count() > 0) {
                startedTime = DateTime.Now;
                cancellationToken = new CancellationTokenSource();
                cancellationToken.Token.Register(() => Stop());

                output.Invoke((MethodInvoker)(() => {
                    overlay.Show();
                }));
                overlay.UpdateValues(cards, rounds, displayCardCount.Checked, displayCardList.Checked);
                setHook(cancellationToken.Token);

            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            this.TopMost = true;
            keyboardHook = new KeyboardHook();
            keyboardHook.KeyboardPressed += OnKeyPressed;
            loadCards();
            cancellationToken.Token.Register(() => Stop());

        }

        private async void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e) {
            if (e.KeyboardState == KeyboardHook.KeyboardState.KeyDown) {
                if (e.KeyboardData.VirtualCode == 107) {
                    if (cancellationToken.IsCancellationRequested) {
                        await Start();
                        mainButton.Text = "Stop Cheat";
                    } else {
                        cancellationToken.Cancel(false);
                        mainButton.Text = "Start Cheat";
                    }
                }
            }
        }

        private void loadCards() {
            cards = CardManager.PopulateCards(4);
        }

        private async void mainButton_Click(object sender, EventArgs e) {
            

            if (mainButton.Text.Contains("Start")) {
                var isNoxRunning = Process.GetProcessesByName("Nox").Any();
                if (isNoxRunning) {
                    await Start();
                    mainButton.Text = "Stop Cheat";
                } else {
                    MessageBox.Show("Make sure NoxPlayer is running", "Process not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } else {
                cancellationToken.Cancel(false);
                mainButton.Text = "Start Cheat";
                
            }
        }



        private void displayCardCount_CheckedChanged(object sender, EventArgs e) {
            overlay.UpdateValues(cards, rounds, displayCardCount.Checked, displayCardList.Checked);

        }

        private void displayCardList_CheckedChanged(object sender, EventArgs e) {
            overlay.UpdateValues(cards, rounds, displayCardCount.Checked, displayCardList.Checked);

        }



        private async Task setHook(CancellationToken cancelToken) {
            await Task.Run(async () => {
                while (true) {
                    if (cancelToken.IsCancellationRequested)
                        return;
                    var snap = new Image<Bgr, byte>(Helper.TakeSnap(Settings.TABLE_LOCATION_WITHOUT_PLAYER0));
                    var cardsConts = Imaging.ExtractCards(snap.PreProcessImage());

                    for (int i = 0; i < cardsConts.Size; i++) {
                        var rotatedRect = CvInvoke.MinAreaRect(cardsConts[i]);
                        if (rounds.Count > 0) {
                            /////// dont check current card if already checked before
                            if (rounds.Last().Player.Any(x => Helper.RectanlgesInteresct(x.Location, rotatedRect)))
                                continue;
                        }
                        var rect = rotatedRect.MinAreaRect();
                        var img = Imaging.FourPointsTransform(snap, cardsConts[i]);
                        var card = await CardManager.MatchCard(img, suitsPath, ranksPath, cards.Any(x => x.Face == Faces.Joker));
                        if (!string.IsNullOrEmpty(card.Name)) { 
                            if (cards.Any(x => (x.Value == card.Value) && (x.Face == card.Face))) {//card not found before
                                output.Invoke((MethodInvoker)(() => {
                                    output.AppendText($"Card {{{card.ToString()}}} [{card.TrueSuitName}] was put down ({card.SuitSimilarity})\n");
                                    output.ScrollToCaret();
                                }));

                                if (!File.Exists(startedTime.ToString())) {
                                    var path = $@"{loggingPath}/{startedTime.ToShortTimeString().Replace(":", ".")}";
                                    Directory.CreateDirectory(path);
                                }
                                img.Bitmap.Save($@"{parentPath}/logging/{startedTime.ToShortTimeString().Replace(":", ".")}/{card.ToString()}-{card.TrueSuitName}.png", ImageFormat.Png);
                                CardManager.RemoveCard(cards, card);
                                
                                if (rounds.Count == 0) {
                                    rounds.Add(new Round(new Player[4]));
                                    var bigSnap = new Image<Bgr, byte>(Helper.TakeSnap(Screen.PrimaryScreen.Bounds));
                                    var hokam = await CardManager.GetHokam(bigSnap, hokamsPath);
                                    rounds.Last().Hokam = hokam;
                                }
                                Player player = new Player {
                                    Card = card,
                                    TimeThrown = DateTime.Now,
                                    Location = rotatedRect
                                };
                                rounds.Last().UpdatePlayer(player);
                                overlay.UpdateValues(cards, rounds, displayCardCount.Checked, displayCardList.Checked);
                                if (rounds.Last().IsFinished) { //round finished
                                    rounds.Add(new Round(new Player[4]));
                                    output.Invoke((MethodInvoker)(() => {
                                        output.AppendText("-----------------------------------------------------\n");
                                    }));
                                    await Task.Delay(3000);
                                }
                            }
                        } else { //card not identified
                            //img.Bitmap.Save($@"{loggingPath}/unidentified/{new Random().Next(999)}.png", ImageFormat.Png);
                        }
                    }

                    if (cards.Count <= 4) {
                        Stop();
                        _ = Start();
                        await Task.Delay(7000);
                    }
                    await Task.Delay(30);
                }
            }, cancelToken);
            
        }
    }
}
