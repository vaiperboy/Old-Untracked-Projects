using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace ikout_assister {
    public static class Settings {

        public readonly static Rectangle TABLE_LOCATION_PLAYER0 = new Rectangle(
            467,
            350,
            1550,
            1030
            );

        //  var point = new Point(860, 469); //830 437 866 469
        //var size = new Size(780, 550); //1620 477
        public readonly static Rectangle TABLE_LOCATION_WITHOUT_PLAYER0 = new Rectangle(
            865,
            450,
            800,
            580
            );

        public readonly static Rectangle CARD_CROP_DIMENSIONS = new Rectangle(
            18,
            5,
            54,
            118
            );

        

        public static class Thresholds {
            public readonly static int Table = 231,
                Card = 237;
        }

        public static class Sizes {
            public readonly static Size SCREEN_WORKING_ON_DIMENSIONS = new Size(2560, 1440);
            
        }
        public static class Locations {
            public readonly static Point[] Players = {
                new Point(2223, 816),
                new Point(1190, 350),
                new Point(140, 820)
            };
        }
    }
}
