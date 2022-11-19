using System;
using EasyConsole;
using Console = Colorful.Console;
using System.Drawing;

namespace Discord_Bots.Pages.CurrencyBots {
    public class CurrenyBotsPage : MenuPage{
        public CurrenyBotsPage(Program program) : base ("Abusing currency bots",
            program,
            new Option("Claim Dailies", program.NavigateTo<Pages.CurrencyBots.ClaimDailies>),
            new Option("Send Mekos, OwO", program.NavigateTo<MekosOwoBot>),
            new Option("Send Tatsu [BETA - SLOW]", program.NavigateTo<TatsuBot>),
            new Option("Claim Pokecord [BETA", program.NavigateTo<PokeCord>),
            new Option("Claim Flowers [BETA", program.NavigateTo<FlowersBot>)
            //,
            //new Option("Send Yui [BETA - SLOW]", program.NavigateTo<YuiBot>)
            ) {
        }
    }
}
