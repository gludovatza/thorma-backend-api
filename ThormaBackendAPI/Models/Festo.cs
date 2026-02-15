using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ThormaBackendAPI.Models
{
    [Table("festok")]
    public class Festo
    {
        [Key]
        public int Azon { get; set; }

        [Required]
        public string Nev { get; set; } = string.Empty;

        [Required]
        public int Szuletett { get; set; }

        [Required]
        public int Meghalt { get; set; }

        // navigation property
        public ICollection<Kep>? Kepek { get; set; }
    }
}
