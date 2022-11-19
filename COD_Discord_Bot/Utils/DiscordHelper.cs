using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Utils {
    class DiscordHelper {
        public static DiscordEmbed CreateEmbed(string title, string content = "", bool error = false) {
            return new DiscordEmbedBuilder {
                Author = new DiscordEmbedBuilder.EmbedAuthor() { Name = "COD Bot" },
                Title = title,
                Description = content,
                Color = error ? DiscordColor.Red : DiscordColor.Green
            }.Build();
        }
    }
}
