using System;
using System.Collections.Generic;
using System.Text;
using EasyConsole;

namespace Discord_Bots.Pages {
    class MainMenu : MenuPage  {
        public MainMenu(Program program) : base ("Main menu",
            program,
            new Option("Mass server joiner", program.NavigateTo<ServerJoiner>),
            new Option("Server message sender", program.NavigateTo<ServerMessageSender>),
            new Option("Currency bots", program.NavigateTo<CurrencyBots.CurrenyBotsPage>),
            new Option("test page", program.NavigateTo<TestPage>),
            new Option("Token validator", program.NavigateTo<Pages.ValidateTokens>),
            new Option("Token Aliver", program.NavigateTo<Pages.TokenAliver>)) {
        }
    }
}
