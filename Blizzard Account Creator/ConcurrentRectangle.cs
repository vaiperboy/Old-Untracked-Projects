using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Blizzard_Account_Creator {
    public class RectangleV2 {
        public Size Size { get; set; }
        public Point Location { get; set; }
        public int Used { get; set; }
        public bool isBeingUsed { get; set; }
    }
    class RectanglesList : List<RectangleV2> {
        //i dont need all the shit Rectangle gives me...
        public RectanglesList(List<Rectangle> rects) {
            for (int i = 0; i < rects.Count; i++) {
                this.Add(new RectangleV2() {
                    Location = rects[i].Point(),
                    Size = rects[i].Size
                });
            }
        }

        public RectanglesList(List<RectangleV2> rects) {
            for (int i = 0; i < rects.Count; i++) {
                this.Add(rects[i]);
            }
        }
        private static readonly object obj = new object();
        public RectangleV2 PickBestPlacement() {
            lock (obj) {
                var rect = this.OrderBy(x => x.isBeingUsed)
              .ThenBy(x => x.Used)
              .FirstOrDefault();
                lock (rect) {
                    if (rect != default) {
                        rect.Used++;
                        return rect;
                    }
                    return default;
                }
            }
          
        }
    }
}
