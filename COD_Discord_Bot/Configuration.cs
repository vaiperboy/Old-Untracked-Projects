using COD_Discord_Bot.DiscordClasses;
using System;
using System.Collections.Generic;
using System.IO;
namespace COD_Discord_Bot {

    public enum UserIds : ulong {
        ToxicAlt = 621620337733271583,
        ToxicMain = 621643802695696384,
        MarXz = 346243302598770689,
        Cordisc = 762822572378292295,
        PrelacyJ = 251266599439302656,
        TikkiMessi = 607690665416982528,
        GodisHere = 311186545971298314,
        Bado = 269050707070418945,
        Gavzter = 205832091252948992,
        Argate = 853719117075972106,
        Ghost69 = 567039689509306378,
        Cracked = 757940964076683275,
        Supreme = 607308155373748225
    };

    public enum ServerIds : ulong {
        CodAccounts = 722783836206923776,
        RickServer = 738468147165790379,
        BadoServer = 823156583298498562,
        NeyoTheMidgetServer = 748327383571890299,
        Supreme1 = 699136347369242645,
        Supreme2 = 810685620728758342,
        Supreme3 = 774774382039662633,
        LocksmithServices = 762823319933288468,
        ArgateServer = 853717706313039963
    }

    public enum ChannelIds : ulong {
        saleLog = 821863271064666143,
        ticketsParent = 760855721012101211
    }

    public enum TicketsChannelIds : ulong {
        codAccounts = 760855721012101211,
        argate1 = 764960948267122688,
        argate2 = 816218589199466536,
        argate3 = 807401940085768202,
        argate4 = 789615654134480946,
        argate5 = 789602715101691904,
        argate6 = 812896478950457374,
        argate7 = 815855443792101406,
        argate8 = 809433453803339776
    }

    class Configuration {

        public static Dictionary<ulong, DiscordUser[]> Servers = new Dictionary<ulong, DiscordUser[]>() {
            {0,new DiscordUser[] { //DANGEROUS AREA TO ADD PEOPLE IN
                new DiscordUser("Toxic Big", (ulong)UserIds.ToxicMain, Permissions.All),
                new DiscordUser("Toxic small", (ulong)UserIds.ToxicAlt, Permissions.All)
            }},
            {(ulong)ServerIds.CodAccounts, new DiscordUser[] {
                new DiscordUser("Marxz", (ulong)UserIds.MarXz, Permissions.All),
                new DiscordUser("Ghost69", (ulong)UserIds.Ghost69, Permissions.All),
                new DiscordUser("Bado", (ulong)UserIds.Bado, Permissions.All),
                new DiscordUser("PrelacyJ", (ulong)UserIds.PrelacyJ, Permissions.All),
                new DiscordUser("GodisHere", (ulong)UserIds.GodisHere, Permissions.All),
                new DiscordUser("Cordisc", (ulong)UserIds.Cordisc, Permissions.All),
                new DiscordUser("Argate", (ulong)UserIds.Argate, Permissions.All),
                new DiscordUser("Cracked", (ulong)UserIds.Cracked, Permissions.All),
                new DiscordUser("Gavzter", (ulong)UserIds.Gavzter, Permissions.Accounts) }
            },
            {(ulong)ServerIds.BadoServer, new DiscordUser[] {
                new DiscordUser("Bado", (ulong)UserIds.Bado, Permissions.All),
                new DiscordUser("PPP-King", 600893050562281482, Permissions.Shoppy),
            } },
            {(ulong)ServerIds.Supreme1, new DiscordUser[] {
                new DiscordUser("Supreme", 607308155373748225, Permissions.All)
            } },
            {(ulong)ServerIds.Supreme2, new DiscordUser[] {
                new DiscordUser("Supreme", 607308155373748225,  Permissions.Shoppy | Permissions.Activision)
            } },
            {(ulong)ServerIds.Supreme3, new DiscordUser[] {
                new DiscordUser("Supreme", 607308155373748225,  Permissions.Shoppy | Permissions.Activision)
            } },
            {(ulong)ServerIds.LocksmithServices, new DiscordUser[] {
                new DiscordUser("Cordisc", 762822572378292295, Permissions.All)
            }},
            {(ulong)ServerIds.ArgateServer, new DiscordUser[] {
                new DiscordUser("Argate", (ulong)UserIds.Argate, Permissions.All),
                new DiscordUser(".", 664110667749523476 , Permissions.All),
                new DiscordUser(".", 564556408079646721 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 173627267853844490 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 252622066140839938 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 380236405491892224 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 474355113704816644 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 173627267853844490 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 383549033529999361 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 658469353682894885 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 664523811680419861 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 709362340864327731 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 799333242330808320, Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 853717297260658728, Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 747488317137747980 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 331674504881242112 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 720070548876427355 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 631282207864127552 , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 780964158278860810  , Permissions.Shoppy | Permissions.Activision),
                new DiscordUser(".", 807948019105529906   , Permissions.Shoppy | Permissions.Activision),
            }
            },
        };

        public static Dictionary<ulong, Shoppy.Shoppy> Shoppys = new Dictionary<ulong, Shoppy.Shoppy>() {
            {(ulong)ServerIds.CodAccounts, new Shoppy.Shoppy("hWi1pBdkgs3JXSVbUz837ZIkqveTVjwQudR23e1yBOssfPUfMG", new Shoppy.ShoppyConfig("gSrFD5v")) },
            {(ulong)ServerIds.Supreme1, new Shoppy.Shoppy("dXPqg2KYsZownANNqrpPMdNqvUm8CiUGberC6mMrkOz7IAkiK2", new Shoppy.ShoppyConfig("6Lso26R"))},
            {(ulong)ServerIds.Supreme2, new Shoppy.Shoppy("dXPqg2KYsZownANNqrpPMdNqvUm8CiUGberC6mMrkOz7IAkiK2", new Shoppy.ShoppyConfig("6Lso26R"))},
            {(ulong)ServerIds.Supreme3, new Shoppy.Shoppy("dXPqg2KYsZownANNqrpPMdNqvUm8CiUGberC6mMrkOz7IAkiK2", new Shoppy.ShoppyConfig("6Lso26R"))},
            {(ulong)ServerIds.LocksmithServices, new Shoppy.Shoppy("EdB9Suk3sO22vURuRtdECIBBjvcIkgkCU2VhcMUSkj7Tqea9da", new Shoppy.ShoppyConfig("DxUDhr6"))},
            {(ulong)ServerIds.BadoServer, new Shoppy.Shoppy("WqO9CN9eG6azl4GjuAJ3uHJYvmCjW4s9EgaHeLpFne6u20Ndg2", new Shoppy.ShoppyConfig("x2VE5rI"))},
            {(ulong)ServerIds.ArgateServer, new Shoppy.Shoppy("s9EaVIJjNWOEAFw5oFC1qiO16A5A1OIcOdPLeFxxo8xceniQKQ", new Shoppy.ShoppyConfig("Ta5ra4E"))}
        };

        public static int MaxCodesPerEmail = 3;

        public static string[] AllowedTLDS = {
            "mail.ru",
            "bk.ru",
            "inbox.ru",
            "list.ru",
            "mail.ua"
        };
        public static ulong[] CensoredServersID =  {
            (ulong)ServerIds.CodAccounts,
            (ulong)ServerIds.RickServer,
            (ulong)ServerIds.NeyoTheMidgetServer
        };

        public static string AccountsLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "accounts.txt");

        public static int SafeAmountToSend = 10;
        public static string SpreadshetID = "12LXfbOAHQ4JqlDLCDfREhVEFFDX779_FZQ-n3gefnjM";
        public static string WorksheetName = "AUTOMATED BOT";
        public static int MaxActivisionAccounts = 20;
        public static int MaxThreads = 150;
        public static int ShoppyTimeOffsetSubtract = 2;
    }
}
