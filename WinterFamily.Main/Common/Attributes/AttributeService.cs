using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using WinterFamily.Main.Persistence;

namespace WinterFamily.Main.Common.Attributes;

internal class AttributeService<T> 
{

    public string GetFileNameAgrument()
    {
        var fileName = typeof(T).GetCustomAttributes(
            typeof(FileNameAttribute), true
        ).FirstOrDefault() as FileNameAttribute;

        if (fileName != null)
        {
            return fileName.Name;
        }
        throw new NullReferenceException($"Attribute 'FileNameAttribute' not found in '{typeof(T).Name}'");
    }

    public Dictionary<string, AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs>> GetComponentInteractionHandlers()
    {
        var methods = typeof(T).GetMethods()
            .Where(x => x
            .GetCustomAttribute(typeof(ComponentInteractionAttribute), false) != null && 
            x.GetParameters().Length == 2 &&
            (x.GetParameters()[0].ParameterType == typeof(DiscordClient) &&
            x.GetParameters()[1].ParameterType == typeof(ComponentInteractionCreateEventArgs) &&
            IsAsyncMethod(x.Name) &&
            x.ReturnType == typeof(Task)));

        var dictionary = new Dictionary<string, AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs>>();

        foreach (var method in methods) 
        {
            var attr = method.GetCustomAttribute(typeof(ComponentInteractionAttribute)) as ComponentInteractionAttribute;
            
            var eventHandler = method.CreateDelegate(typeof(AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs>), 
                Activator.CreateInstance(typeof(T))) 
                as AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs>;
            if (eventHandler != null && attr != null)
            {
                dictionary.Add(attr.CustomId, eventHandler);
            }
        }
        return dictionary;
    }

    public Dictionary<string, AsyncEventHandler<DiscordClient, ModalSubmitEventArgs>> GetModalSubmitHandlers()
    {
        var methods = typeof(T).GetMethods()
            .Where(x => x
            .GetCustomAttribute(typeof(ModalSubmittedAttribute), false) != null &&
            x.GetParameters().Length == 2 &&
            (x.GetParameters()[0].ParameterType == typeof(DiscordClient) &&
            x.GetParameters()[1].ParameterType == typeof(ModalSubmitEventArgs) &&
            IsAsyncMethod(x.Name) &&
            x.ReturnType == typeof(Task)));

        var dictionary = new Dictionary<string, AsyncEventHandler<DiscordClient, ModalSubmitEventArgs>>();

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute(typeof(ModalSubmittedAttribute)) as ModalSubmittedAttribute;

            var eventHandler = method.CreateDelegate(typeof(AsyncEventHandler<DiscordClient, ModalSubmitEventArgs>),
                Activator.CreateInstance(typeof(T)))
                as AsyncEventHandler<DiscordClient, ModalSubmitEventArgs>;
            if (eventHandler != null && attr != null)
            {
                dictionary.Add(attr.CustomId, eventHandler);
            }
        }
        return dictionary;
    }

    private bool IsAsyncMethod(string methodName)
    {
        MethodInfo method = typeof(T).GetMethod(methodName);
        Type attType = typeof(AsyncStateMachineAttribute);
        var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);
        return (attrib != null);
    }
}
