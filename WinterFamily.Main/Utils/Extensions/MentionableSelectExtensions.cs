using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using WinterFamily.Main.Common.Handlers;

namespace WinterFamily.Main.Utils.Extensions;

internal static class MentionableSelectExtensions
{
    public static void OnSelect(this DiscordMentionableSelectComponent selectComponent, AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> function)
    {
        if (ComponentInteractionHandler.ComponentInvoker!.ContainsKey(selectComponent.CustomId) != true)
            ComponentInteractionHandler.ComponentInvoker!.Add(selectComponent.CustomId, function);
        else
            ComponentInteractionHandler.ComponentInvoker![selectComponent.CustomId] = function;
    }
}
