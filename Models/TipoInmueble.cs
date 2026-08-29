using System.ComponentModel.DataAnnotations;

namespace InmobiliariaULP.Models
{
    public class TipoInmueble
    {
        [Key]
        [Display(Name = "Codigo")]
        public int IdTipoInmueble { get; set; }

        [Required(ErrorMessage = "La descripcion del tipo es obligatoria.")]
        [StringLength(50)]
        [Display(Name = "Tipo de Inmueble")]
        public string Descripcion { get; set; } = string.Empty;

        public override string ToString() => Descripcion;
    }
}
