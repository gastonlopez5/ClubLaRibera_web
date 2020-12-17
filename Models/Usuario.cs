using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace CLubLaRibera_Web.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Clave { get; set; }

        
        [Required]
        public int RolId { get; set; }
        
        
        [ForeignKey("RolId")]
        public TipoUsuario TipoUsuario { get; set; }

        [Required]
        public int GrupoId { get; set; }


        [ForeignKey("GrupoId")]
        public Grupo Grupo { get; set; }


        public string Nombre { get; set; }


        public string Apellido { get; set; }


        public string Telefono { get; set; }

        
        public string Dni { get; set; }


        public string FotoPerfil { get; set; }


        public Boolean Estado { get; set; }
    }
}
