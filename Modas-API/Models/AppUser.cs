using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Modas.Models
{   // this class will evenutally inherit from Identity
    public class AppUser : IdentityUser
    {
        //public string Id { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string Email { get; set; }

        //public static AppUser Authenticate(UserLogin login)
        //{
        //    AppUser user = null;

        //    if ((login.Username).ToLower() == "john" && login.Password == "secret")
        //    {
        //        user = new AppUser { Id = "7d1a65e1-143b-4a57-b19a-1308c78384b8", FirstName = "John", LastName = "Doe", Email = "john.doe@mail.com" };
        //    }
        //    return user;
        //}
    }

    public class UserLogin
    {   //username is not an email address - fix later!!!
        [Required, EmailAddress]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}