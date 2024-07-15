using System.ComponentModel.DataAnnotations;

namespace WinterFamily.Main.Persistence.Models;

internal class AutoRole
{
    [Key]
    public string? CustomId { get; set; }

    public ulong RoleId { get; set; }
}
