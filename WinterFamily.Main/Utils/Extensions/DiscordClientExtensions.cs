using DSharpPlus;
using WinterFamily.Main.Common.Handlers;
using WinterFamily.Main.Common.Attributes;

namespace WinterFamily.Main.Utils.Extensions;

internal static class DiscordClientExtensions
{
    public static void RegisterComponentInteractionHandler<T>(this DiscordClient client)
    {
        AttributeService<T> attributeSerivce = new();
        var dictionary = attributeSerivce.GetComponentInteractionHandlers();
        foreach (var pair in dictionary)
            ComponentInteractionHandler.ComponentInvoker.Add(pair.Key, pair.Value);
    }

    public static void RegisterModalSubmitHandler<T>(this DiscordClient client)
    {
        AttributeService<T> attributeSerivce = new();
        var dictionary = attributeSerivce.GetModalSubmitHandlers();
        foreach (var pair in dictionary)
            ModalSubmittedHandler.ModalSubmitInvoker.Add(pair.Key, pair.Value);
    }
}
