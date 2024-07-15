using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using WinterFamily.Main.Common.Handlers;

namespace WinterFamily.Main.Utils.Extensions;

internal static class ModalExtensions
{
    public static void OnSubmit(this DiscordInteractionResponseBuilder responseBuilder, AsyncEventHandler<DiscordClient, ModalSubmitEventArgs> function)
    {
        if (ModalSubmittedHandler.ModalSubmitInvoker!.ContainsKey(responseBuilder.CustomId) != true)
            ModalSubmittedHandler.ModalSubmitInvoker!.Add(responseBuilder.CustomId, function);
        else
            ModalSubmittedHandler.ModalSubmitInvoker![responseBuilder.CustomId] = function;
    }
}