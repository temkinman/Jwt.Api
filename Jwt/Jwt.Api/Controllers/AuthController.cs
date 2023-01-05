using Jwt.Api.Data.Context;
using Jwt.Api.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Jwt.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLogin userLogin)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            // check if the user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userLogin.UserName && u.Password == userLogin.Password);
            
            if(user == null) 
            {
                return Unauthorized();
            }

            // create the Token

            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var singinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var signinCredentials = new SigningCredentials(singinKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(7),
                    signingCredentials: signinCredentials
                );

            var JwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { Token = JwtToken });
        }
    }
}
