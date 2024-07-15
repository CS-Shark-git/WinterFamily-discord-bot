using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace WinterFamily.Main.Application.Commands;

[SlashCommandGroup("autoroles", "Команды для авторолей")]
internal class AutoRolesCommands : ApplicationCommandModule
{
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("invoke-panel", "Вызывает панель для подачи тикетов")]
    public async Task InvokePanel(InteractionContext ctx) 
    {
        var thisChannel = ctx.Channel;

        var firstEmbed = new DiscordEmbedBuilder
        {
            Title = "►ㅤАвтороли.",
            Color = new DiscordColor("2b2d31"),
        }.WithImageUrl("https://media.discordapp.net/attachments/1144020463291486249/1144255848118485002/TNTsPXn.png");

        var secondEmbed = new DiscordEmbedBuilder
        {
            Color = new DiscordColor("2b2d31"),
            Description = "**Выберите роль, раскрыв категорию ниже.\r\nСнимается повторным нажатием!**"
        }.WithImageUrl("https://i.imgur.com/tabpqjj.png");

        var rolesSelect = new DiscordSelectComponent("autoroles_select", "Выберите роль...",
           new List<DiscordSelectComponentOption>
           {
                new DiscordSelectComponentOption("Убрать все роли", 
                "remove_roles", emoji: new DiscordComponentEmoji(1163784803196346388)),
                new DiscordSelectComponentOption("Получать уведомления об ивентах", 
                "events_role", emoji: new DiscordComponentEmoji(1164208823918133310)),
                new DiscordSelectComponentOption("Получать уведомления о конкурсах", 
                "giveaways_role", emoji : new DiscordComponentEmoji(1164208823918133310)),
                new DiscordSelectComponentOption("Получать уведомления о новостях", 
                "news_role", emoji : new DiscordComponentEmoji(1164208823918133310)),
                new DiscordSelectComponentOption("Получать уведомления о товарах в лавке", 
                "shops_role", emoji : new DiscordComponentEmoji(1164208823918133310)),
           });

        await thisChannel.SendMessageAsync(
            new DiscordMessageBuilder()
            .AddEmbed(firstEmbed)
            .AddEmbed(secondEmbed)
            .AddComponents(rolesSelect));
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Панель отправлена успешно!").AsEphemeral());
        await Task.Delay(1000);
        await ctx.DeleteResponseAsync();
    }
}
