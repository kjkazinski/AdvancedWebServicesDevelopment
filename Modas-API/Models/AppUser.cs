using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace Modas.Models
{
    public class AppUser : IdentityUser
    {

    }

    public class UserLogin
    {
        [Required, EmailAddress]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}