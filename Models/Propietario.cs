using System.ComponentModel.DataAnnotations;

namespace InmobiliariaULP.Models
{
    public class Propietario : Persona
    {
        [Key]
        [Display(Name = "Código")]
        public int IdPropietario { get; set; }

        
    }
}