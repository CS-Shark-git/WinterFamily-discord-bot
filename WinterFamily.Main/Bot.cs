using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using WinterFamily.Main.Application.Commands;
using WinterFamily.Main.Application.Handlers;
using WinterFamily.Main.Common.Configuration.JsonModels;
using WinterFamily.Main.Common.Handlers;
using WinterFamily.Main.Persistence;
using WinterFamily.Main.Utils.Extensions;

namespace WinterFamily.Main;

internal class Bot
{
    private readonly DiscordClient _client;

    public Bot(Token token)
    {
        _client = new DiscordClient(
        new DiscordConfiguration
        {
            Token = token.Value,
            TokenType = TokenType.Bot,
            MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Error,
            Intents = DiscordIntents.All
        });
    }

    public async Task RunAsync()
    {

        OnClientReady();
        InitDataBase();

         var interactionHandler = new ComponentInteractionHandler();
        _client.ComponentInteractionCreated += interactionHandler.Handle;
        _client.RegisterComponentInteractionHandler<TicketsInteractionHandler>();
        _client.RegisterComponentInteractionHandler<VacanciesInteractionHandler>();
        _client.RegisterComponentInteractionHandler<MiddleMansInteractionHandler>();
        _client.RegisterComponentInteractionHandler<AutoRolesInteractionHandler>();
        _client.RegisterComponentInteractionHandler<ShopInteractionHandler>();
        _client.RegisterComponentInteractionHandler<RulesInteractionHandler>();

        var modalHandler = new ModalSubmittedHandler();
        _client.ModalSubmitted += modalHandler.Handle;
        _client.RegisterModalSubmitHandler<TicketsInteractionHandler>();
        _client.RegisterModalSubmitHandler<VacanciesInteractionHandler>();
        _client.RegisterModalSubmitHandler<MiddleMansInteractionHandler>();
        _client.RegisterModalSubmitHandler<AutoRolesInteractionHandler>();
        _client.RegisterModalSubmitHandler<ShopInteractionHandler>();

        var slash = _client.UseSlashCommands();
        slash.RegisterCommands<TicketsCommands>();
        slash.RegisterCommands<VacanciesCommands>();
        slash.RegisterCommands<MiddleMansCommands>();
        slash.RegisterCommands<AutoRolesCommands>();
        slash.RegisterCommands<ShopCommands>();
        slash.RegisterCommands<RulesCommands>();

        IncludeInteractivity();
        IncludeErrorHandling(slash);

        await _client.ConnectAsync();
        await Task.Delay(-1);
    }


    private void IncludeInteractivity()
    {
        _client.UseInteractivity(new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromSeconds(60)
        });
    }

    private void OnClientReady()
    {
        _client!.Ready += async (s, e) =>
        {
            await Task.Run(() =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now}] Bot started!");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.Now}] Connected as {s.CurrentUser.Username}#{s.CurrentUser.Discriminator}");
                Console.ForegroundColor = ConsoleColor.White;
            });
        };
    }

    private void InitDataBase() 
    {
        new ApplicationContext();
    }

    private void IncludeErrorHandling(SlashCommandsExtension slash)
    {
        slash.SlashCommandErrored += async (ex, args) =>
        {
            if (args.Exception is SlashExecutionChecksFailedException slex)
            {
                foreach (var check in slex.FailedChecks)
                    if (check is SlashRequireUserPermissionsAttribute att) 
                    {
                        await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .WithContent($"Только ``администратор`` может использовать эту команду")
                            .AsEphemeral());
                        return;
                    }
   
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(args.Exception + "\n");
            Console.ForegroundColor = ConsoleColor.White;
        };
    }
}