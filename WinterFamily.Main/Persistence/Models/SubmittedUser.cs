using System.ComponentModel.DataAnnotations;

namespace WinterFamily.Main.Persistence.Models;

internal class SubmittedUser
{
    [Key]
    public ulong Id { get; set; }
    public bool ModeratorSubmitted { get; set; }
    public bool EventerSubmitted { get; set; }
}
