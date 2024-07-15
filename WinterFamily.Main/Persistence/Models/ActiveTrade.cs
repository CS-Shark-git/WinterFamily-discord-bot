using System.ComponentModel.DataAnnotations;

namespace WinterFamily.Main.Persistence.Models
{
    internal class ActiveTrade
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public ulong FirstTraderId { get; set; }

        [Required]
        public ulong MiddleManId { get; set; }

        [Required]
        public ulong SecondTraderId { get; set; }
    }
}
