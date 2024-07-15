using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus;
using WinterFamily.Main.Common.Attributes;
using DSharpPlus.Interactivity.Extensions;
using WinterFamily.Main.Persistence;
using WinterFamily.Main.Persistence.Models;
using WinterFamily.Main.Utils.Discord;
using System.Text.RegularExpressions;

namespace WinterFamily.Main.Application.Handlers;

internal class TicketsInteractionHandler
{

    [ComponentInteraction("ask_question_button")]
    public async Task AskQuestionClick(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        using (var db = new ApplicationContext())
        {
            var user = db.Cooldowns.Find(args.User.Id);
            if (user != null)
            {
                if (user.QuestionTimeStamp > DateTime.UtcNow)
                {
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .AddEmbed(
                            StyledMessageBuilder.BuildResultEmbed(Result.Error,
                            $"Вы не можете сново задать вопроса до <t:{GetUnixTimeStamp(user.QuestionTimeStamp)}>!", TextType.Bold))
                        .AsEphemeral());
                    return;
                }
            }
        }

        var modalBuilder = new DiscordInteractionResponseBuilder()
                    .WithTitle($"Вопрос по серверу")
                    .WithCustomId("ask_question_modal")
                    .AddComponents(
                        new TextInputComponent("Вопрос",
                        "question_input",
                        "Опишите свой вопрос как можно более подробно!",
                        style: TextInputStyle.Paragraph));
        await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modalBuilder);
    }


    [ModalSubmitted("ask_question_modal")]
    public async Task AskQuestionSubmit(DiscordClient client, ModalSubmitEventArgs args)
    {

        var questionEmbed = new DiscordEmbedBuilder()
        {
            Title = "Новый вопрос!",
            Description = $"От пользователя {args.Interaction.User.Mention}",
            Color = new DiscordColor("2b2d31"),
        };
        questionEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        questionEmbed.WithThumbnail("https://i.imgur.com/KmgkzBM.png");
        questionEmbed.AddField("Вопрос:", $"```{args.Values["question_input"]}```");

        var targetChannel = args.Interaction.Guild.GetChannel(Settings.QuestionsChannel);


        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success, "Вы успешно задали вопрос, ожидайте ответа в личных сообщениях!"))
            .AsEphemeral());

        var button = new DiscordButtonComponent(ButtonStyle.Secondary, "question_response_button", "Ответить",
            emoji: new DiscordComponentEmoji(1163527791581605950));

        using (var db = new ApplicationContext())
        {
            var user = db.Cooldowns.Find(args.Interaction.User.Id);
            if (user != null)
            {
                user.QuestionTimeStamp = DateTime.UtcNow.AddDays(1);
                db.SaveChanges();
            }
            else
            {
                db.Cooldowns.Add(new Cooldown
                {
                    UserId = args.Interaction.User.Id,
                    QuestionTimeStamp = DateTime.UtcNow.AddDays(1),
                    ComplaintTimeStamp = DateTime.UnixEpoch
                });
                db.SaveChanges();
            }
        }
        await targetChannel.SendMessageAsync(new DiscordMessageBuilder()
            .AddEmbed(questionEmbed)
            .AddComponents(button));

    }

    [ComponentInteraction("user_complaint_button")]
    public async Task ComplaintClick(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var complaintEmbed = new DiscordEmbedBuilder
        {
            Title = "Жалоба на пользователя",
            Description = "Напишите никнейм пользователя ниже...",
        };

        var userSelect = new DiscordMentionableSelectComponent("user_complaint_select", "Поиск...");
        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(complaintEmbed)
            .AddComponents(userSelect)
            .AsEphemeral());
    }

    [ComponentInteraction("user_complaint_select")]
    public async Task ComplaintSelect(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        using (var db = new ApplicationContext())
        {
            var user = db.Cooldowns.Find(args.User.Id);
            if (user != null)
            {
                if (user.ComplaintTimeStamp > DateTime.UtcNow)
                {
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                        $"Вы не можете сново подать жалобу до <t:{GetUnixTimeStamp(user.ComplaintTimeStamp)}>!", TextType.Bold))
                        .AsEphemeral());
                    return;
                }
            }
        }

        var modalBuilder = new DiscordInteractionResponseBuilder()
            .WithTitle($"Жалоба на пользователя")
            .WithCustomId("user_complaint_modal")
            .AddComponents(
                new TextInputComponent("Описание",
                "user_complaint_description",
                "Опишите подробно свою проблему\n" +
                "Упоминание каналов — <#ID>\n" +
                "Упоминание пользователей — <@ID>",
                style: TextInputStyle.Paragraph))
            .AddComponents(
            new TextInputComponent("Доказательство",
            "user_complaint_proof",
            "Ссылка c доказательством на imgur или youtube"));
        await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modalBuilder);

        var modalResult = await client.GetInteractivity().WaitForModalAsync("user_complaint_modal", args.User,
            TimeSpan.FromMinutes(10));

        if (modalResult.TimedOut)
        {
            return;
        }

        var userId = ulong.Parse(args.Values.First());

        var targetChannel = await client.GetChannelAsync(Settings.ComplaintsChannel);
        var complaintEmbed = new DiscordEmbedBuilder
        {
            Title = "Новая жалоба!",
            Description = $"**Жалоба на пользователя: <@{userId}>**\n " +
            $"**От {modalResult.Result.Interaction.User.Mention}**",
            Color = new DiscordColor("2b2d31")
        };
        complaintEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        complaintEmbed.WithThumbnail("https://i.imgur.com/KmgkzBM.png");
        complaintEmbed.AddField("Описание:", modalResult.Result.Values["user_complaint_description"], true);
        complaintEmbed.AddField("Доказательство:", modalResult.Result.Values["user_complaint_proof"], true);

        await modalResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success, "Вы успешно пожаловались на пользователя!"))
            .AsEphemeral());
        var accept = new DiscordButtonComponent(ButtonStyle.Secondary, "complaint_accept_button", "Принять",
            emoji: new DiscordComponentEmoji(1163537815666163712));
        var decline = new DiscordButtonComponent(ButtonStyle.Secondary, "complaint_decline_button", "Отклонить",
            emoji: new DiscordComponentEmoji(1163784803196346388));
        using (var db = new ApplicationContext())
        {
            var bdUser = db.Cooldowns.Find(args.Interaction.User.Id);
            if (bdUser != null)
            {
                bdUser.ComplaintTimeStamp = DateTime.UtcNow.AddDays(1);
                db.SaveChanges();
            }
            else
            {
                db.Cooldowns.Add(new Cooldown
                {
                    UserId = args.Interaction.User.Id,
                    ComplaintTimeStamp = DateTime.UtcNow.AddDays(1),
                    QuestionTimeStamp = DateTime.UnixEpoch
                });
                db.SaveChanges();
            }
        }
        await targetChannel.SendMessageAsync(new DiscordMessageBuilder()
            .AddEmbed(complaintEmbed)
            .AddComponents(accept, decline));
    }

    [ComponentInteraction("complaint_accept_button")]
    public async Task OnComplaintAccept(DiscordClient client, ComponentInteractionCreateEventArgs args) 
    {
        var descriptionStrings = args.Message.Embeds.First().Description.Split("\n");
        var authorString = descriptionStrings[1];
        var complaintString = descriptionStrings[0];
        ulong authorId;
        ulong complaintTargetId;
        string regexPattern = @"(?<=<@!?)\d+(?=>)";
        var parseResult1 = ulong.TryParse(Regex.Match(authorString, regexPattern).Value, out authorId);
        var parseResult2 = ulong.TryParse(Regex.Match(complaintString, regexPattern).Value, out complaintTargetId);

        if (parseResult1 == false || parseResult2 == false)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Неккоректное ID!"))
            .AsEphemeral());
            return;
        }

        DiscordMember member;
        try
        {
            member = await args.Guild.GetMemberAsync(authorId);

        }
        catch
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Данного пользователя нет на сервере!"))
            .AsEphemeral());
            return;
        }

        var acceptEmoji = DiscordEmoji.FromName(client, ":white_check_mark:");

        var responseEmbed = new DiscordEmbedBuilder
        {
            Title = "Ваша жалоба рассмотрена",
            Description = $"**Ответ: {acceptEmoji} Ваша жалоба принята, пользователь получит выговор\n\n" +
            $"Жалоба на: <@{complaintTargetId}>\n " +
            $"Ответил: <@{args.User.Id}>**\n ",
            Color = new DiscordColor("2b2d31")
        };
        responseEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        responseEmbed.WithThumbnail("https://i.imgur.com/KmgkzBM.png");

        try
        {
            await member.SendMessageAsync(new DiscordMessageBuilder()
            .AddEmbed(responseEmbed));
        }
        catch
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                "Не удалось отправить пользователю ответ на вопрос в личные сообщения!"))
                .AsEphemeral());
            return;
        }

        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success, "Отправлено успешно!"))
            .AsEphemeral());

        await args.Message.ModifyAsync(x =>
        {

            x.AddEmbed(new DiscordEmbedBuilder(args.Message.Embeds.First()).WithDescription(
                args.Message.Embeds.First().Description +
                $"\n **Принял: {args.Interaction.User.Mention}**"));
            x.ClearComponents();
            x.AddComponents(
                new DiscordButtonComponent(ButtonStyle.Secondary, "complaint_accept_button", "Принять",
                emoji: new DiscordComponentEmoji(1163537815666163712), disabled: true), 
                new DiscordButtonComponent(ButtonStyle.Secondary, "complaint_decline_button", "Отклонить",
                emoji: new DiscordComponentEmoji(1163784803196346388), disabled: true));
        });
    }

    [ComponentInteraction("complaint_decline_button")]
    public async Task OnComplaintDecline(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var description = args.Message.Embeds.First().Description.Split("\n");
        ulong authorId;
        ulong complaintTargetId;
        string regexPattern = @"(?<=<@!?)\d+(?=>)";
        var parseResult1 = ulong.TryParse(Regex.Match(description[1], regexPattern).Value, out authorId);
        var parseResult2 = ulong.TryParse(Regex.Match(description[0], regexPattern).Value, out complaintTargetId);

        if (parseResult1 == false || parseResult2 == false)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Неккоректное ID!"))
            .AsEphemeral());
            return;
        }

        DiscordMember member;
        try
        {
            member = await args.Guild.GetMemberAsync(authorId);

        }
        catch
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Данного пользователя нет на сервере!"))
            .AsEphemeral());
            return;
        }

        var declineEmoji = DiscordEmoji.FromName(client, ":x:");

        var responseEmbed = new DiscordEmbedBuilder
        {
            Title = "Ваша жалоба рассмотрена",
            Description = $"**Ответ: {declineEmoji} Ваша жалоба отклонена, пользователь не будет наказан\n\n" +
            $"Жалоба на: <@{complaintTargetId}>\n " +
            $"Ответил: <@{args.User.Id}>**\n ",
            Color = new DiscordColor("2b2d31")
        };
        responseEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        responseEmbed.WithThumbnail("https://i.imgur.com/KmgkzBM.png");

        try
        {
            await member.SendMessageAsync(new DiscordMessageBuilder()
            .AddEmbed(responseEmbed));
        }
        catch
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                "Не удалось отправить пользователю ответ на вопрос в личные сообщения!"))
                .AsEphemeral());
            return;
        }

        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success, "Отправлено успешно!"))
            .AsEphemeral());

        await args.Message.ModifyAsync(x =>
        {

            x.AddEmbed(new DiscordEmbedBuilder(args.Message.Embeds.First()).WithDescription(
                args.Message.Embeds.First().Description +
                $"\n **Отклонил: {args.Interaction.User.Mention}**"));
            x.ClearComponents();
            x.AddComponents(
                new DiscordButtonComponent(ButtonStyle.Secondary, "complaint_accept_button", "Принять",
                emoji: new DiscordComponentEmoji(1163537815666163712), disabled: true),
                new DiscordButtonComponent(ButtonStyle.Secondary, "complaint_decline_button", "Отклонить",
                emoji: new DiscordComponentEmoji(1163784803196346388), disabled: true));
        });
    }


    [ComponentInteraction("question_response_button")]
    public async Task OnQuestionResponse(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var description = args.Message.Embeds.First().Description;
        var question = args.Message.Embeds.First().Fields.First().Value.Replace("```", "");
        string regexPattern = @"(?<=<@!?)\d+(?=>)";
        ulong userId;
        var parseResult = ulong.TryParse(Regex.Match(description, regexPattern).Value, out userId);

        if (parseResult == false)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Неккоректное ID!"))
            .AsEphemeral());
            return;
        }

        DiscordMember member;
        try
        {
            member = await args.Guild.GetMemberAsync(userId);

        }
        catch
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error, "Данного пользователя нет на сервере!"))
            .AsEphemeral());
            return;
        }

        var modal = new DiscordInteractionResponseBuilder()
        {
            CustomId = "question_response_modal",
            Title = "Ответ на вопрос"
        };
        modal.AddComponents(
            new TextInputComponent("Ответ на вопрос",
            "answer_input",
            "Распишите подробно",
            style: TextInputStyle.Paragraph));

        await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);

        var modalResult = await client.GetInteractivity().WaitForModalAsync("question_response_modal", args.User,
            TimeSpan.FromMinutes(10));

        if (modalResult.TimedOut)
        {
            return;
        }

        var responseEmbed = new DiscordEmbedBuilder
        {
            Title = "Ответ на вопрос",
            Description = $"**На ваш вопрос: \"{question}\"\n " +
            $"Ответил: <@{args.User.Id}>**\n ",
            Color = new DiscordColor("2b2d31")
        };
        responseEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        responseEmbed.WithThumbnail("https://i.imgur.com/KmgkzBM.png");
        responseEmbed.AddField("Ответ", $"```{modalResult.Result.Values["answer_input"]}```");

        try
        {
            await member.SendMessageAsync(new DiscordMessageBuilder()
            .AddEmbed(responseEmbed));
        }
        catch
        {
            await modalResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                "Не удалось отправить пользователю ответ на вопрос в личные сообщения!"))
                .AsEphemeral());
            return;
        }

        await modalResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
               new DiscordInteractionResponseBuilder()
               .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
               "Ответ отправлен!"))
               .AsEphemeral());

        await args.Message.ModifyAsync(x =>
        {

            x.AddEmbed(new DiscordEmbedBuilder(args.Message.Embeds.First()).WithDescription(
                args.Message.Embeds.First().Description +
                $"\n **Ответил {modalResult.Result.Interaction.User.Mention}**")
                .AddField("Ответ",
                $"```{modalResult.Result.Values["answer_input"]}```"));
            x.ClearComponents();
            x.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "question_response_button", "Ответить",
            emoji: new DiscordComponentEmoji(1163527791581605950), disabled: true));
        });
    }

    private int GetUnixTimeStamp(DateTime dateTime)
    {
        return (int)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}
