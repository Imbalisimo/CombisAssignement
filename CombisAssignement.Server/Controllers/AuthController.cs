using CombisAssignment.Application.Auth;
using CombisAssignment.Application.Auth.DTOs;
using CombisAssignment.Core.Entities;
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
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
        {
            var token = await _authService.Login(request);

            if (token != null)
            {
                return Ok(token);
            }

            return Unauthorized();
        }

        [HttpPost("Register")]
        public async Task<bool> RegisterUserAsync(string name, string email, string password)
        {
            return await _authService.RegisterUserAsync(name, email, password);
        }
    }
}
