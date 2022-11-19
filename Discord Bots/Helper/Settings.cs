using System;
using System.Collections.Generic;
using System.Text;

namespace Discord_Bots {
    public class Settings {

        public static class ID {
            public const string Toxic_7464 = "165879914103439360";
            public const string Toxic_0099 = "399656832475463701";
            public const string SecondSon_4410 = "418136111123136514";
            public static string temp { get; set; }


        }
        public static string id { get; set; } = ID.Toxic_7464;
        public static string TatsuId { get; set; } = id;

        public static string[] channels { get; set; } = new string[] {
            "618921583360081970"
            ,"618921604432396318"};
        public static int botDailyDelay { get; set; } = 3000;
        public static int botSendDelay { get; set; } = 3000;
        public static int botRepDelay { get; set; } = 3000;
        public static int msgSendDelay { get; set; }
        public static int tokenLimit { get; set; } = 4;
    }
}
