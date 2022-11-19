using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV.Structure;

namespace ikout_assister {
    public class Round {
        public Player[] Player { get; set; }
        public Card Hokam { get; set; }
        public bool IsFinished { get; set; }
        public Card RootCard { get; set; }
        public Player DominantPlayer { get; set; }

        public Round(Player[] player, Card hokam = default) { //incase i decide stuff later on
            this.Player = player;
            this.Hokam = hokam;
        }

        public void UpdatePlayer(Player player) {
            int index = this.Player.Count(x => x.Card != default(Card));
            this.Player[index] = player;
            if (index == 0) RootCard = player.Card;
            else if (index == this.Player.Length - 1) { //round finished
                SortPlayers();
                DominantPlayer = Player
                    .OrderByDescending(x => x.Card.Value)
                    .First();

                IsFinished = true;


            }
        }

        private void SortPlayers() {
            //order bottom, right, top, left
            Player[] sorted = {
                this.Player.OrderByDescending(x => x.Location.Center.Y).First(),
                this.Player.OrderByDescending(x => x.Location.Center.X).First(),
                this.Player.OrderByDescending(x => x.Location.Center.Y).Last(),
                this.Player.OrderByDescending(x => x.Location.Center.X).Last()
            };
            this.Player = sorted;
        }

    }
    public struct Player {
        public Card Card { get; set; }
        public DateTime TimeThrown { get; set; }
        public RotatedRect Location { get; set; }
    }
}
