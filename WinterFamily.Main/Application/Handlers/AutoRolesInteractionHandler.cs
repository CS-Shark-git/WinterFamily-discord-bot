using DSharpPlus;
using DSharpPlus.EventArgs;
using WinterFamily.Main.Common.Attributes;
using DSharpPlus.Entities;
using WinterFamily.Main.Utils.Discord;
using WinterFamily.Main.Persistence.Models;
using WinterFamily.Main.Persistence;

namespace WinterFamily.Main.Application.Handlers;

internal class AutoRolesInteractionHandler
{
    [ComponentInteraction("autoroles_select")]
    public async Task OnAutoRolesSelect(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        var value = args.Values.First();
        var member = await args.Guild.GetMemberAsync(args.User.Id);
        if (value == "remove_roles") 
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .AsEphemeral());
            var roles = GetRoles(args.Guild);
            foreach(var autoRole in roles) 
            {
                await member.RevokeRoleAsync(autoRole);
            }            
            await args.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(StyledMessageBuilder.BuildResultEmbed(Result.Success, 
                "Все роли убраны", TextType.Bold))
                .AsEphemeral());

            await Task.Delay(2000);
            await args.Interaction.DeleteOriginalResponseAsync();
            return;
        }

        var role = args.Guild.GetRole(GetRole(value).RoleId);

        if (member.Roles.Contains(role) != true) 
        {
            await member.GrantRoleAsync(role);
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder
            .BuildResultEmbed(Result.Success, $"Роль {role.Mention} выдана", TextType.Bold))
            .AsEphemeral());
            await Task.Delay(2000);
            await args.Interaction.DeleteOriginalResponseAsync();
        }
        else 
        {
            await member.RevokeRoleAsync(role);
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(StyledMessageBuilder
                .BuildResultEmbed(Result.Success, $"Роль {role.Mention} убрана", TextType.Bold))
                .AsEphemeral());
            await Task.Delay(2000);
            await args.Interaction.DeleteOriginalResponseAsync();
        }
        
    }

    private IEnumerable<DiscordRole> GetRoles(DiscordGuild guild)
    {
        using (var db = new ApplicationContext())
        {
            foreach (var autoRole in db.AutoRoles) 
            {
                yield return guild.GetRole(autoRole.RoleId);
            }

        }
    }

    private static AutoRole GetRole(string value)
    {
        AutoRole role;
        using (var db = new ApplicationContext())
        {
            role = db.AutoRoles.Find(value)!;
        }

        return role;
    }
}
