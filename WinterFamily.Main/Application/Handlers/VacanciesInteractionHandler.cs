using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;
using WinterFamily.Main.Common.Attributes;
using WinterFamily.Main.Persistence;
using WinterFamily.Main.Persistence.Models;
using WinterFamily.Main.Utils.Discord;

namespace WinterFamily.Main.Application.Handlers;

internal class VacanciesInteractionHandler
{

    [ComponentInteraction("vacancies_select")]
    public async Task VacanciesSelect(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var member = await args.Guild.GetMemberAsync(args.User.Id);
        if (member.Roles.Contains(args.Guild.GetRole(Settings.BlackListRole))) 
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder
                    .BuildResultEmbed(Result.Error, "Вы в \"ЧС Набор\" и более не сможете подать заявку в стафф"))
                    .AsEphemeral());
            return;
        }

        var moderatorEmbed = new DiscordEmbedBuilder()
        {
            Title = "►ㅤВакансия: Модератор",
            Description = "```● В обязанности входит слежка за чатом и наказание участников, нарушающих правила. " +
            "Если Вы готовы проявить себя в роли модератора — просим оставить Вашу заявку.```",
            Color = new DiscordColor("2b2d31")
        };
        moderatorEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        moderatorEmbed.WithThumbnail("https://i.imgur.com/gQ1tV7U.png");

        var eventerEmbed = new DiscordEmbedBuilder()
        {
            Title = "►ㅤВакансия: Ивентер",
            Description = "```● В обязанности входит проведение мероприятий на сервере. " +
            "Если Вы активный участник сервера, любите общаться и хотите проводить ивенты — просим оставить Вашу заявку.```",
            Color = new DiscordColor("2b2d31")
        };
        eventerEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
        eventerEmbed.WithThumbnail("https://i.imgur.com/AcMF1BE.png");

        var moderatorButton = new DiscordButtonComponent(ButtonStyle.Secondary, "moderator_request_button", "Подать заявку",
            emoji: new DiscordComponentEmoji(1163537815666163712));

        var eventerButton = new DiscordButtonComponent(ButtonStyle.Secondary, "eventer_request_button", "Подать заявку",
            emoji: new DiscordComponentEmoji(1163537815666163712));
        var value = args.Values.First();
        switch (value)
        {
            case "moderator_value":
                if (IsOpenedVacancy("moderator_value"))
                {
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(moderatorEmbed)
                    .AddComponents(moderatorButton)
                    .AsEphemeral());
                    return;
                }
                await SendClosedVacancyNotification(args);
                break;

            case "eventer_value":
                if (IsOpenedVacancy("eventer_value"))
                {
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(eventerEmbed)
                    .AddComponents(eventerButton)
                    .AsEphemeral());
                    return;
                }
                await SendClosedVacancyNotification(args);
                break;
        }

    }

    [ComponentInteraction("moderator_request_button")]
    public async Task ModeratorButtonClick(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {

        using (var db = new ApplicationContext())
        {
            var dbUser = db.SubmittedUsers.Find(args.User.Id);
            if (dbUser != null)
            {
                if (dbUser.ModeratorSubmitted)
                {
                    var responseEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Ошибка!",
                        Description = $"```● Вы уже подавали заявку на модератора! Ожидайте пока с Вами свяжется администрация.```",
                        Color = new DiscordColor("2b2d31")
                    }
                    .WithThumbnail("https://i.imgur.com/6rX0rEn.png")
                    .WithImageUrl("https://i.imgur.com/tabpqjj.png");                  
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .AddEmbed(responseEmbed).AsEphemeral());
                    return;
                }
            }
        }

        var modal = new DiscordInteractionResponseBuilder().WithCustomId("moderator_request_modal")
            .WithTitle("Заявка на модератора");
        modal.AddComponents(
            new TextInputComponent("Ваш возраст", "moderator_age_input"));
        modal.AddComponents(
            new TextInputComponent("Расскажите о своём опыте на этой должности",
            "moderator_exp_input",
            style: TextInputStyle.Paragraph));
        modal.AddComponents(
            new TextInputComponent("Как давно Вы на сервере? Ваш онлайн.",
            "moderator_reason_input",
            style: TextInputStyle.Paragraph));
        modal.AddComponents(
            new TextInputComponent("Часовой пояс. Прайм-тайм. Сколько уделите?", "moderator_rules_input"));
        modal.AddComponents(
            new TextInputComponent("Оценка стрессоустойчивости и причина подачи?",
            "moderator_ask_input",
            style: TextInputStyle.Paragraph));

        if (IsOpenedVacancy("moderator_value") != true)
        {
            await SendClosedVacancyNotification(args);
            return;
        }
        await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }

    [ComponentInteraction("eventer_request_button")]
    public async Task EventerButtonClick(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        using (var db = new ApplicationContext())
        {
            var dbUser = db.SubmittedUsers.Find(args.User.Id);
            if (dbUser != null)
            {
                if (dbUser.EventerSubmitted)
                {
                    var responseEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Ошибка!",
                        Description = $"```● Вы уже подавали заявку на ивентера! Ожидайте пока с Вами свяжется администрация.```",
                        Color = new DiscordColor("2b2d31")
                    }
                    .WithThumbnail("https://i.imgur.com/6rX0rEn.png")
                    .WithImageUrl("https://i.imgur.com/tabpqjj.png");
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .AddEmbed(responseEmbed).AsEphemeral());
                    return;
                }
            }
        }

        var modal = new DiscordInteractionResponseBuilder()
            .WithCustomId("eventer_request_modal")
            .WithTitle("Заявка на ивентера");
        modal.AddComponents(
            new TextInputComponent("Ваш возраст", "eventer_age_input"));
        modal.AddComponents(
            new TextInputComponent("Расскажите о своём опыте на этой должности",
            "eventer_exp_input",
            style: TextInputStyle.Paragraph));  
        modal.AddComponents(
            new TextInputComponent("Как давно Вы на сервере? Ваш онлайн.",
            "eventer_reason_input",
            style: TextInputStyle.Paragraph));
        modal.AddComponents(new TextInputComponent("Какие ивенты вы планируете делать?",
            "eventer_events_input",
            style: TextInputStyle.Paragraph));

        if (IsOpenedVacancy("eventer_value") != true)
        {
            await SendClosedVacancyNotification(args);
            return;
        }
        await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }

    [ModalSubmitted("moderator_request_modal")]
    public async Task ModeratorModalSubmitted(DiscordClient client, ModalSubmitEventArgs args)
    {
        var recruitForm = new DiscordEmbedBuilder
        {
            Title = "Новая заявка на модератора!",
            Description = $"От участника {args.Interaction.User.Mention}\n**Форма заявки:\n**",
            Color = new DiscordColor("2b2d31")
        }
        .AddField("Ваш возраст", $"```{args.Values["moderator_age_input"]}```")
        .AddField("Расскажите о своём опыте на этой должности", $"```{args.Values["moderator_exp_input"]}```")
        .AddField("Как давно Вы на сервере? Ваш онлайн.", $"```{args.Values["moderator_reason_input"]}```")
        .AddField("Часовой пояс. Прайм-тайм. Сколько уделите?", $"```{args.Values["moderator_rules_input"]}```")
        .AddField("Оценка стрессоустойчивости и причина подачи?", $"```{args.Values["moderator_ask_input"]}```")
        .WithThumbnail("https://i.imgur.com/gQ1tV7U.png")
        .WithImageUrl("https://i.imgur.com/tabpqjj.png");

        var acceptButton = new DiscordButtonComponent(ButtonStyle.Secondary, "accept_vacancy_button", "Принять",
            emoji: new DiscordComponentEmoji(1163537815666163712));
        var declineButton = new DiscordButtonComponent(ButtonStyle.Secondary, "decline_vacancy_button", "Отклонить",
            emoji: new DiscordComponentEmoji(1163784803196346388));
        var blackListButton = new DiscordButtonComponent(ButtonStyle.Secondary, "blacklist_vacancy_button", "ЧС Набор",
            emoji: new DiscordComponentEmoji(1163786478640431156));
        var responseEmbed = new DiscordEmbedBuilder
        {
            Title = "Успешно!",
            Description = $"```● Вы успешно подали заявку на модератора! Ожидайте пока с Вами свяжется администрация.```",
            Color = new DiscordColor("2b2d31")
        }
        .WithThumbnail("https://i.imgur.com/flYWSSl.png")
        .WithImageUrl("https://i.imgur.com/tabpqjj.png");

        var targetChannel = args.Interaction.Guild.GetChannel(Settings.ModeratorChannel);
        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(responseEmbed)
            .AsEphemeral());

        using (var db = new ApplicationContext())
        {
            var dbUser = db.SubmittedUsers.Find(args.Interaction.User.Id);
            if (dbUser != null)
            {
                dbUser.ModeratorSubmitted = true;
            }
            else
            {
                var newUser = new SubmittedUser()
                {
                    Id = args.Interaction.User.Id,
                    ModeratorSubmitted = true,
                };
                db.SubmittedUsers.Add(newUser);
            }
            db.SaveChanges();
        }
        await targetChannel.SendMessageAsync(new DiscordMessageBuilder()
            .AddEmbed(recruitForm)
            .AddComponents(acceptButton, declineButton, blackListButton));

    }

    [ModalSubmitted("eventer_request_modal")]
    public async Task EventerModalSubmitted(DiscordClient client, ModalSubmitEventArgs args)
    {
        var recruitForm = new DiscordEmbedBuilder
        {
            Title = "Новая заявка на ивентера!",
            Description = $"От участника {args.Interaction.User.Mention}\n**Форма заявки:\n**",
            Color = new DiscordColor("2b2d31")
        }
        .AddField("Ваш возраст", $"```{args.Values["eventer_age_input"]}```")
        .AddField("Расскажите о своём опыте на этой должности", $"```{args.Values["eventer_exp_input"]}```")
        .AddField("Как давно Вы на сервере? Ваш онлайн.", $"```{args.Values["eventer_reason_input"]}```")
        .AddField("Какие ивенты вы планируете делать?", $"```{args.Values["eventer_events_input"]}```")
        .WithThumbnail("https://i.imgur.com/AcMF1BE.png")
        .WithImageUrl("https://i.imgur.com/tabpqjj.png");
        var acceptButton = new DiscordButtonComponent(ButtonStyle.Secondary, "accept_vacancy_button", "Принять",
            emoji: new DiscordComponentEmoji(1163537815666163712));
        var declineButton = new DiscordButtonComponent(ButtonStyle.Secondary, "decline_vacancy_button", "Отклонить",
            emoji: new DiscordComponentEmoji(1163784803196346388));
        var blackListButton = new DiscordButtonComponent(ButtonStyle.Secondary, "blacklist_vacancy_button", "ЧС Набор",
            emoji: new DiscordComponentEmoji(1163786478640431156));

        var responseEmbed = new DiscordEmbedBuilder
        {
            Title = "Успешно!",
            Description = $"```● Вы успешно подали заявку на ивентера! Ожидайте пока с Вами свяжется администрация.```",
            Color = new DiscordColor("2b2d31")
        }
        .WithThumbnail("https://i.imgur.com/flYWSSl.png")
        .WithImageUrl("https://i.imgur.com/tabpqjj.png");

        var targetChannel = args.Interaction.Guild.GetChannel(Settings.EventerChannel);
        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(responseEmbed)
            .AsEphemeral());

        using (var db = new ApplicationContext())
        {
            var dbUser = db.SubmittedUsers.Find(args.Interaction.User.Id);
            if (dbUser != null)
            {
                dbUser.EventerSubmitted = true;
            }
            else
            {
                var newUser = new SubmittedUser()
                {
                    Id = args.Interaction.User.Id,
                    EventerSubmitted = true,
                };
                db.SubmittedUsers.Add(newUser);
            }
            db.SaveChanges();
        }

        await targetChannel.SendMessageAsync(new DiscordMessageBuilder()
            .AddEmbed(recruitForm)
            .AddComponents(acceptButton, declineButton, blackListButton));
    }

    [ComponentInteraction("accept_vacancy_button")]
    public async Task AcceptVacancy(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var description = args.Message.Embeds.First().Description;
        var thumbnail = args.Message.Embeds.First().Thumbnail;
        var imageUrl = args.Message.Embeds.First().Image.Url;
        var title = args.Message.Embeds.First().Title;
        var color = args.Message.Embeds.First().Color;
        var fields = args.Message.Embeds.First().Fields;
        var vacancy = title.Substring(12).Substring(0, title.Substring(12).Length - 1);

        ulong userId;
        string regexPattern = @"(?<=<@!?)\d+(?=>)";
        var parseResult = ulong.TryParse(Regex.Match(description, regexPattern).Value, out userId);
        if (parseResult)
        {
            DiscordMember member;

            try
            {
                member = await args.Guild.GetMemberAsync(userId);
            }
            catch
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                    $"Пользователь не находиться на сервере!"))
                    .AsEphemeral());
                return;
            }

            switch (vacancy) 
            {
                case " на ивентера":
                    await member.GrantRoleAsync(args.Guild.GetRole(Settings.EventerRole));
                    break;
                case " на модератора":
                    await member.GrantRoleAsync(args.Guild.GetRole(Settings.StaffRole));
                    break;
            }
            

            var acceptEmbed = new DiscordEmbedBuilder()
            {
                Color = new DiscordColor("2b2d31"),
                Title = $"Ваша заявка{vacancy} принята!",
                Description = $"**{member.Mention}, Ваша заявка была успешно принята участником {args.Interaction.User.Mention}!\n" +
                $"В скором времени с Вами свяжутся.**"
            }
            .WithThumbnail(thumbnail.Url.ToString())
            .WithImageUrl("https://i.imgur.com/tabpqjj.png");

            try
            {
                var dmChannel = await member.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(acceptEmbed));
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
                $"Пользователь с ником {member.Username} успешно принят в стафф"))
                .AsEphemeral());
            } 
            catch
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
                $"По неизвестной причине пользователю {member.Username} невозможно прислать уведомление, но он успешно принят в стафф"))
                .AsEphemeral());
            }
            
            var editedEmbed = new DiscordEmbedBuilder
            {
                Title = $"Заявка{vacancy} была принята!",
                Description = $"** Пользователь {member.Mention} принят в стафф. \nЗаявку принял {args.Interaction.User.Mention}. **",
                Color = color,
            }
            .WithThumbnail(thumbnail.Url.ToString())
            .WithImageUrl(imageUrl.ToString());

            foreach (var field in fields)
            {
                editedEmbed.AddField(field.Name, field.Value);
            }

            var editedMessage = new DiscordMessageBuilder().AddEmbed(editedEmbed).AddComponents(
                new DiscordButtonComponent(ButtonStyle.Secondary, "accept_vacancy_button", "Принять",
                emoji: new DiscordComponentEmoji(1163537815666163712), disabled: true),

                new DiscordButtonComponent(ButtonStyle.Secondary, "decline_vacancy_button", "Отклонить",
                emoji: new DiscordComponentEmoji(1163784803196346388), disabled: true),

                new DiscordButtonComponent(ButtonStyle.Secondary, "blacklist_vacancy_button", "ЧС Набор",
                emoji: new DiscordComponentEmoji(1163786478640431156), disabled: true));


            await args.Message.ModifyAsync(editedMessage);
        }
        else
        {
            var embed = StyledMessageBuilder.BuildResultEmbed(Result.Error, "Неудачный парсинг userId в методе AcceptVacancy.\n" +
                "Обратитесь к разработчику — cs_shark");
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral());
        }
    }

    [ComponentInteraction("decline_vacancy_button")]
    public async Task DeclineVacancy(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var description = args.Message.Embeds.First().Description;
        var thumbnail = args.Message.Embeds.First().Thumbnail;
        var imageUrl = args.Message.Embeds.First().Image.Url;
        var title = args.Message.Embeds.First().Title;
        var color = args.Message.Embeds.First().Color;
        var fields = args.Message.Embeds.First().Fields;

        ulong userId;
        string regexPattern = @"(?<=<@!?)\d+(?=>)";
        var parseResult = ulong.TryParse(Regex.Match(description, regexPattern).Value, out userId);
        if (parseResult)
        {
            DiscordMember member;

            try
            {
                member = await args.Guild.GetMemberAsync(userId);
            }
            catch
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                    $"Пользователь не находиться на сервере!"))
                    .AsEphemeral());
                return;
            }

            var acceptEmbed = new DiscordEmbedBuilder()
            {
                Color = new DiscordColor("2b2d31"),
                Title = "Ваша заявка на вакансию отклонена!",
                Description = $"**{member.Mention}, Ваша заявка была отклонена участником {args.Interaction.User.Mention}!\n" +
                $"Попытайте свою удачу в следующем наборе.**"
            }
            .WithThumbnail(thumbnail.Url.ToString())
            .WithImageUrl("https://i.imgur.com/tabpqjj.png");

            try
            {
                var dmChannel = await member.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(acceptEmbed));
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
                $"Заявка пользователя с ником {member.Username} была отклонена!"))
                .AsEphemeral());
            }
            catch
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
                $"По неизвестной причине пользователю {member.Username} невозможно прислать уведомление, но его заявка так же отклонена"))
                .AsEphemeral());
            }

            var editedEmbed = new DiscordEmbedBuilder
            {
                Title = $"Заявка{title.Substring(12).Substring(0, title.Substring(12).Length - 1)} была отклонена!",
                Description = $"** Пользователь {member.Mention} не принят в стафф. \nЗаявку отклонил {args.Interaction.User.Mention}. **",
                Color = color,
            }
            .WithThumbnail(thumbnail.Url.ToString())
            .WithImageUrl(imageUrl.ToString());

            foreach (var field in fields)
            {
                editedEmbed.AddField(field.Name, field.Value);
            }

            var editedMessage = new DiscordMessageBuilder().AddEmbed(editedEmbed).AddComponents(
                new DiscordButtonComponent(ButtonStyle.Secondary, "accept_vacancy_button", "Принять",
                emoji: new DiscordComponentEmoji(1163537815666163712), disabled: true),

                new DiscordButtonComponent(ButtonStyle.Secondary, "decline_vacancy_button", "Отклонить",
                emoji: new DiscordComponentEmoji(1163784803196346388), disabled: true),

                new DiscordButtonComponent(ButtonStyle.Secondary, "blacklist_vacancy_button", "ЧС Набор",
                emoji: new DiscordComponentEmoji(1163786478640431156), disabled: true));


            await args.Message.ModifyAsync(editedMessage);
        }
        else
        {
            var embed = StyledMessageBuilder.BuildResultEmbed(Result.Error, "Неудачный парсинг userId в методе AcceptVacancy.\n" +
                "Обратитесь к разработчику — cs_shark");
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral());
        }
    }

    [ComponentInteraction("blacklist_vacancy_button")]
    public async Task BlacklistVacancy(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var description = args.Message.Embeds.First().Description;
        var thumbnail = args.Message.Embeds.First().Thumbnail;
        var imageUrl = args.Message.Embeds.First().Image.Url;
        var title = args.Message.Embeds.First().Title;
        var color = args.Message.Embeds.First().Color;
        var fields = args.Message.Embeds.First().Fields;

        ulong userId;
        string regexPattern = @"(?<=<@!?)\d+(?=>)";
        var parseResult = ulong.TryParse(Regex.Match(description, regexPattern).Value, out userId);
        if (parseResult)
        {
            DiscordMember member;

            try
            {
                member = await args.Guild.GetMemberAsync(userId);
            }
            catch
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Error,
                    $"Пользователь не находиться на сервере!"))
                    .AsEphemeral());
                return;
            }
            await member.GrantRoleAsync(args.Guild.GetRole(Settings.BlackListRole));

            var acceptEmbed = new DiscordEmbedBuilder()
            {
                Color = new DiscordColor("2b2d31"),
                Title = "Вы попали в черный список!",
                Description = $"**{member.Mention}, Вас внес пользователь {args.Interaction.User.Mention}!\n" +
                $"Более вы не сможете подать заявку.**"
            }
            .WithThumbnail("https://i.imgur.com/YCf9rak.png")
            .WithImageUrl("https://i.imgur.com/tabpqjj.png");

            try
            {
                var dmChannel = await member.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(acceptEmbed));
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
                $"Пользователь с ником {member.Username} теперь в находиться в статусе 'ЧС Набор'!!"))
                .AsEphemeral());
            }
            catch
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success,
                $"По неизвестной причине пользователю {member.Username} невозможно прислать уведомление, но ему также установлен статус 'ЧС Наборы'"))
                .AsEphemeral());
            }

            var editedEmbed = new DiscordEmbedBuilder
            {
                Title = $"Заявка{title.Substring(12).Substring(0, title.Substring(12).Length - 1)} была отклонена по причине 'ЧС Набор'!",
                Description = $"** Пользователь {member.Mention} внесен в ЧС Набор. \nВнес: {args.Interaction.User.Mention}. **",
                Color = color,
            }
            .WithThumbnail(thumbnail.Url.ToString())
            .WithImageUrl(imageUrl.ToString());

            foreach (var field in fields)
            {
                editedEmbed.AddField(field.Name, field.Value);
            }

            var editedMessage = new DiscordMessageBuilder().AddEmbed(editedEmbed).AddComponents(
                new DiscordButtonComponent(ButtonStyle.Secondary, "accept_vacancy_button", "Принять",
                emoji: new DiscordComponentEmoji(1163537815666163712), disabled: true),

                new DiscordButtonComponent(ButtonStyle.Secondary, "decline_vacancy_button", "Отклонить",
                emoji: new DiscordComponentEmoji(1163784803196346388), disabled: true),

                new DiscordButtonComponent(ButtonStyle.Secondary, "blacklist_vacancy_button", "ЧС Набор",
                emoji: new DiscordComponentEmoji(1163786478640431156), disabled: true));


            await args.Message.ModifyAsync(editedMessage);
        }
        else
        {
            Console.WriteLine(description.Substring(12 + 4).Substring(0, description.IndexOf('\n') - 1));
            var embed = StyledMessageBuilder.BuildResultEmbed(Result.Error, "Неудачный парсинг userId в методе AcceptVacancy.\n" +
                "Обратитесь к разработчику — cs_shark");
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral());
        }
    }

    private async Task SendClosedVacancyNotification(ComponentInteractionCreateEventArgs args)
    {
        var embed = StyledMessageBuilder.BuildResultEmbed(Result.Error, "Набор на данную вакансию закрыт!");
        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AddEmbed(embed)
            .AsEphemeral());
    }
    private bool IsOpenedVacancy(string value)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var vacancy = db.Vacancies.Find(value);
            if (vacancy == null)
            {
                return false;
            }
            return vacancy.IsOpened;
        }
    }
}
