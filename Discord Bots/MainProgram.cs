using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using EasyConsole;
using IniParser;
using IniParser.Model;
namespace Discord_Bots {
    public class MainProgram : Program {
        public static List<Token> tokens = new List<Token>();
        public MainProgram() : base ("Main menu",
            breadcrumbHeader: true) {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            var parser = new FileIniDataParser();
            
            if (File.Exists(path)) {
                var data = parser.ReadFile(path);
                Colorful.Console.WriteLine("config file found... setting options", System.Drawing.Color.Yellow);
                var id = data["main"]["id"];
                var channels = data["main"]["channels"].Split(',');
                if (id.Length > 0)
                    Settings.id = id;
                if (channels.Length > 0)
                    Settings.channels = channels;
            }
            AddPage(new Pages.TokenLoader(this));
            AddPage(new Pages.TokenLoader.Local(this));
            AddPage(new Pages.MainMenu(this));
            SetPage<Pages.TokenLoader>().Display(CancellationToken.None);
            AddPage(new Pages.TestPage(this, tokens));
            //SetPage<Pages.TestPage>();



            AddPage(new Pages.ServerJoiner(this, tokens)) ;
            AddPage(new Pages.TokenAliver(this, tokens));
            AddPage(new Pages.ServerMessageSender(this, tokens));
            AddPage(new Pages.CurrencyBots.CurrenyBotsPage(this));
            AddPage(new Pages.CurrencyBots.MekosOwoBot(this, tokens));
            AddPage(new Pages.CurrencyBots.ClaimDailies(this, tokens));
            AddPage(new Pages.CurrencyBots.TatsuBot(this, tokens));
            AddPage(new Pages.CurrencyBots.YuiBot(this, tokens));
            AddPage(new Pages.CurrencyBots.PokeCord(this, tokens));
            AddPage(new Pages.CurrencyBots.FlowersBot(this, tokens));
            AddPage(new Pages.ValidateTokens(this, tokens));
            //SetPage<Pages.CurrencyBots.TatsuBot>().Display(CancellationToken.None);

            SetPage<Pages.MainMenu>();
        }
    }
}
