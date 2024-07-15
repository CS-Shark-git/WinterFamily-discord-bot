using DSharpPlus.Entities;
using static System.Net.WebRequestMethods;

namespace WinterFamily.Main.Utils.Discord;

internal enum Result
{
    Success,
    Error
}

internal enum TextType
{
    Styled,
    Bold
}

internal enum PunishmentType 
{
    Mute,
    Ban
}

internal static class StyledMessageBuilder
{
    public static DiscordEmbedBuilder BuildResultEmbed(Result resultType, string message, TextType textType = TextType.Styled) 
    {
        string title;
        string iconUrl;
        if (resultType == Result.Success) 
        {
            title = "Успешно!";
            iconUrl = "https://i.imgur.com/flYWSSl.png";
        }
        else 
        {
            title = "Ошибка!";
            iconUrl = "https://i.imgur.com/6rX0rEn.png";
        }

        if (textType == TextType.Bold) 
        {
            message = $"**● {message} **";
        }
        else 
        {
            message = $"```● {message}```";
        }

        var responseEmbed = new DiscordEmbedBuilder
        {
            Title = title,
            Description = $"{message}",
            Color = new DiscordColor("2b2d31")
        }
        .WithThumbnail(iconUrl)
        .WithImageUrl("https://i.imgur.com/tabpqjj.png");


        return responseEmbed;
    }

    public static DiscordEmbedBuilder BuildRuleEmbed(string rule, string description, string duration, 
        PunishmentType punishment, string note = null) 
    {

        var embed = new DiscordEmbedBuilder
        {
            Title = $"╔ {rule}",
            Description = $"**● {description}\nㅤ\n**",
            Color = new DiscordColor("2b2d31")
        };
        embed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        embed.WithThumbnail("https://i.imgur.com/qCZcVHC.png");

        embed.AddField("<:timeicon:1167139886575390883>ㅤВремя:", $" ```{duration}```", true);
        if (punishment == PunishmentType.Mute)
        {
            embed.AddField("<:mutemsg:1167139884440494161>ㅤНаказание: ", " ```Мьют```", true);
        }
        else
        {
            embed.AddField("<:banhammer:1167139881559003287>ㅤНаказание: ", " ```Бан```", true);
        }

        if(note != null) 
        {
            embed.AddField($"Примечание:", $"```{note}```");
        }


        return embed;
    }

    public static DiscordEmbedBuilder BuildRuleEmbed(string rule, string description, string note = null)
    {

        var embed = new DiscordEmbedBuilder
        {
            Title = $"╔ {rule}",
            Description = $"**● {description}\nㅤ\n**",
            Color = new DiscordColor("2b2d31")
        };
        embed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        embed.WithThumbnail("https://i.imgur.com/qCZcVHC.png");       

        if (note != null)
        {
            embed.AddField($"Примечание:", $"```{note}```");
        }


        return embed;
    }
}
