using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Data
{
    public class Userpers : IdentityUser
    {
          
        [Required]
        [StringLength(100)]
        public string Nom { get; set; }
    }
}

