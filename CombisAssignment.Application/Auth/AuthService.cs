using CombisAssignment.Application.Auth.DTOs;
using CombisAssignment.Application.Auth.Settings;
using CombisAssignment.Core.Constants;
using CombisAssignment.Core.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CombisAssignment.Application.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserRepository userRepo, IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepo;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<string?> Login(LoginRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !VerifyPassword(request.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid credentials");

            return GenerateToken(user);
        }

        public async Task<bool> RegisterUserAsync(string name, string email, string password)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
                return false;

            var hashedPassword = HashPassword(password);
            var newUser = new Core.Entities.User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                Password = hashedPassword
            };

            await _userRepository.AddAsync(newUser);
            return true;
        }

        private string GenerateToken(Core.Entities.User user)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role switch
            {
                Core.Enums.Role.User => RoleNames.User,
                Core.Enums.Role.Admin => RoleNames.Admin,
                _ => "",
            })
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private bool VerifyPassword(string password, string hashedPassword) =>
            BCrypt.Net.BCrypt.Verify(password, hashedPassword);

        private  string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);
    }
}
