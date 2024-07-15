using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using WinterFamily.Main.Persistence;
using Microsoft.EntityFrameworkCore;
using WinterFamily.Main.Utils.Discord;
using DSharpPlus.SlashCommands.Attributes;
using WinterFamily.Main.Utils.Extensions;

namespace WinterFamily.Main.Application.Commands;

[SlashCommandGroup("vacancies", "Команды для тикетов")]
internal class VacanciesCommands : ApplicationCommandModule
{
    public enum Vacancies
    {
        [ChoiceName("Модератор")]
        Moderator,
        [ChoiceName("Ивентер")]
        Eventer,
        [ChoiceName("Все")]
        All
    }

    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("invoke-panel", "Вызывает панель для подачи заявки на вакансии")]
    public async Task InvokePanel(InteractionContext ctx)
    {
        var thisChannel = ctx.Channel;

        var firstEmbed = new DiscordEmbedBuilder
        {
            Title = "►ㅤВакансии.",
            Color = new DiscordColor("2b2d31"),
            Description= "```● Если Вы хотите стать частью персонала нашего сервера, то у Вас есть такая возможность!\n" +
            "Подайте заявку на интересующую Вас должность и мы обязательно её рассмотрим.```"
        };
        firstEmbed.WithImageUrl("https://media.discordapp.net/attachments/1144020463291486249/1144255848118485002/TNTsPXn.png");
        DiscordSelectComponent vacanciesSelect = new DiscordSelectComponent("vacancies_select", "Вакансия...", 
            new List<DiscordSelectComponentOption>
            { 
                new DiscordSelectComponentOption("Модератор", "moderator_value", emoji: new DiscordComponentEmoji(1163525225779044415)),
                new DiscordSelectComponentOption("Ивентер", "eventer_value", emoji: new DiscordComponentEmoji(1163525221890932766)),
            });

        var secondEmbed = new DiscordEmbedBuilder
        {
            Description = "**Внимание! За шуточные заявки Вы получите наказание вплоть до чс набор**",
            Color = new DiscordColor("2b2d31")
        };
        secondEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");


        await thisChannel.SendMessageAsync( 
            new DiscordMessageBuilder()
            .AddEmbed(firstEmbed)
            .AddEmbed(secondEmbed)
            .AddComponents(vacanciesSelect));

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Панель отправлена успешно!").AsEphemeral());
        await Task.Delay(1000);
        await ctx.DeleteResponseAsync();
    }

    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("close", "Закрыть набор на опр.вакансии")]
    public async Task CloseVacancies(InteractionContext ctx, 
        [Option("Вакансия", "Выберите вакансию, которую хотите закрыть")] Vacancies vacancies) 
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());
        
        switch(vacancies) 
        {
            case Vacancies.Moderator:
                using(var db = new ApplicationContext()) 
                {
                    var mod = db.Vacancies.Find("moderator_value");
                    if (mod is null) 
                    {
                        var errorEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Error,
                        "Ошибка в базе данных, свяжитесь с разработчиком — cs_shark");
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(errorEmbed));
                    }
                    mod!.IsOpened = false;
                    await db.SubmittedUsers.ForEachAsync(x => x.ModeratorSubmitted = false);
                    await db.SaveChangesAsync();

                    var responseEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Success,
                        "Набор на вакансию модератора закрыт");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(responseEmbed));
                }
                break;

            case Vacancies.Eventer:
                using (var db = new ApplicationContext())
                {
                    var mod = db.Vacancies.Find("eventer_value");
                    if (mod is null)
                    {
                        var errorEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Error,
                        "Ошибка в базе данных, свяжитесь с разработчиком — cs_shark");
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(errorEmbed));
                    }
                    mod!.IsOpened = false;
                    await db.SubmittedUsers.ForEachAsync(x => x.EventerSubmitted = false);
                    await db.SaveChangesAsync();

                    var responseEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Success,
                        "Набор на вакансию ивентера закрыт");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(responseEmbed));
                }
                break;

            case Vacancies.All:
                using (var db = new ApplicationContext())
                {
                    db.SubmittedUsers.Clear();
                    await db.Vacancies.ForEachAsync(x => x.IsOpened = false);
                    await db.SaveChangesAsync();

                    var responseEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Success,
                        "Набор на все вакаснии закрыт");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(responseEmbed));
                }
                break;
        }
    }

    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("open", "Открыть набор на опр.вакансии")]
    public async Task OpenVacancies(InteractionContext ctx,
        [Option("Вакансия", "Выберите вакансию, которую хотите открыть")] Vacancies vacancies)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        switch (vacancies)
        {
            case Vacancies.Moderator:
                using (var db = new ApplicationContext())
                {
                    var mod = db.Vacancies.Find("moderator_value");
                    if (mod is null)
                    {
                        var errorEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Error,
                        "Ошибка в базе данных, свяжитесь с разработчиком — cs_shark");
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(errorEmbed));
                    }
                    mod!.IsOpened = true;
                    await db.SaveChangesAsync();
                    var responseEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Success,
                        "Набор на вакансию модератора открыт");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(responseEmbed));
                }
                break;

            case Vacancies.Eventer:
                using (var db = new ApplicationContext())
                {
                    var mod = db.Vacancies.Find("eventer_value");
                    if (mod is null)
                    {
                        var errorEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Error,
                        "Ошибка в базе данных, свяжитесь с разработчиком — cs_shark");
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(errorEmbed));
                    }
                    mod!.IsOpened = true;
                    await db.SaveChangesAsync();
                    var responseEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Success,
                        "Набор на вакансию ивентера открыт");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(responseEmbed));
                }
                break;

            case Vacancies.All:
                using (var db = new ApplicationContext())
                {
                    await db.Vacancies.ForEachAsync(x => x.IsOpened = true);
                    await db.SaveChangesAsync();

                    var responseEmbed = StyledMessageBuilder.BuildResultEmbed(Result.Success,
                        "Набор на все вакаснии открыт");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(responseEmbed));
                }
                break;
        }
    }
}
