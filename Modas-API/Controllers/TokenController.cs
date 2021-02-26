using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Modas.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Modas.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private IConfiguration _config;
        private UserManager<AppUser> _userManager;

        public TokenController(UserManager<AppUser> userManager, IConfiguration config)
        {
            _config = config;
            _userManager = userManager;
        }

        //public IActionResult RequestToken([FromBody]UserLogin login)

        [HttpPost]
        public async Task<object> RequestToken([FromBody] UserLogin login)
        {
            IActionResult response = Unauthorized();
            //var user = AppUser.Authenticate(login);

            //if (user != null)
            if (ModelState.IsValid)
            {
                //var tokenString = BuildToken(user);
                //response = Ok(new { token = tokenString });
                AppUser user = await _userManager.FindByEmailAsync(login.Username);
                if (user != null)
                {
                    var result = await _userManager.CheckPasswordAsync(user, login.Password);
                    if (result)
                    {
                        // Check for role
                        if (await _userManager.IsInRoleAsync(user, _config["Jwt:Role"]))
                        {
                            response = Ok(new { token = BuildToken(user) });
                        }
                        else
                        {
                            // 403 Forbidden
                            response = Forbid();
                        }
                    }
                }

            }

            return response;
        }

        private string BuildToken(AppUser user)
        {
            var claims = new[] {
                //new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                //new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                //new Claim(JwtRegisteredClaimNames.Email, user.Email),

                new Claim(JwtRegisteredClaimNames.UniqueName, user.Id)

                //new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                //new Claim(JwtRegisteredClaimNames.UniqueName, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                null, // issuer
                null, // audience
                claims,
                //expires: DateTime.Now.AddDays(7),
                expires: DateTime.Now.AddDays(Int16.Parse(_config["Jwt:ValidFor"])),
                signingCredentials: creds);
            // returns the json data with key named "token" and the value of the key
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
