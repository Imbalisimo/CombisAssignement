using CombisAssignment.Application.Auth;
using CombisAssignment.Application.Auth.DTOs;
using CombisAssignment.Application.Auth.Settings;
using CombisAssignment.Core.Entities;
using CombisAssignment.Core.Enums;
using CombisAssignment.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CombisAssignment.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ILoginAttemptService> _loginAttemptMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _loginAttemptMock = new Mock<ILoginAttemptService>();

            var jwtSettings = new JwtSettings
            {
                SecretKey = "ThisIsASecretKeyForTestingPurposesOnly!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiryMinutes = 60
            };

            var options = Options.Create(jwtSettings);
            _authService = new AuthService(_userRepoMock.Object, options, _loginAttemptMock.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnJwtToken()
        {
            // Arrange
            var password = "P@ssw0rd";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Password = hashedPassword,
                Role = Role.Admin
            };

            var loginRequest = new LoginRequestDto
            {
                Email = user.Email,
                Password = password
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            // Act
            var token = await _authService.Login(loginRequest);

            // Assert
            token.Should().NotBeNullOrWhiteSpace();
            var handler = new JwtSecurityTokenHandler();
            var parsedToken = handler.ReadJwtToken(token);
            parsedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        }

        [Fact]
        public async Task Login_WithInvalidEmail_ShouldThrowUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                Email = "nonexistent@example.com",
                Password = "any"
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(loginRequest.Email)).ReturnsAsync((User)null);

            // Act
            var act = () => _authService.Login(loginRequest);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid credentials");
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldThrowUnauthorized()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("correctpass"),
                Role = Role.User
            };

            var loginRequest = new LoginRequestDto
            {
                Email = user.Email,
                Password = "wrongpass"
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            // Act
            var act = () => _authService.Login(loginRequest);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid credentials");
        }

        [Fact]
        public async Task RegisterUserAsync_UserAlreadyExists_ShouldReturnFalse()
        {
            // Arrange
            var email = "existing@example.com";
            _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(new User());

            // Act
            var result = await _authService.RegisterUserAsync("John", email, "password");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterUserAsync_NewUser_ShouldHashPassword_AndReturnTrue()
        {
            // Arrange
            string name = "Jane";
            string email = "new@example.com";
            string password = "mypassword";

            _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User)null);

            User capturedUser = null;
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterUserAsync(name, email, password);

            // Assert
            result.Should().BeTrue();
            capturedUser.Should().NotBeNull();
            capturedUser.Email.Should().Be(email);
            capturedUser.Name.Should().Be(name);
            capturedUser.Password.Should().NotBe(password); // Should be hashed
            BCrypt.Net.BCrypt.Verify(password, capturedUser.Password).Should().BeTrue();
        }
    }
}
