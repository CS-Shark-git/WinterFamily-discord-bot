using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace WinterFamily.Main.Application.Commands
{
    [SlashCommandGroup("rules", "Команды для правил")]
    internal class RulesCommands : ApplicationCommandModule
    {
        [SlashRequireUserPermissions(Permissions.Administrator)]
        [SlashCommand("invoke-panel", "Вызывает панель правил сервера")]
        public async Task InvokePanel(InteractionContext ctx)
        {
            var thisChannel = ctx.Channel;

            var firstEmbed = new DiscordEmbedBuilder
            {
                Title = "►ㅤПравила сервера.",
                Color = new DiscordColor("2b2d31"),
                Description = "```● Правила — набор определенных соглашений, " +
                "которые необходимо соблюдать каждому участнику сервер. " +
                "Их нарушение влечет за собой наказания.```"
            };
            firstEmbed.WithImageUrl("https://media.discordapp.net/attachments/1144020463291486249/1144255848118485002/TNTsPXn.png");

            var secondEmbed = new DiscordEmbedBuilder
            {
                Description = "**Посмотреть правила можно, раскрыв категорию разделов ниже. Читайте внимательно!**",
                Color = new DiscordColor("2b2d31"),
            };
            secondEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");

            var options = new List<DiscordSelectComponentOption> 
            {
                new DiscordSelectComponentOption("╔ Раздел 1", "rules_group1_select", "● Токс", emoji: 
                new DiscordComponentEmoji(1167126002489634876)),
                new DiscordSelectComponentOption("╔ Раздел 2", "rules_group2_select", "● Реклама", emoji:
                new DiscordComponentEmoji(1167126002489634876)),
                new DiscordSelectComponentOption("╔ Раздел 3", "rules_group3_select", "● Флуд, спам, оффтоп", emoji:
                new DiscordComponentEmoji(1167126002489634876)),
                new DiscordSelectComponentOption("╔ Раздел 4", "rules_group4_select", "● Контент 18+", emoji:
                new DiscordComponentEmoji(1167126002489634876)),
                new DiscordSelectComponentOption("╔ Раздел 5", "rules_group5_select", "● Войс", emoji:
                new DiscordComponentEmoji(1167126002489634876)),
                new DiscordSelectComponentOption("╔ Раздел 6", "rules_group6_select", "● Доп. правила", emoji:
                new DiscordComponentEmoji(1167126002489634876)),
            };

            var rulesSelect = new DiscordSelectComponent("rules_select", "Разделы...", options);

            await thisChannel.SendMessageAsync(
                new DiscordMessageBuilder()
                .AddEmbed(firstEmbed)
                .AddEmbed(secondEmbed)
                .AddComponents(rulesSelect)
                );

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Панель отправлена успешно!").AsEphemeral());
            await Task.Delay(1000);
            await ctx.DeleteResponseAsync();
        }

    }
}
