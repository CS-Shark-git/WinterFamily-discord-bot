using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using WinterFamily.Main.Persistence;
using DSharpPlus.SlashCommands.Attributes;
using WinterFamily.Main.Utils.Discord;
using WinterFamily.Main.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace WinterFamily.Main.Application.Commands;

[SlashCommandGroup("mm", "Команды для гарантов")]
internal class MiddleMansCommands : ApplicationCommandModule
{
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("invoke-panel", "Вызывает панель гарантов сервера")]
    public async Task InvokePanel(InteractionContext ctx)
    {
        var thisChannel = ctx.Channel;

        var firstEmbed = new DiscordEmbedBuilder
        {
            Title = "►ㅤГаранты.",
            Color = new DiscordColor("2b2d31"),
            Description = "```● Гарант - независимая сторона, обеспечивающая безопасность и надежность при сделках. " +
            "Он контролирует процесс и обязательства сторон. " +
            "Гарант также отвечает за хранение и распределение средств.```"
        };
        firstEmbed.WithImageUrl("https://media.discordapp.net/attachments/1144020463291486249/1144255848118485002/TNTsPXn.png");

        var secondEmbed = new DiscordEmbedBuilder
        {
            Description = "**Посмотреть список гарантов можно, раскрыв категорию ниже. За шуточные заявки - наказание**",
            Color = new DiscordColor("2b2d31"),
        };
        secondEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");

        using (var db = new ApplicationContext())
        {
            var options = new List<DiscordSelectComponentOption>();
            var middleMans = db.MiddleMans;
            if (middleMans.Count() < 1)
            {
                await thisChannel.SendMessageAsync(
                new DiscordMessageBuilder()
                .AddEmbed(firstEmbed)
                .AddEmbed(secondEmbed)
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, "none_mm_button", "Нет гарантов в списке!",
                disabled: true))
                );
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Панель отправлена успешно!")
                    .AsEphemeral());
                await Task.Delay(1000);
                await ctx.DeleteResponseAsync();
                return;
            }
            foreach (var mm in db.MiddleMans.Include(x => x.Reviews))
            {
                var member = await ctx.Guild.GetMemberAsync(mm.Id);
                options.Add(new DiscordSelectComponentOption(
                    member.Username,
                    mm.Id.ToString(),
                    description: member.CreationTimestamp.Date.ToShortDateString(),
                    emoji: new DiscordComponentEmoji(1164595872663879760)));;
            }

            var middleMansSelect = new DiscordSelectComponent("mm_select", "Выберите гаранта...", options);
            var submitAllButton = new DiscordButtonComponent(ButtonStyle.Secondary, "mm_submit_all_button",
                "Оставить заявку всем гарантам", emoji: new DiscordComponentEmoji(1163527148787744918));

            await thisChannel.SendMessageAsync(
                new DiscordMessageBuilder()
                .AddEmbed(firstEmbed)
                .AddEmbed(secondEmbed)
                .AddComponents(middleMansSelect)
                .AddComponents(submitAllButton)
                );
        }
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Панель отправлена успешно!").AsEphemeral());
        await Task.Delay(1000);
        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("add", "Добавить нового гаранта")]
    public async Task Add(InteractionContext ctx, [Option("Пользователь", "Пользователь для добавления")] DiscordUser user)
    {
        var member = await ctx.Guild.GetMemberAsync(user.Id);
        if (member is null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                "Этого пользователя нет на сервере!"))
                .AsEphemeral());
            return;
        }

        if (ctx.Member.Roles.ToList().Contains(ctx.Guild.GetRole(Settings.LowAdminRole)) != true &&
            ctx.Member.Permissions.HasPermission(Permissions.Administrator) != true)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                $"Только участник с ролью " +
                $"{ctx.Guild.GetRole(Settings.LowAdminRole).Name} или выше может использовать данную команду!"))
                .AsEphemeral());
            return;
        }

        using (var db = new ApplicationContext())
        {
            var middleMan = db.MiddleMans.Find(member.Id);
            if (middleMan != null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                    "Гарант уже внесен в список!"))
                    .AsEphemeral());
                return;
            }

            db.MiddleMans.Add(new MiddleMan
            {
                Id = member.Id,
            });
            await db.SaveChangesAsync();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
                    $"Гарант {member.Mention} добавлен успешно!",
                    TextType.Bold))
                    .AsEphemeral());
        }
    }

    [SlashCommand("remove", "Убрать гарантаиз списка")]
    public async Task Remove(InteractionContext ctx, [Option("Пользователь", "Пользователь для удаления")] DiscordUser user)
    {
        var member = await ctx.Guild.GetMemberAsync(user.Id);
        if (member is null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                "Этого пользователя нет на сервере!")));
            return;
        }

        if (ctx.Member.Roles.ToList().Contains(ctx.Guild.GetRole(Settings.LowAdminRole)) != true &&
            ctx.Member.Permissions.HasPermission(Permissions.Administrator) != true)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, $"Только участник с ролью " +
                $"{ctx.Guild.GetRole(Settings.LowAdminRole).Name} или выше может использовать данную команду!")));
            return;
        }

        using (var db = new ApplicationContext())
        {
            var middleMan = db.MiddleMans.Find(member.Id);
            if (middleMan is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                    "Этого пользователя нет в списке гарантов!"))
                    .AsEphemeral());
                return;
            }

            db.MiddleMans.Remove(middleMan);
            await db.SaveChangesAsync();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
                    $"Гарант {member.Mention} удалён успешно!",
                    TextType.Bold))
                    .AsEphemeral());
        }
    }

    [SlashCommand("edit", "Редактировать свой профиль гаранта")]
    public async Task Edit(InteractionContext ctx, [Option("Описание", "Введите описание")] string description = "",
        [Option("Прайс_лист", "Введите прайс лист")] string priceList = "")
    {
        using (var db = new ApplicationContext())
        {
            var middleMan = db.MiddleMans.Find(ctx.Member.Id);
            if (middleMan is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                    "Вас нет в списке гарантов!"))
                    .AsEphemeral());
                return;
            }

            middleMan.PriceList = priceList;
            middleMan.Description = description;
            db.MiddleMans.Update(middleMan);
            await db.SaveChangesAsync();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
                    "Профиль отредактирован успешно!",
                    TextType.Bold))
                    .AsEphemeral());
        }
    }

    [SlashCommand("view", "Отобразить профиль гаранта")]
    public async Task View(InteractionContext ctx, [Option("Гарант", "Введите гаранта")] DiscordUser user)
    {
        var member = await ctx.Guild.GetMemberAsync(user.Id);
        if (member is null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                    "Пользователя нет на сервере!"))
                    .AsEphemeral());
            return;
        }

        using (var db = new ApplicationContext())
        {
            var middleMan = db.MiddleMans.Include(x => x.Reviews)
                .FirstOrDefault(x => x.Id == member.Id);
            if (middleMan is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                    "Пользователя нет в списке гарантов!"))
                    .AsEphemeral());
                return;
            }

            var responseEmbed = new DiscordEmbedBuilder
            {
                Title = $"►ㅤГарант:ㅤ{member.Username}",
                Color = new DiscordColor("2b2d31"),
                Description = $"> {member.Mention}\n⠀"
            };

            double averageRate = 0;
            if (middleMan.Reviews.Count != 0)
            {
                averageRate = Math.Round((double)middleMan.Reviews.Select(x => x.Rate).Sum() / middleMan.Reviews.Count, 1);
            }
            
            responseEmbed.AddField("╔Описание:", $"```● {middleMan!.Description}```");
            responseEmbed.AddField("╔Прайс лист:", $"```● {middleMan.PriceList}```");
            responseEmbed.AddField("╔Отзывы:", $"```● {middleMan.Reviews.Count}```", true);
            responseEmbed.AddField("╔Средняя оценка:", $"```● {averageRate}```", true);
            responseEmbed.WithThumbnail(member.AvatarUrl);
            responseEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");

            var submitButton = new DiscordButtonComponent(ButtonStyle.Secondary, "mm_submit_button",
                "Оставить заявку гаранту", emoji: new DiscordComponentEmoji(1163527148787744918));
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(responseEmbed)
                .AddComponents(submitButton).AsEphemeral());
        }
    }
}
