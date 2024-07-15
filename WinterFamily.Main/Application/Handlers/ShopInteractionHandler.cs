using DSharpPlus.EventArgs;
using DSharpPlus;
using WinterFamily.Main.Common.Attributes;
using DSharpPlus.Entities;
using WinterFamily.Main.Utils.Discord;

namespace WinterFamily.Main.Application.Handlers
{
    internal class ShopInteractionHandler
    {
        [ComponentInteraction("shop_select")]
        public async Task InvokePanel(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {
            var value = args.Values.First();

            var packageEmoji = DiscordEmoji.FromName(client, ":package:");
            var coinEmoji = DiscordEmoji.FromName(client, ":coin:");

            DiscordEmbedBuilder firstAdEmbed = BuildFirstAdEmbed();
            DiscordEmbedBuilder secondAdEmbed = BuildSecondAdEmbed(packageEmoji, coinEmoji);
            DiscordEmbedBuilder firstStoresBuilder = BuildFirstStoresEmbed(packageEmoji);
            DiscordEmbedBuilder secondStoresBuilder = BuildSecondStoresEmbed(packageEmoji);

            switch (value)
            {
                case "ad":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .AddEmbed(firstAdEmbed)
                        .AddEmbed(secondAdEmbed)
                        .AsEphemeral());
                    break;
                case "stores":
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .AddEmbed(firstStoresBuilder)
                        .AddEmbed(secondStoresBuilder)
                        .AsEphemeral());
                    break;
            }

        }

        [ComponentInteraction("shop_submit_button")]
        public async Task OnSubmitButton(DiscordClient client, ComponentInteractionCreateEventArgs args) 
        {
            var modal = new DiscordInteractionResponseBuilder 
            {
                CustomId = "shop_submit_modal",
                Title = "Заявка на покупку"
            };
            modal.AddComponents(
                new TextInputComponent("Товар", "product_input", "Напишите сюда интересующий Вас товар."))
                .AddComponents(
                new TextInputComponent("Оплата", "payment_input", "Сбербанк, СБП, Киви, Тинькофф и т.д."));
            
            await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }

        [ModalSubmitted("shop_submit_modal")]
        public async Task OnSubmitModal(DiscordClient client, ModalSubmitEventArgs args)
        {
            var responseEmbed = new DiscordEmbedBuilder
            {
                Title = "Новая заявка на покупку!",
                Description = $"``от:`` {args.Interaction.User.Mention}",
                Color = new DiscordColor("2b2d31")
            };
            responseEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
            responseEmbed.WithThumbnail("https://i.imgur.com/oAltFIi.png");
            responseEmbed.AddField("Товар:", $"```{args.Values["product_input"]}```", true);
            responseEmbed.AddField("Оплата:", $"```{args.Values["payment_input"]}```", true);

            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
                new DiscordInteractionResponseBuilder()
                .AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Success, "Заявка отправлена успешно!"))
                .AsEphemeral());

            var channel = args.Interaction.Guild.GetChannel(Settings.ShopRequestsChannel);
        }

        private static DiscordEmbedBuilder BuildSecondStoresEmbed(DiscordEmoji packageEmoji)
        {
            DiscordEmbedBuilder secondStoresBuilder = new DiscordEmbedBuilder
            {
                Title = "►ㅤУправление.",
                Color = new DiscordColor("2b2d31")
            };
            secondStoresBuilder.WithImageUrl("https://i.imgur.com/tabpqjj.png");
            secondStoresBuilder.AddField("ㅤ",
                $"> {packageEmoji}ㅤ**Поднять лавку до топ 1:**\n" +
                $"> ```400 ₽ | РАЗ```\nㅤ\n" +
                $"> {packageEmoji}ㅤ**Поднять или опустить лавку:**\n" +
                $"> *• Кроме топ 1*\n" +
                $"> ```200 ₽ | РАЗ```\nㅤ\n" +
                $"> {packageEmoji}ㅤ**Добавить третье место:**\n" +
                $"> *• Место для третьего человека*\n" +
                $"> ```600 ₽ | МЕСЯЦ```", true);
            return secondStoresBuilder;
        }

        private static DiscordEmbedBuilder BuildFirstStoresEmbed(DiscordEmoji packageEmoji)
        {
            DiscordEmbedBuilder firstStoresBuilder = new DiscordEmbedBuilder
            {
                Title = "►ㅤЛавки.",
                Color = new DiscordColor("2b2d31")
            };
            firstStoresBuilder.WithImageUrl("https://i.imgur.com/eGHnbzy.png");
            firstStoresBuilder.AddField("ㅤ",
                $"> {packageEmoji}ㅤ**Личный канал:**\n" +
                $"> ```1300 ₽ | МЕСЯЦ```\nㅤ\n" +
                $"> {packageEmoji}ㅤ**Личный премиум-канал:**\n" +
                $"> *• Добавляется в категорию \"Лучшие лавки\"*\n" +
                $"> ```2900 ₽ | МЕСЯЦ```", true);
            return firstStoresBuilder;
        }

        private static DiscordEmbedBuilder BuildSecondAdEmbed(DiscordEmoji packageEmoji, DiscordEmoji coinEmoji)
        {
            var secondAdEmbed = new DiscordEmbedBuilder
            {
                Title = "►ㅤПрайс лист.",
                Color = new DiscordColor("2b2d31")
            };
            secondAdEmbed.WithImageUrl("https://i.imgur.com/tabpqjj.png");
            secondAdEmbed.AddField("ㅤ",
                $"> {packageEmoji}ㅤ**Услуга:**\n" +
                $"> ```Реклама в списке \n> \"Лучшие розыгрыши\"```\nㅤ\n" +
                $"> {packageEmoji}ㅤ**Услуга:**\n" +
                $"> ```Отдельный канал```\nㅤ\n" +
                $"> {packageEmoji}ㅤ**Услуга:**\n" +
                $"> ```Реклама в розыгрышах```\nㅤ\n" +
                $"> {packageEmoji}ㅤ**Услуга:**\n" +
                $"> ```Доп пинг | Доп день```", true);

            secondAdEmbed.AddField("ㅤ",
                $"> {coinEmoji}ㅤ**Стоимость:**\n" +
                $"> ```1600 ₽```\nㅤ\nㅤ\n" +
                $"> {coinEmoji}ㅤ**Стоимость:**\n" +
                $"> ```1100 ₽```\nㅤ\n" +
                $"> {coinEmoji}ㅤ**Стоимость:**\n" +
                $"> ```850 ₽```\nㅤ\n" +
                $"> {coinEmoji}ㅤ**Стоимость:**\n" +
                $"> ```400 | 150 ₽```", true);
            return secondAdEmbed;
        }

        private static DiscordEmbedBuilder BuildFirstAdEmbed()
        {
            var firstAdEmbed = new DiscordEmbedBuilder
            {
                Title = $"►ㅤРеклама.",
                Color = new DiscordColor("2b2d31"),
                Description = $"**╔►ㅤАктуальная информация для рекламодателей:**\n> \n" +
                              $"> • ``1:`` *Мы не терпим попытки обмана от рекламодателей.*\n" +
                              $"> ``(В их число входит: Не выдача призов, скам-ссылки.)``\n> \n" +
                              $"> • ``2:`` *Администрация не выдает приз победителю.*\n" +
                              $"> ``(В их число входит: Отсутствие выписок и ролл'ов.)``\n> \n" +
                              $"> • ``3:`` *Выдача призов без проверки выполненных условий.*\n" +
                              $"> ``(В их число входит: реролл ради друзей, игнор условий.)``\n> \n" +
                              $"**╚Обязательно к прочтению!**\n"
            };
            firstAdEmbed.WithImageUrl("https://i.imgur.com/eGHnbzy.png");
            return firstAdEmbed;
        }
    }
}
