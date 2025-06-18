using Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UserService.Data;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController :ControllerBase
    {
        private readonly UserDbContext _context;

        public UsersController (UserDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers ()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser (Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            return user;
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser (User user)
        {
            if(string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email))
            {
                return BadRequest("Username and Email are required.");
            }

            // Generate salt and hash password (simple example, use BCrypt in production)
            using(var hmac = new HMACSHA512())
            {
                user.PasswordSalt = Convert.ToBase64String(hmac.Key);
                user.PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(user.PasswordHash ?? "")));
            }

            user.Id = Guid.NewGuid();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser (Guid id, User updatedUser)
        {
            if(id != updatedUser.Id)
            {
                return BadRequest();
            }

            var user = await _context.Users.FindAsync(id);
            if(user == null)
            {
                return NotFound();
            }

            // Update only allowed fields
            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            if(!string.IsNullOrEmpty(updatedUser.PasswordHash))
            {
                using(var hmac = new HMACSHA512(Convert.FromBase64String(user.PasswordSalt)))
                {
                    user.PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(updatedUser.PasswordHash)));
                }
            }
            user.IsActive = updatedUser.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser (Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if(user == null)
            {
                return NotFound();
            }

            user.IsActive = false; // Soft delete by setting IsActive to false
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
