using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;

namespace WinterFamily.Main.Common.Handlers;

internal class ModalSubmittedHandler
{
    public static Dictionary<string, AsyncEventHandler<DiscordClient, ModalSubmitEventArgs>>? ModalSubmitInvoker { get; set; }

    public ModalSubmittedHandler()
    {
        ModalSubmitInvoker = new Dictionary<string, AsyncEventHandler<DiscordClient, ModalSubmitEventArgs>>();
    }

    public async Task Handle(DiscordClient sender, ModalSubmitEventArgs args) 
    {
        AsyncEventHandler<DiscordClient, ModalSubmitEventArgs> function;
        if (ModalSubmitInvoker!.TryGetValue(args.Interaction.Data.CustomId, out function!))
            await function!.Invoke(sender, args);
    }
}
