using System.ComponentModel.DataAnnotations;

namespace WinterFamily.Main.Persistence.Models
{
    internal class Vacancy
    {
        [Key]
        public string? Value { get; set; }

        public bool IsOpened { get; set; }
    }
}
