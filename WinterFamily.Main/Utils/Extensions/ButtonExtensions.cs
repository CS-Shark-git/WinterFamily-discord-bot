using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using WinterFamily.Main.Common.Handlers;

namespace WinterFamily.Main.Utils.Extensions;

internal static class ButtonExtensions
{
    public static void OnClick(this DiscordButtonComponent buttonComponent, AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> function) 
    {
        if (ComponentInteractionHandler.ComponentInvoker!.ContainsKey(buttonComponent.CustomId) != true)
            ComponentInteractionHandler.ComponentInvoker!.Add(buttonComponent.CustomId, function);
        else 
            ComponentInteractionHandler.ComponentInvoker![buttonComponent.CustomId] = function;
    }
}
