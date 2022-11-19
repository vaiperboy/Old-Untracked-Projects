using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ikout_assister {
    public partial class FormOverlay : Form {
        #region overlaying
        RECT rect;
        public const string windowName = "Nox";
        readonly IntPtr handle = FindWindow(null, windowName);

        public struct RECT {
            public int left, top, right, bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        #endregion


        string indexToRank(int num) {
            switch (num) {
                case 0:
                    return "hearts";
                case 1:
                    return "spades";
                case 2:
                    return "diamonds";
                case 3:
                    return "clubs";
                default:
                    return string.Empty;
            }
        }
        public FormOverlay() {
            InitializeComponent();

            Bitmap[] ranks = {
                Properties.Resources.Hearts,
                Properties.Resources.Spades,
                Properties.Resources.Diamond,
                Properties.Resources.Clubs
            };

            int width = 50, height = 50;
            for (int player = 0; player < Settings.Locations.Players.Length; player++) { //loop through all players' points
                for (int card = 0; card < 4; card++) { //create 4 ranks pics
                    PictureBox element = new PictureBox {
                        Image = ranks[card],
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Name = $"{(player+1).ToString()}_{indexToRank(card)}"
                    };
                    var location = Settings.Locations.Players[player].PointForScreen();
                    element.Location = new Point(location.X + (width * card), location.Y);
                    element.Size = (new Size(width, height)).SizeForScreen();
                    //element.Name = ranks[card 
                    this.Controls.Add(element);
                }
            }
        }

        private void FormOverlay_Load(object sender, EventArgs e) {
            this.BackColor = Color.Wheat;
            this.TransparencyKey = Color.Wheat;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            GetWindowRect(handle, out rect);
            this.Size = new Size(rect.right - rect.left, rect.bottom - rect.top);

            
        }

        private Label CreateLabel(string name, Size size) {
            return new Label {
                Name = name,
                Size = size, // 42 46 best size
                Font = new Font(spadesNumber.Font, FontStyle.Bold),
                ForeColor = "HD".Contains(name.ElementAt(name.Count() - 1)) ? Color.Red : Color.Black,
                Text = name.Substring(0, name.Length)
            };
        }

        private void UpdateList(List<Card> cards) {
            spadesNumber.Invoke((MethodInvoker)(() => {
                jrList.Visible = cards.Any(x => x.ToString() == "JR");
                jbList.Visible = cards.Any(x => x.ToString() == "JB");

                List<Card> hearts = cards
                .Where(x => x.Face == Faces.Hearts)
                .OrderByDescending(x => x.Value)
                .ToList();
            Control panel = heartsPanel;
            if (hearts.Count != panel.Controls.Count) {
                panel.Controls.Clear();
                for (int i = 0; i < hearts.Count; i++) {
                    Size size = new Size(42, 46);
                    int x = panel.Controls.OfType<Label>()
                        .Select(l => l.Bounds.Right)
                        .LastOrDefault();
                    int y = size.Height;
                    
                    var label = CreateLabel(hearts[i].ToString(), size);
                    label.Location = new Point(x, y);
                    panel.Controls.Add(label);
                }
            }

            List<Card> spades = cards
                .Where(x => x.Face == Faces.Spades)
                .OrderByDescending(x => x.Value)
                .ToList();
            panel = spadesPanel;
            if (spades.Count != panel.Controls.Count) {
                panel.Controls.Clear();
                for (int i = 0; i < spades.Count; i++) {
                    Size size = new Size(42, 46);
                    int x = panel.Controls.OfType<Label>()
                        .Select(l => l.Bounds.Right)
                        .LastOrDefault();
                    int y = size.Height;

                    var label = CreateLabel(spades[i].ToString(), size);
                    label.Location = new Point(x, y);
                    panel.Controls.Add(label);
                }
            }

            List<Card> clubs = cards
                .Where(x => x.Face == Faces.Clubs)
                .OrderByDescending(x => x.Value)
                .ToList();
            panel = clubsPanel;
            if (clubs.Count != panel.Controls.Count) {
                panel.Controls.Clear();
                for (int i = 0; i < clubs.Count; i++) {
                    Size size = new Size(42, 46);
                    int x = panel.Controls.OfType<Label>()
                        .Select(l => l.Bounds.Right)
                        .LastOrDefault();
                    int y = size.Height;

                    var label = CreateLabel(clubs[i].ToString(), size);
                    label.Location = new Point(x, y);
                    panel.Controls.Add(label);
                }
            }

            List<Card> diamonds = cards
                .Where(x => x.Face == Faces.Diamonds)
                .OrderByDescending(x => x.Value)
                .ToList();
            panel = diamondsPanel;
            if (diamonds.Count != panel.Controls.Count) {
                panel.Controls.Clear();
                for (int i = 0; i < diamonds.Count; i++) {
                    Size size = new Size(42, 46);
                    int x = panel.Controls.OfType<Label>()
                        .Select(l => l.Bounds.Right)
                        .LastOrDefault();
                    int y = size.Height;

                    var label = CreateLabel(diamonds[i].ToString(), size);
                    label.Location = new Point(x, y);
                    panel.Controls.Add(label);
                }
            }
            }));
        }

        public void UpdateValues(List<Card> cards, List<Round> rounds, bool displayCardCount = true, bool displayCardList = true, bool displayCardsLeft = true) {
            cardListPanel.Invoke((MethodInvoker)(() => {
                cardListPanel.Visible = displayCardList;
                cardsCountPanel.Visible = displayCardCount;
            }));
            if (displayCardsLeft) {
                //BOTTOM RIGHT TOP LEFT
                if (rounds.Count > 0) {
                    if (rounds.Last().IsFinished) {
                        var rootCard = rounds.Last().RootCard;
                        if (rootCard.Face != Faces.Joker) {
                            Dictionary<int, Faces?> dict = new Dictionary<int, Faces?>();
                            for (int i = 1; i < rounds.Last().Player.Length; i++) {
                                if (rounds.Last().Player[i].Card.Face != Faces.Joker &&
                                    rounds.Last().Player[i].Card.Face != rounds.Last().RootCard.Face) {
                                    dict.Add(i, rounds.Last().Player[i].Card.Face);
                                }
                            }

                            for (int i = 0; i < dict.Count; i++) {
                                string key = $"{dict.Keys.ElementAt(i).ToString()}_{rootCard.Face.ToString().ToLower()}";
                                var elm = this.Controls.Find(key, true).First();
                                elm.Invoke((MethodInvoker)(() => {
                                    elm.Visible = false;
                                }));
                            }
                        }
                    }
                } else { //match finished 
                    for (int i = 1; i <= 3; i++) {
                        for (int j = 0; j < 4; j++) {
                            var elm = this.Controls.Find($"{i.ToString()}_{indexToRank(j)}", true).FirstOrDefault();
                            if (elm != default)
                                elm.Invoke((MethodInvoker)(() => {
                                    elm.Visible = true;
                                }));
                        }
                    }
                }
            }
            if (displayCardList) UpdateList(cards);
            if (displayCardCount) {
                int spades = cards.Count(x => x.Face == Faces.Spades);
                int clubs = cards.Count(x => x.Face == Faces.Clubs);
                int hearts = cards.Count(x => x.Face == Faces.Hearts);
                int diamonds = cards.Count(x => x.Face == Faces.Diamonds);

                cardsCountPanel.Invoke((MethodInvoker)(() => {
                    spadesNumber.Text = spades.ToString();
                    clubsNumber.Text = clubs.ToString();
                    heartsNumber.Text = hearts.ToString();
                    diamondsNumber.Text = diamonds.ToString();
                }));
            }
        }
    }
}
