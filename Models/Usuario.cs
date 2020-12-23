using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CLubLaRibera_Web.Models;
using Microsoft.AspNetCore.Http;

namespace CLubLaRibera_Web.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [NotMapped]
        public string Clave { get; set; }

        [NotMapped]
        public int RolId { get; set; }
        
        [ForeignKey("RolId")]
        public TipoUsuario TipoUsuario { get; set; }

        [NotMapped]
        public int GrupoId { get; set; }

        [ForeignKey("GrupoId")]
        public Grupo Grupo { get; set; }

        [Required(ErrorMessage ="Complete el campo Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Complete el campo Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "Complete el campo Telefono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "Complete el campo Dni")]
        public string Dni { get; set; }

        public string FotoPerfil { get; set; }

        public IFormFile Archivo { get; set; }

        public Boolean Estado { get; set; }
    }
}
