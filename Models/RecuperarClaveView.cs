using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CLubLaRibera_Web.Models
{
    public class RecuperarClaveView
    {
        [Required(ErrorMessage = "Email requerido")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Ingrese un email válido")]
        public string Email { get; set; }
    }
}
