using System.ComponentModel.DataAnnotations;

namespace WinterFamily.Main.Persistence.Models;

internal class Cooldown
{
    [Key]
    public ulong UserId { get; set; }

    public DateTime QuestionTimeStamp { get; set; }

    public DateTime ComplaintTimeStamp { get; set; }

}
