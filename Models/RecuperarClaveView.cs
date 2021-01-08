using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CLubLaRibera_Web.Models
{
    public class RecuperarClaveView
    {
        [Required(ErrorMessage ="Debe ingresar un mail válido")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
