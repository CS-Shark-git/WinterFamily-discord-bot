using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace WinterFamily.Main.Application.Commands;

[SlashCommandGroup("tickets", "Команды для тикетов")]
internal class TicketsCommands : ApplicationCommandModule
{
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("invoke-panel", "Вызывает панель для подачи тикетов")]
    public async Task InvokePanel(InteractionContext ctx)
    {
        var thisChannel = ctx.Channel;

        var panelEmbed = new DiscordEmbedBuilder
        {
            Title = "►ㅤПоддержка.",
            Color = new DiscordColor("2b2d31")
        };
        panelEmbed.AddField("╔ Вопрос по серверу: ", "```● Задайте интересующий вас вопрос и получите на него ответ в личные сообщения.```", true);
        panelEmbed.AddField("╔ Жалоба на пользователя:", "```● Если у Вас проблемы с неприятным пользователем на сервере, мы поможем!```", true);
        panelEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        DiscordButtonComponent askQuestion = new DiscordButtonComponent(ButtonStyle.Secondary, "ask_question_button", 
            "Задать вопрос", emoji: new DiscordComponentEmoji(1163526378575106149));

        DiscordButtonComponent userComplaint = new DiscordButtonComponent(ButtonStyle.Secondary, "user_complaint_button",
            "Жалоба на пользователя", emoji: new DiscordComponentEmoji(1163527791581605950));

        await thisChannel.SendMessageAsync(
            new DiscordMessageBuilder()
            .AddEmbed(panelEmbed)
            .AddComponents(askQuestion, userComplaint));

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Панель отправлена успешно!").AsEphemeral());
        await Task.Delay(1000);
        await ctx.DeleteResponseAsync();
    }
}
