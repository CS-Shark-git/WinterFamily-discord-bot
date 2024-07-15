using DSharpPlus.EventArgs;
using DSharpPlus;
using WinterFamily.Main.Common.Attributes;
using DSharpPlus.Entities;
using WinterFamily.Main.Utils.Discord;

namespace WinterFamily.Main.Application.Handlers
{
    internal class RulesInteractionHandler
    {
        [ComponentInteraction("rules_select")]
        public async Task OnRulesSelect(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {
            string value = args.Values.First();

            var responseBuilder = new DiscordInteractionResponseBuilder().AsEphemeral();

            switch (value)
            {
                case "rules_group1_select":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        responseBuilder
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("1.1 | Оскорбления",
                        "Запрещены оскорбления пользователей (пример: 'лох,мудак,пидр' и т.д)", "2 часа", PunishmentType.Mute))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("1.2 | Упоминание родителей",
                        "Запрещено упоминание родителей", "15 часов", PunishmentType.Mute))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("1.3 | Оскорбление родителей",
                        "Запрещено оскорбление родителей", "7 дней", PunishmentType.Mute))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("1.4 | Запрещенные названия",
                        "Запрещено называть пользователя болезнями и профессиями клоуном и т.д", "5 часов", PunishmentType.Mute))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("1.5 | Оскорбление национальности, религии",
                        "Запрещено оскорблять национальсть (расса в том числе) или религию", "7 дней", PunishmentType.Mute))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("1.6 | Угрозы",
                        "Запрещенно угрожать расправой и убийством", "15 дней", PunishmentType.Ban))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("1.7 | Оскорбление администрации/стаффа",
                        "Запрещено оскорбление стаффа/сервера/администрации", "7 дней",
                        PunishmentType.Mute, note: "В некоторых случаях вы можете получить бан от администрации"))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("1.8 | Провокации",
                        "Запрещено провоцировать кого-то", "10 часов", PunishmentType.Mute)));
                    break;

                case "rules_group2_select":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        responseBuilder
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("2.1 | Пиар в чатах",
                        "Запрещены пиар в чатах", "30 дней", PunishmentType.Ban))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("2.2 | Пиар в лс",
                        "Запрещен пиар в лс", "4 месяца", PunishmentType.Ban))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("2.3 | Ссылки в профиле",
                        "Запрещено просить зайти по ссылке в профиле", "2 месяца", PunishmentType.Ban))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("2.4 | Пиар скам серверов",
                        "Строго запрещено пиарить различные скам сервера", "∞", PunishmentType.Ban)));
                    break;

                case "rules_group3_select":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        responseBuilder
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("3.1 | Флуд",
                        "Запрещено флудить флуд от 8 символов, например: 'ааааааааааа,пвшмигрпкпгитыавгширваи,пооооооооон'",
                        "2 часа", PunishmentType.Mute, note: "Сначала выдается предупреждение, затем мут (в трейдах не распостраняется)"))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("3.2 | Смех-флуд",
                        "Смех не считается флудом до 10 символов, 10 и более расценивается как нарушение", "1 час",
                        PunishmentType.Mute, note: "Сначала выдается предупреждение, затем мут (в трейдах не распостраняется)"))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("3.3 | Злоупотребление флудом",
                        "Запрещено нарочно злоупотреблять сообщениями до 8 повторений символов", "15 часов", PunishmentType.Mute))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("3.4 | Спам",
                        "Запрещено спамить, за спам считается от 4 одинаковых сообщений", "3 часа", PunishmentType.Mute,
                        note: "Сначала выдается предупреждение, затем мут (в трейдах не распостраняется)"))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("3.5 | Спам",
                        "Запрещено спамить, за спам считается от 4 одинаковых сообщений", "3 часа", PunishmentType.Mute,
                        note: "Сначала выдается предупреждение, затем мут (в трейдах не распостраняется)"))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("3.6 | Спам",
                        "Запрещено спамить, за спам считается от 4 одинаковых сообщений", "3 часа", PunishmentType.Mute,
                        note: "Сначала выдается предупреждение, затем мут (в трейдах не распостраняется)"))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("3.7 | Спам",
                        "Запрещено спамить, за спам считается от 4 одинаковых сообщений. " +
                        "Запрещено повторять одно сообщение более 4 раз за короткий промежуток времени (5 минут). " +
                        "Запрещено отправлять быстро бессмысленные сообщения от 5 раз с лимитом до 2 минут" +
                        " (примеры спама: слово 'привет', отправленное раз 5 подряд или просто спам буквами по типу: 'а а а  а а а а а')", "3 часа", PunishmentType.Mute,
                        note: "Сначала выдается предупреждение, затем мут (в трейдах не распостраняется)"))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("3.8 | Оффтоп",
                        "Запрещено писать оффтоп (не по теме канала). " +
                        "Нельзя писать в общий чат: 'трейжу дракона на тень' — для этого есть трейды блокс фрукт. " +
                        "Также нельзя писать в другие трейды, по типу: в трейды юбы нельзя писать 'трейжу будду'"
                        , "2 часа", PunishmentType.Mute,
                        note: "Сначала выдается предупреждение, затем мут — вы получите сообщение от модератора," +
                        " что Вы нарушете правила. Если повторите — мут."))
                        );
                    break;

                case "rules_group4_select":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        responseBuilder
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("4.1 | Порнография",
                        "Запрещено отправлять порнографию на сервере", "15 дней", PunishmentType.Ban))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("4.2 | Контент 18+",
                        "Нельзя отправлять видео, содержащие контент 18+ ", "15 дней", PunishmentType.Ban))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("4.3 | Оголённые фото",
                        "Нельзя скидывать фотографии: оголенное тело даже с цензурой", "5 дней", PunishmentType.Ban,
                        note: "Если на фотографии присутствует цензура — наказание смягчается до 5 часов мута")));
                    break;

                case "rules_group5_select":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        responseBuilder
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("5.1 | Оскорбление в войсах",
                        "Запрещено оскорбление в участников в войсах", "2 часа", PunishmentType.Mute,
                        note: "Сначала выдается предупреждение, затем мут"))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("5.2 | Звуковые панели",
                        "Запрещено злоупотреблять звуковыми панелями", "5 часов", PunishmentType.Mute))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("5.3 | ПО для изменения голоса",
                        "Нельзя изменять свой голос, используя программы", "2 часа", PunishmentType.Mute))
                        .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("5.4 | Громкие звуки",
                        "Нельзя издавать громкие звуки, скримеры ", "4 часа", PunishmentType.Mute)));
                    break;
                case "rules_group6_select":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        BuildSixthRulesGroupFragment(responseBuilder));
                    break;
            }
            await args.Message.ModifyAsync();
        }

        [ComponentInteraction("next_rules_button")]
        public async Task OnNextButton(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.10 | Боты",
                "Запрещено делать действия, направленные на неккоректную работу ботов сервера"
                , "20 дней", PunishmentType.Ban))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.11 | Твинки",
                "Твинки разрешены только если они не используются для скама, обхода наказания, " +
                "накручивание валюты в эко, иначе следует наказание"
                , "5 дней", PunishmentType.Ban, note: "Банятся все аккаунты"))
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "back_rules_button", "Назад",
                        emoji: new DiscordComponentEmoji(1167195419852406885))));

        }

        [ComponentInteraction("back_rules_button")]
        public async Task OnBackButton(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                BuildSixthRulesGroupFragment(new DiscordInteractionResponseBuilder()));

        }

        private DiscordInteractionResponseBuilder BuildSixthRulesGroupFragment(DiscordInteractionResponseBuilder responseBuilder)
        {
            return responseBuilder
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("Права администрации: ",
                "Админы могут выдать вам любое наказание по любой причине.\n" +
                "Админ может изменить услугу или отказать вам в её продаже в любой момент времени"))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.1 | Рейд",
                "Организация рейда категорически запрещена. " +
                "Под рейдом подразумевается общая организация и исполнение следующих действий:" +
                "\n спамить очень быстро длинными сообщениями, спамить порнографией, " +
                "спамить сообщениями, содержащие оскорбления и прочие нарушения правил. Наказание получают `все` участники рейда"
                , "∞", PunishmentType.Ban))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.2 | Личная информация",
                "Запрещено распространять личную информацию пользователя без его согласия", "10 дней", PunishmentType.Ban))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.3 | Фейки",
                "Запрещено выдавать себя за другого человека (гарантов, продавцов)", "∞", PunishmentType.Ban))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.4 | Тикеты",
                "Нельзя создавать тикеты без существенной на то причины", "1 час", PunishmentType.Mute))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.5 | Пинг стаффа",
                "Нельзя пинговать (упомянать) модерацию/администрацию без весомой причины", "1 час", PunishmentType.Mute))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.6 | Отказ от гаранта",
                "Отказ от гаранта недопустим (в случае если человек не знает кто такой гарант и он должен ему объяснить).\n"
                , "∞", PunishmentType.Ban, note: "Отказ от сделки и просьба поменять мм не является нарушением правил"))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.7 | Попрошайничество",
                "Нельзя попрошайничать, будь то просьба дать аккаунт, фрукт или еще что-либо подобное"
                , "10 часов", PunishmentType.Mute))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.8 | Скупка/продажа в трейдах, кросс трейд",
                "Продажа или скупка фруктов/аккаунтов в трейдах запрещена.\n" +
                "Также запрещен кросс трейд бф на стендофф."
                , "2 часа", PunishmentType.Mute))
                .AddEmbed(StyledMessageBuilder.BuildRuleEmbed("6.9 | Обман (скам)",
                "Любая попытка или процесс обмана (скама) участника ни в коем случае недопустима"
                , "∞", PunishmentType.Ban))
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "next_rules_button", "Далее",
                emoji: new DiscordComponentEmoji(1167195417289699339)));
        }

    }
}
