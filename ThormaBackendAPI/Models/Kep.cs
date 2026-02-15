using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ThormaBackendAPI.Models
{
    [Table("kepek")]
    public class Kep
    {
        [Key]
        public string Leltar { get; set; } = null!;

        [Required]
        public int Fazon { get; set; }

        [Required]
        public string Cim { get; set; } = string.Empty;

        [Required]
        public int Keszult { get; set; }

        [Required]
        public string Anyag { get; set; } = string.Empty;

        [Required]
        public string Technika { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,1)")]
        public decimal Szeles { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,1)")]
        public decimal Magas { get; set; }

        // Navigációs tulajdonság
        // ASP.NET alapból a "FestoID" nevet keresi,
        // ezért kell a ForeignKey attribútum
        [ForeignKey("Fazon")]
        public Festo? Festo { get; set; }
    }
}
