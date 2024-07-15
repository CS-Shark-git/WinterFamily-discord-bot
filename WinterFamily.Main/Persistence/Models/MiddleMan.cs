using System.ComponentModel.DataAnnotations;

namespace WinterFamily.Main.Persistence.Models;

internal class MiddleMan
{
    [Key]
    public ulong Id { get; set; }
    public string? PriceList { get; set; }
    public string? Description { get; set; }
    public int TradesAmount { get; set; }

    public List<Review> Reviews { get; set; } = new List<Review>();

}
