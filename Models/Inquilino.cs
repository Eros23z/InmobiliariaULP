using System.ComponentModel.DataAnnotations;

namespace InmobiliariaULP.Models
{
    public class Inquilino : Persona
    {
        [Key]
        [Display(Name = "Código")]
        public int IdInquilino { get; set; }

        
    }
}