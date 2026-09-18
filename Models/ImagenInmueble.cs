using System.ComponentModel.DataAnnotations;

namespace InmobiliariaULP.Models
{
    public class ImagenInmueble
    {
        [Key]
        public int IdImagen { get; set; }

        [Required]
        [StringLength(255)]
        public string Url { get; set; } = string.Empty;

        public int IdInmueble { get; set; }
        public Inmueble? Inmueble { get; set; }
    }
}