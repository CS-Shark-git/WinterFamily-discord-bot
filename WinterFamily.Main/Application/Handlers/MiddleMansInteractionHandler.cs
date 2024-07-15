using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WinterFamily.Main.Common.Attributes;
using WinterFamily.Main.Persistence;
using WinterFamily.Main.Utils.Discord;
using WinterFamily.Main.Persistence.Models;

namespace WinterFamily.Main.Application.Handlers;

internal class MiddleMansInteractionHandler
{
    [ComponentInteraction("mm_select")]
    public async Task OnGuarantSelect(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        using (var db = new ApplicationContext())
        {
            var id = ulong.Parse(args.Values.First());
            var middleMan = db.MiddleMans.Include(x => x.Reviews).Where(x => x.Id == id).FirstOrDefault();
            var member = await args.Guild.GetMemberAsync(id);

            var responseEmbed = new DiscordEmbedBuilder
            {
                Title = $"►ㅤГарант:ㅤ{member.Username}",
                Color = new DiscordColor("2b2d31"),
                Description = $"> {member.Mention}\n⠀"
            };

            double averageRate = Math.Round((double)middleMan.Reviews.Select(x => x.Rate).Sum() / middleMan.Reviews.Count, 1);
            if (middleMan.Reviews.Count == 0)
            {
                averageRate = 0;
            }

            responseEmbed.AddField("╔Описание:", $"```● {middleMan!.Description}```");
            responseEmbed.AddField("╔Прайс лист:", $"```● {middleMan.PriceList}```");
            responseEmbed.AddField("╔Отзывы:", $"```● {middleMan.Reviews.Count}```", true);
            responseEmbed.AddField("╔Средняя оценка:", $"```● {averageRate}```", true);
            responseEmbed.WithThumbnail(member.AvatarUrl);
            responseEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");

            var submitButton = new DiscordButtonComponent(ButtonStyle.Secondary, "mm_submit_button",
                "Оставить заявку гаранту", emoji: new DiscordComponentEmoji(1163527148787744918));
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(responseEmbed)
                .AddComponents(submitButton).AsEphemeral());

            await args.Message.ModifyAsync();
        }

    }

    [ComponentInteraction("mm_submit_button")]
    public async Task OnSubmit(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        string description = args.Message.Embeds.First().Description;
        string regexPattern = @"(?<=<@!?)\d+(?=>)";
        var id = ulong.Parse(Regex.Match(description, regexPattern).Value);

        using (var db = new ApplicationContext())
        {
            if (db.ActiveTrades.Where(x => x.FirstTraderId == args.User.Id || x.SecondTraderId == args.User.Id).Count() > 0)
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .AddEmbed(StyledMessageBuilder
                        .BuildResultEmbed(Result.Error, "У Вас уже есть активная сделка. " +
                        "Сначала закройте её, чтобы отправить заявку на новую!"))
                        .AsEphemeral());
        }

        var modal = new DiscordInteractionResponseBuilder()
        {
            CustomId = "mm_submit_modal",
            Title = "Заявка гаранту"
        };
        modal.AddComponents(new TextInputComponent("Что вы трейдите?", "offer_input"));
        modal.AddComponents(new TextInputComponent("Что вы получаете?", "recieve_input"));
        modal.AddComponents(new TextInputComponent("ID пользователя с которым трейдитесь", "userid_input",
            "Он должен быть на сервере"));


        await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
        var modalResult = await client.GetInteractivity().WaitForModalAsync("mm_submit_modal", args.User, TimeSpan.FromMinutes(10));

        if (modalResult.TimedOut)
        {
            return;
        }

        await modalResult.Result.Interaction.CreateResponseAsync(
            InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        var middleMan = await args.Interaction.Guild.GetMemberAsync(id);
        var dmChannel = await middleMan.CreateDmChannelAsync();


        ulong traderId;
        var isId = ulong.TryParse(modalResult.Result.Values["userid_input"], out traderId);
        if (isId != true)
        {
            await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder()
                .AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Error, "Неккоректно введён ID!")).AsEphemeral());
            return;
        }
        using (var db = new ApplicationContext())
        {
            if (db.MiddleMans.Select(x => x.Id).Contains(modalResult.Result.Interaction.User.Id))
            {
                await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                    new DiscordFollowupMessageBuilder()
                    .AddEmbed(StyledMessageBuilder
                    .BuildResultEmbed(Result.Error, "Вы являетесь гарантом!")).AsEphemeral());
                return;
            }
        }
        DiscordMember traderMember;

        try
        {
            traderMember = await modalResult.Result.Interaction.Guild.GetMemberAsync(traderId);
        }
        catch
        {
            await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder()
                .AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Error, "Пользователя нет на сервере!")).AsEphemeral());
            return;
        }

        using (var db = new ApplicationContext())
        {
            if (db.ActiveTrades.Where(x => x.FirstTraderId == traderMember.Id || x.SecondTraderId == traderMember.Id).Count() > 0)
            {
                await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder()
                .AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Error, "У пользователя уже есть активная сделка. " +
                "Подождите, пока он закроёт её, чтобы отправить заявку на новую!"))
                .AsEphemeral());
                return;
            }
        }

        if (modalResult.Result.Interaction.User.Id == traderId)
        {
            await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder()
                .AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Error, "Вы не можете создать сделку с самим собой!")).AsEphemeral());
            return;
        }
        using (var db = new ApplicationContext())
        {
            if (db.MiddleMans.Select(x => x.Id).Contains(traderId))
            {
                await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                    new DiscordFollowupMessageBuilder()
                    .AddEmbed(StyledMessageBuilder
                    .BuildResultEmbed(Result.Error, "Вы не можете создать сделку с гарантом!")).AsEphemeral());
                return;
            }
        }

        var responseEmbed = new DiscordEmbedBuilder
        {
            Title = "Новая заявка на сделку!",
            Color = new DiscordColor("2b2d31"),
            Description = $"** Отправил: {modalResult.Result.Interaction.User.Mention}\nВторой участник сделки: {traderMember.Mention}**",
        };
        responseEmbed.WithThumbnail("https://i.imgur.com/v8fW7Js.png");
        responseEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        responseEmbed.AddField("Что Вы трейдите?", $"```{modalResult.Result.Values["offer_input"]}```");
        responseEmbed.AddField("Что Вы получаете?", $"```{modalResult.Result.Values["recieve_input"]}```");

        var acceptTradeButton = new DiscordButtonComponent(ButtonStyle.Secondary, "accept_trade_button", "Принять",
            emoji: new DiscordComponentEmoji(1163537815666163712));
        var cancelTradeButton = new DiscordButtonComponent(ButtonStyle.Secondary, "cancel_trade_button", "Отклонить",
            emoji: new DiscordComponentEmoji(1163784803196346388));

        await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
            "Ваша заявка отправлена успешно!")).AsEphemeral());

        await dmChannel.SendMessageAsync(new DiscordMessageBuilder()
            .AddEmbed(responseEmbed)
            .AddComponents(acceptTradeButton, cancelTradeButton));
    }

    [ComponentInteraction("mm_submit_all_button")]
    public async Task OnSubmitAll(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        using (var db = new ApplicationContext())
        {
            if (db.ActiveTrades.Where(x => x.FirstTraderId == args.User.Id || x.SecondTraderId == args.User.Id).Count() > 0)
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .AddEmbed(StyledMessageBuilder
                        .BuildResultEmbed(Result.Error, "У Вас уже есть активная сделка. " +
                        "Сначала закройте её, чтобы отправить заявку на новую!"))
                        .AsEphemeral());
        }

        var modal = new DiscordInteractionResponseBuilder()
        {
            CustomId = "mm_submit_modal",
            Title = "Заявка гаранту"
        };
        modal.AddComponents(new TextInputComponent("Что вы трейдите?", "offer_input"));
        modal.AddComponents(new TextInputComponent("Что вы получаете?", "recieve_input"));
        modal.AddComponents(new TextInputComponent("ID пользователя с которым трейдитесь", "userid_input",
            "Он должен быть на сервере"));


        await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
        var modalResult = await client.GetInteractivity().WaitForModalAsync("mm_submit_modal", args.User, TimeSpan.FromMinutes(10));

        if (modalResult.TimedOut)
        {
            return;
        }

        await modalResult.Result.Interaction.CreateResponseAsync(
            InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        ulong traderId;
        var isId = ulong.TryParse(modalResult.Result.Values["userid_input"], out traderId);
        if (isId != true)
        {
            await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder()
                .AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Error, "Неккоректно введён ID!")).AsEphemeral());
            return;
        }
        using (var db = new ApplicationContext())
        {
            if (db.MiddleMans.Select(x => x.Id).Contains(modalResult.Result.Interaction.User.Id))
            {
                await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                    new DiscordFollowupMessageBuilder()
                    .AddEmbed(StyledMessageBuilder
                    .BuildResultEmbed(Result.Error, "Вы являетесь гарантом!")).AsEphemeral());
                return;
            }
        }
        DiscordMember traderMember;

        try
        {
            traderMember = await modalResult.Result.Interaction.Guild.GetMemberAsync(traderId);
        }
        catch
        {
            await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder()
                .AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Error, "Пользователя нет на сервере!")).AsEphemeral());
            return;
        }

        using (var db = new ApplicationContext())
        {
            if (db.ActiveTrades.Where(x => x.FirstTraderId == traderMember.Id || x.SecondTraderId == traderMember.Id).Count() > 0)
            {
                await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder()
                .AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Error, "У пользователя уже есть активная сделка. " +
                "Подождите, пока он закроёт её, чтобы отправить заявку на новую!"))
                .AsEphemeral());
                return;
            }
        }

        if (modalResult.Result.Interaction.User.Id == traderId)
        {
            await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder()
                .AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Error, "Вы не можете создать сделку с самим собой!")).AsEphemeral());
            return;
        }
        using (var db = new ApplicationContext())
        {
            if (db.MiddleMans.Select(x => x.Id).Contains(traderId))
            {
                await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                    new DiscordFollowupMessageBuilder()
                    .AddEmbed(StyledMessageBuilder
                    .BuildResultEmbed(Result.Error, "Вы не можете создать сделку с гарантом!")).AsEphemeral());
                return;
            }
        }

        var responseEmbed = new DiscordEmbedBuilder
        {
            Title = "Новая заявка на сделку!",
            Color = new DiscordColor("2b2d31"),
            Description = $"** Отправил: {modalResult.Result.Interaction.User.Mention}\nВторой участник сделки: {traderMember.Mention}**",
        };
        responseEmbed.WithThumbnail("https://i.imgur.com/v8fW7Js.png");
        responseEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        responseEmbed.AddField("Что Вы трейдите?", $"```{modalResult.Result.Values["offer_input"]}```");
        responseEmbed.AddField("Что Вы получаете?", $"```{modalResult.Result.Values["recieve_input"]}```");

        var acceptTradeButton = new DiscordButtonComponent(ButtonStyle.Secondary, "accept_trade_button", "Принять",
            emoji: new DiscordComponentEmoji(1163537815666163712));

        await modalResult.Result.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder().AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
            "Ваша заявка отправлена успешно!")).AsEphemeral());

        var mmChannel = args.Guild.GetChannel(Settings.MiddleMansRequestsChannel);

        await mmChannel.SendMessageAsync(new DiscordMessageBuilder()
                        .AddEmbed(responseEmbed)
                        .AddComponents(acceptTradeButton));
    }

    [ComponentInteraction("accept_trade_button")]
    public async Task AcceptTrade(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        using (var db = new ApplicationContext())
        {
            if (db.ActiveTrades.Where(x => x.MiddleManId == args.User.Id).Count() > 0)
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder
                    .BuildResultEmbed(Result.Error, "У Вас уже есть активная сделка. " +
                    "Сначала закройте её, чтобы принять эту!"))
                    .AsEphemeral());
            }
        }

        var guild = await client.GetGuildAsync(Settings.Guild);

        string description = args.Message.Embeds.First().Description;
        string[] stringArray = description.Split('\n');
        string regexPattern = @"(?<=<@!?)\d+(?=>)";

        ulong user1Id = ulong.Parse(Regex.Match(stringArray[0], regexPattern).Value);
        ulong user2Id = ulong.Parse(Regex.Match(stringArray[1], regexPattern).Value);


        DiscordMember middleMan = await guild.GetMemberAsync(args.User.Id);
        DiscordMember trader1;
        DiscordMember trader2;

        try
        {
            trader1 = await guild.GetMemberAsync(user1Id);
            trader2 = await guild.GetMemberAsync(user2Id);
        }
        catch
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder
                    .BuildResultEmbed(Result.Error, "Пользователя(-ей) нет на сервере!")).AsEphemeral());
            return;
        }

        using (var db = new ApplicationContext())
        {
            if (db.ActiveTrades.Where(x => x.FirstTraderId == trader1.Id
            || x.SecondTraderId == trader1.Id || x.FirstTraderId == trader2.Id || x.SecondTraderId == trader2.Id).Count() > 0)
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder
                    .BuildResultEmbed(Result.Error, "У одного из пользователей уже есть актинвая сделка. " +
                    "Вы не можете принять эту сделку!"))
                    .AsEphemeral());

                await args.Message.DeleteAsync();
                return;
            }
        }

        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder
            .BuildResultEmbed(Result.Success, "Успешно! Будет создан канал на сервере, Вас пинганут!")).AsEphemeral());

        if (args.Guild is null)
        {
            await args.Message.DeleteAsync();
        }
        else
        {
            await args.Message.ModifyAsync(x =>
            {
                x.AddEmbed(args.Message.Embeds.First());
                x.ClearComponents();
                x.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "mm_accepted_button", 
                    $"Принял — {args.User.Username}", disabled: true, 
                    new DiscordComponentEmoji(1164595872663879760)));
            });
        }

        var @everyone = guild.GetRole(guild.Id);

        var overWriteBuilderList = new DiscordOverwriteBuilder[]
        {
            new DiscordOverwriteBuilder(@everyone).Deny(Permissions.All),

            new DiscordOverwriteBuilder(trader1)
            .Allow(Permissions.AccessChannels)
            .Allow(Permissions.SendMessages)
            .Allow(Permissions.ReadMessageHistory)
            .Allow(Permissions.AttachFiles)
            .Allow(Permissions.UseExternalEmojis)
            .Allow(Permissions.UseExternalStickers),

            new DiscordOverwriteBuilder(trader2)
            .Allow(Permissions.AccessChannels)
            .Allow(Permissions.SendMessages)
            .Allow(Permissions.ReadMessageHistory)
            .Allow(Permissions.AttachFiles)
            .Allow(Permissions.UseExternalEmojis)
            .Allow(Permissions.UseExternalStickers),

            new DiscordOverwriteBuilder(middleMan)
            .Allow(Permissions.AccessChannels)
            .Allow(Permissions.SendMessages)
            .Allow(Permissions.ReadMessageHistory)
            .Allow(Permissions.AttachFiles)
            .Allow(Permissions.UseExternalEmojis)
            .Allow(Permissions.UseExternalStickers)

        };

        var tradeChannel = await guild.CreateChannelAsync(
            args.User.Username,
            ChannelType.Text,
            parent: guild.GetChannel(Settings.MiddleMansCategory),
            overwrites: overWriteBuilderList);

        var panelMessage = new DiscordMessageBuilder()
        {
            Content =
            $"{middleMan.Mention}\n" +
            $"{trader1.Mention}\n" +
            $"{trader2.Mention}"
        };
        panelMessage.AddEmbed(new DiscordEmbedBuilder()
        {
            Title = "►ㅤСделка.",
            Color = new DiscordColor("2b2d31"),
            Description = "```● Панель управления сделкой```"
        }
        .WithImageUrl("https://i.imgur.com/tabpqjj.png")
        .WithThumbnail("https://i.imgur.com/IU4AHiA.png")
        .AddField("Гарант", $"``●`` {middleMan.Mention}", true)
        .AddField("Первая сторона", $"``●`` {trader1.Mention}", true)
        .AddField("Вторая сторона", $"``●`` {trader2.Mention}", true));

        panelMessage.AddComponents(

            new DiscordButtonComponent(
                ButtonStyle.Secondary,
                "close_trade_button",
                "Закрыть сделку",
                emoji: new DiscordComponentEmoji(1163784803196346388)),

            new DiscordButtonComponent(
                ButtonStyle.Secondary,
                "decline_trade_button",
                "Отказатья от сделки",
                emoji: new DiscordComponentEmoji(1163784803196346388))
            );

        panelMessage.AddMentions(new List<IMention>
        {
            new UserMention(middleMan.Id),
            new UserMention(trader1.Id),
            new UserMention(trader2.Id)
        });

        using (var db = new ApplicationContext())
        {
            db.ActiveTrades.Add(new ActiveTrade
            {
                Id = Guid.NewGuid(),
                FirstTraderId = trader1.Id,
                MiddleManId = middleMan.Id,
                SecondTraderId = trader2.Id,
            });
            await db.SaveChangesAsync();
        }

        var msg = await tradeChannel.SendMessageAsync(panelMessage);
        await msg.PinAsync();
    }

    [ComponentInteraction("cancel_trade_button")]
    public async Task CancelTrade(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var guild = await client.GetGuildAsync(Settings.Guild);

        string description = args.Message.Embeds.First().Description;
        string[] stringArray = description.Split('\n');
        string regexPattern = @"(?<=<@!?)\d+(?=>)";

        ulong userId = ulong.Parse(Regex.Match(stringArray[0], regexPattern).Value);


        DiscordMember middleMan = await guild.GetMemberAsync(args.User.Id);
        DiscordMember sender;

        try
        {
            sender = await guild.GetMemberAsync(userId);

        }
        catch
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder
                    .BuildResultEmbed(Result.Error, "Пользователя(-ей) нет на сервере!")).AsEphemeral());
            return;
        }

        await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        await args.Message.DeleteAsync();
        try
        {
            await sender.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Title = "Отказ от сделки",
                Color = new DiscordColor("2b2d31"),
                Description = $"**● Гарант {middleMan.Mention} отказался от Вашей заявки на сделку!**",
            }
            .WithThumbnail("https://i.imgur.com/6rX0rEn.png")
            .WithImageUrl("https://i.imgur.com/tabpqjj.png"));

            await args.Interaction.CreateFollowupMessageAsync(
            new DiscordFollowupMessageBuilder()
            .AddEmbed(StyledMessageBuilder
            .BuildResultEmbed(Result.Success, "Отказ отправлен!")).AsEphemeral());
        }
        catch
        {
            await args.Interaction.CreateFollowupMessageAsync(
            new DiscordFollowupMessageBuilder()
            .AddEmbed(StyledMessageBuilder
            .BuildResultEmbed(Result.Success, "Вы отказались от сделки, но сообщение не было доставлено из-за настроек пользователя!")).AsEphemeral());
        }

    }

    [ComponentInteraction("close_trade_button")]
    public async Task CloseTrade(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        string content = args.Message.Content;
        string[] strings = content.Split("\n");
        string regexPattern = @"(?<=<@!?)\d+(?=>)";

        ulong middleManId = ulong.Parse(Regex.Match(strings[0], regexPattern).Value);

        var interactionMember = await args.Guild.GetMemberAsync(args.User.Id);

        DiscordMember member1;
        DiscordMember member2;
        try
        {
            member1 = await args.Guild.GetMemberAsync(ulong.Parse(Regex.Match(strings[1], regexPattern).Value));
            member2 = await args.Guild.GetMemberAsync(ulong.Parse(Regex.Match(strings[2], regexPattern).Value));
        }
        catch
        {
            using (var db = new ApplicationContext())
            {
                var trade = db.ActiveTrades.Where(x => x.MiddleManId == middleManId).FirstOrDefault();
                if (trade is null)
                {
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Сделки не найдено, ошибка в базе данных! " +
                    "Обратитесь к разработчику — cs_shark"))
                    .AsEphemeral());
                    return;
                }

                db.ActiveTrades.Remove(trade);
                await db.SaveChangesAsync();

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success, "Сделка закрыта гарантом! " +
                    "Т.к один из участников не находиться на сервере, отзывы оставлены не будут!", TextType.Bold)));
                return;
            }
        }

        if (interactionMember.Id != middleManId && interactionMember.Roles.ToList()
            .Contains(args.Guild.GetRole(Settings.LowAdminRole)) != true &&
            interactionMember.Permissions.HasPermission(Permissions.Administrator) != true)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Только гарант может закрыть сделку!"))
                .AsEphemeral());
            return;
        }

        using (var db = new ApplicationContext())
        {
            var trade = db.ActiveTrades.Where(x => x.MiddleManId == middleManId).FirstOrDefault();
            if (trade is null)
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Сделки не найдено, ошибка в базе данных! " +
                "Обратитесь к разработчику — cs_shark"))
                .AsEphemeral());
                return;
            }

            db.ActiveTrades.Remove(trade);
            await db.SaveChangesAsync();

            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success, "Сделка закрыта гарантом! " +
                "Канал будет удалён через ``10 секунд``, оставьте отзыв гаранту — форма для отправки отослана в личные сообщения" +
                " участникам сделки.", TextType.Bold)));

            var reviewEmbed = new DiscordEmbedBuilder
            {
                Title = "►ㅤОтзыв.",
                Color = new DiscordColor("2b2d31"),
                Description = $"Оставьте отзыв гаранту <@{middleManId}>."
            }
            .WithThumbnail("https://i.imgur.com/KmgkzBM.png")
            .WithImageUrl("https://i.imgur.com/tabpqjj.png");

            var sendReview = new DiscordButtonComponent(ButtonStyle.Secondary, "send_review_button", "Оставить отзыв",
                emoji: new DiscordComponentEmoji(1163527148787744918));
            try
            {
                await member1.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(reviewEmbed).AddComponents(sendReview));
                await member2.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(reviewEmbed).AddComponents(sendReview));
            }
            catch
            {

            }
            await Task.Delay(10000);
            await args.Channel.DeleteAsync();
        }
    }

    [ComponentInteraction("decline_trade_button")]
    public async Task DeclineTrade(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        string content = args.Message.Content;
        string[] strings = content.Split("\n");
        string regexPattern = @"(?<=<@!?)\d+(?=>)";

        var interactionMember = await args.Guild.GetMemberAsync(args.User.Id);

        ulong middleManId = ulong.Parse(Regex.Match(strings[0], regexPattern).Value);

        if (interactionMember.Id != middleManId && interactionMember.Roles.ToList()
            .Contains(args.Guild.GetRole(Settings.LowAdminRole)) != true &&
            interactionMember.Permissions.HasPermission(Permissions.Administrator) != true)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Только гарант может закрыть сделку!"))
                .AsEphemeral());
            return;
        }

        using (var db = new ApplicationContext())
        {
            var trade = db.ActiveTrades.Where(x => x.MiddleManId == middleManId).FirstOrDefault();
            if (trade is null)
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Сделки не найдено, ошибка в базе данных! " +
                "Обратитесь к разработчику — cs_shark"))
                .AsEphemeral());
                return;
            }

            db.ActiveTrades.Remove(trade);
            await db.SaveChangesAsync();

            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success, "Сделка отклонена гарантом! " +
                "Канал будет удалён через ``10 секунд``, " +
                "вы не сможете оставить свои отзывы — гарант отказался проводить вашу сделку"
                , TextType.Bold)));

            await Task.Delay(10000);
            await args.Channel.DeleteAsync();
        }
    }

    [ComponentInteraction("send_review_button")]
    public async Task SendReview(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var guild = await client.GetGuildAsync(Settings.Guild);
        var description = args.Message.Embeds.First().Description;

        string regexPattern = @"(?<=<@!?)\d+(?=>)";
        ulong middleManId = ulong.Parse(Regex.Match(description, regexPattern).Value);
        var middleMan = await guild.GetMemberAsync(middleManId);

        var modal = new DiscordInteractionResponseBuilder()
        {
            CustomId = "send_review_modal",
            Title = "Оставить отзыв"
        };
        modal.AddComponents(new TextInputComponent("Отзыв", "review_input", "Напишите сюда свой отзыв", style: TextInputStyle.Paragraph));
        modal.AddComponents(new TextInputComponent("Оценка", "grade_input", "От 1 до 5", min_length: 1, max_length: 1));
        await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);

        var modalResult = await client.GetInteractivity().WaitForModalAsync("send_review_modal", args.User, TimeSpan.FromMinutes(10));

        if (modalResult.TimedOut)
        {
            return;
        }

        int number;
        bool isNumber = int.TryParse(modalResult.Result.Values["grade_input"], out number);

        if (isNumber != true || number > 5 || number < 1)
        {
            await modalResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Введите число от 1 до 5!"))
                .AsEphemeral());
            return;
        }

        var starEmoji = DiscordEmoji.FromName(client, ":star:");

        var responseEmbed = new DiscordEmbedBuilder
        {
            Title = "Новый отзыв",
            Color = new DiscordColor("2b2d31"),
            Description = $"**● Пользователь {args.User.Mention} оставил Вам отзыв!**"
        }
        .WithThumbnail("https://i.imgur.com/KmgkzBM.png")
        .WithImageUrl("https://i.imgur.com/tabpqjj.png")
        .AddField("Отзыв", $"```{modalResult.Result.Values["review_input"]}```", true)
        .AddField("Оценка", $"``{modalResult.Result.Values["grade_input"]} {starEmoji}``", true);

        await middleMan.SendMessageAsync(responseEmbed);
        await modalResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success, "Отправлено успешно!"))
                .AsEphemeral());

        using (var db = new ApplicationContext())
        {
            var mm = db.MiddleMans.Include(x => x.Reviews).Where(x => x.Id == middleMan.Id).First();
            var review = new Review
            {
                UserId = middleMan.Id,
                Content = modalResult.Result.Values["review_input"],
                Rate = number
            };
            mm.Reviews.Add(review);
            db.Reviews.Update(review);

            await db.SaveChangesAsync();
        }
        await args.Message.DeleteAsync();
    }
}
