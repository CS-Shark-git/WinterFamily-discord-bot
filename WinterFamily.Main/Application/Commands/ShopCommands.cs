using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace WinterFamily.Main.Application.Commands;

[SlashCommandGroup("shop", "Команды для магазина")]
internal class ShopCommands : ApplicationCommandModule
{
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("invoke-panel", "Вызывает панель магазина сервера")]
    public async Task InvokePanel(InteractionContext ctx) 
    {
        var thisChannel = ctx.Channel;

        var firstEmbed = new DiscordEmbedBuilder
        {
            Title = "►ㅤМагазин.",
            Color = new DiscordColor("2b2d31"),
            Description = "```● В этом канале Вы можете приобрести товары сервера.```"
        };
        firstEmbed.WithImageUrl("https://media.discordapp.net/attachments/1144020463291486249/1144255848118485002/TNTsPXn.png");
        DiscordSelectComponent shopSelect = new DiscordSelectComponent("shop_select", "Товар...",
            new List<DiscordSelectComponentOption>  
            {
            new DiscordSelectComponentOption("Реклама", "ad", emoji: new DiscordComponentEmoji(1164276728559046759)),
            new DiscordSelectComponentOption("Лавки", "stores", emoji: new DiscordComponentEmoji(1164276725002293429)),
            });
        var submitButton = new DiscordButtonComponent(ButtonStyle.Secondary, "shop_submit_button", 
            "Оставить заявку", emoji: new DiscordComponentEmoji(1164279328687800512));
        var secondEmbed = new DiscordEmbedBuilder
        {
            Description = "**Посмотреть ассортимент товаров можно, раскрыв категорию ниже. За шуточную заявку - наказание**",
            Color = new DiscordColor("2b2d31")
        };
        secondEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");

        await thisChannel.SendMessageAsync(
            new DiscordMessageBuilder()
            .AddEmbed(firstEmbed)
            .AddEmbed(secondEmbed)
            .AddComponents(shopSelect)
            .AddComponents(submitButton));

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Панель отправлена успешно!").AsEphemeral());
        await Task.Delay(1000);
        await ctx.DeleteResponseAsync();
    }
}
