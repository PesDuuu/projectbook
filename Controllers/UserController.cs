using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectBooks.Data;
using ProjectBooks.Models;
using System.Security.Cryptography;
using System.Text;

namespace ProjectBooks.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly BookContext _bookContext;

        public UserController(BookContext bookContext)
        {
            _bookContext = bookContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.ASCII.GetBytes(user.Password);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    user.Password = sb.ToString();
                }

                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                _bookContext.Users.Add(user);
                await _bookContext.SaveChangesAsync();

                //return Ok(user);
                return Ok(new { Message = "User registered successfully", UserId = user.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to register user", Message = ex.Message });
            }
        }
    }
}
