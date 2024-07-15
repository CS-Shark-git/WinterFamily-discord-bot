using System.ComponentModel.DataAnnotations;

namespace WinterFamily.Main.Persistence.Models;

internal class Review
{
    [Key]
    public Guid Id { get; set; }
    public ulong UserId { get; set; }
    public string? Content { get; set; }
    public int Rate { get; set; }

    public MiddleMan MiddleMan { get; set; }
}
