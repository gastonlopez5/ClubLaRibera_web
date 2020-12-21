using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CLubLaRibera_Web.Models
{
    public class LoginView
    {
        [Required(ErrorMessage = "Email requerido")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Ingrese un email válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(50, ErrorMessage = "La clave debe tener entre 3 y 50 caracteres", MinimumLength = 3)]
        [DataType(DataType.Password)]
        public string Clave { get; set; }
    }
}
