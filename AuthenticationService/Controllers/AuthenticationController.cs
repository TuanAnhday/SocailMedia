using AuthenticationService.Data;
using AuthenticationService.Dtos;
using Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController :ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationController (AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register ([FromBody] RegisterModel register)
        {
            if(await _context.AuthUsers.AnyAsync(u => u.Username == register.Username))
            {
                return BadRequest("Username already exists.");
            }

            using(var hmac = new HMACSHA512())
            {
                var passwordSalt = Convert.ToBase64String(hmac.Key);
                var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)));

                var user = new User
                {
                    Username = register.Username,
                    Email = register.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                };

                _context.AuthUsers.Add(user);
                await _context.SaveChangesAsync();
            }

            return Created("api/authentication/login", null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login ([FromBody] LoginModel login)
        {
            var user = await _context.AuthUsers
                .FirstOrDefaultAsync(u => u.Username == login.Username && u.IsActive);

            if(user == null || !VerifyPassword(login.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private bool VerifyPassword (string password, string storedHash, string storedSalt)
        {
            using(var hmac = new HMACSHA512(Convert.FromBase64String(storedSalt)))
            {
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == storedHash;
            }
        }

        private string GenerateJwtToken (User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

  
}
