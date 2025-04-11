using CombisAssignment.Application.Auth.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CombisAssignement.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            // Dummy check — replace with real user validation
            if (request.Username == "admin" && request.Password == "password")
            {
                var claims = new[]
                {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin") // Add role claim here
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this_is_your_super_secret_key_123!"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "yourapi",
                    audience: "yourapi_users",
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }

            return Unauthorized();
        }
    }
}
