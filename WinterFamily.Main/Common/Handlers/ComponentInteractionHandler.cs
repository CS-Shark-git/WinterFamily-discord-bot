using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;

namespace WinterFamily.Main.Common.Handlers;

internal class ComponentInteractionHandler
{
    public static Dictionary<string, AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs>>? ComponentInvoker { get; set; }

    public ComponentInteractionHandler()
    {
        ComponentInvoker = new Dictionary<string, AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs>>();
    }
    public async Task Handle(DiscordClient sender, ComponentInteractionCreateEventArgs args)
    {
        AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> function;
        if (ComponentInvoker!.TryGetValue(args.Id, out function!))
        {
            await function!.Invoke(sender, args);
        }
    }
}
