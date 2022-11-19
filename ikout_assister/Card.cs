using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace ikout_assister {
    public enum Faces {
        Spades = 'S',
        Hearts = 'H',
        Diamonds = 'D',
        Clubs = 'C',
        Joker = 'J'
    }
    public class Card {
        public Faces? Face { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public double SuitSimilarity { get; set; }
        public string TrueSuitName { get; set; }
        public bool IsHokam { get; set; }

        private Dictionary<string, int> HighCards = new Dictionary<string, int>() {
            {"J", 11 },
            {"Q", 12 },
            {"K", 13 },
            {"A", 14 },
            {"JB", 15 },
            {"JR", 16 }
        };
        public Card(string _name = "", Faces? _face = default, bool _isHokam = default) {
            if (_name.Contains('x'))
                _name = _name.Replace("x", string.Empty);
            this.Name = _name;
            this.Face = _face;
            this.IsHokam = _isHokam;
            if (!string.IsNullOrEmpty(this.Name)) {
                if (!int.TryParse(_name, out int num)) {
                    if (_name.Length == 2 && _name[0] == 'J') {
                        this.Value = HighCards[_name.ToString()];
                        this.Face = Faces.Joker;
                    }
                    else this.Value = HighCards[_name[0].ToString()];
                } else this.Value = num;
                string[] splitted = { Name.Substring(0, Name.Length - 1), Name[Name.Length - 1].ToString() };
                if (_face == null) {
                    char lastChar = splitted[1][0];
                    if ("SHDC".Contains(lastChar)) {
                        this.Face = (Faces)lastChar;
                    }
                } else this.Face = _face;
            }
        }

        public Card(int _value , Faces? _face, bool _isHokam = default) {
            this.Value = _value;
            this.Face = _face;
            this.IsHokam = _isHokam;
            if (this.Value > 10) {
                this.Name = HighCards.First(x => x.Value == this.Value).Key.ToString();
            } else this.Name = _value.ToString();
        }

        public override string ToString() {
            if (this.Value >= 15) return this.Name; //already identified high card in constructor
            if (this.Face != null) {
                return this.Name.ToString() + (char)this.Face;
            }
            return string.Empty;
        }
    }
}
