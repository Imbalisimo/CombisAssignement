using CombisAssignment.Application.User.DTOs;
using CombisAssignment.Application.User;
using CombisAssignment.Core.Enums;
using CombisAssignment.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CombisAssignment.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repositoryMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _repositoryMock = new Mock<IUserRepository>();
            _service = new UserService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnUserDtos_WithPasswordAndRole()
        {
            // Arrange
            var users = new List<Core.Entities.User>
                {
                    new() { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com", Password = "pass1", Role = Role.User },
                    new() { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@example.com", Password = "pass2", Role = Role.Admin }
                };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Password.Should().Be("pass1");
            result.Last().Role.Should().Be(Role.Admin);
        }

        [Fact]
        public async Task GetByIdAsync_UserExists_ShouldReturnFullUserDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new Core.Entities.User
            {
                Id = userId,
                Name = "Charlie",
                Email = "charlie@example.com",
                Password = "charliepass",
                Role = Role.User
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _service.GetByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Password.Should().Be("charliepass");
            result.Role.Should().Be(Role.User);
        }

        [Fact]
        public async Task GetByIdAsync_UserNotFound_ShouldReturnNull()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Core.Entities.User)null);

            // Act
            var result = await _service.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldAddUserWithPassword_AndReturnId()
        {
            // Arrange
            var dto = new CreateUserDto { Name = "Dave", Email = "dave@example.com", Password = "secret" };
            Core.Entities.User capturedUser = null;

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Core.Entities.User>()))
                .Callback<Core.Entities.User>(u => capturedUser = u)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeEmpty();
            capturedUser.Should().NotBeNull();
            capturedUser.Password.Should().Be("secret");
        }

        [Fact]
        public async Task UpdateAsync_UserExists_ShouldUpdatePasswordAndReturnTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existingUser = new Core.Entities.User
            {
                Id = id,
                Name = "Old",
                Email = "old@example.com",
                Password = "oldpass",
                Role = Role.User
            };
            var dto = new CreateUserDto { Name = "New", Email = "new@example.com", Password = "newpass" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingUser);
            _repositoryMock.Setup(r => r.UpdateAsync(existingUser)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(id, dto);

            // Assert
            result.Should().BeTrue();
            existingUser.Name.Should().Be("New");
            existingUser.Password.Should().Be("newpass");
        }

        [Fact]
        public async Task UpdateAsync_UserNotFound_ShouldReturnFalse()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Core.Entities.User)null);

            // Act
            var result = await _service.UpdateAsync(Guid.NewGuid(), new CreateUserDto());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_UserExists_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            var user = new Core.Entities.User { Id = id };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
            _repositoryMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_UserNotFound_ShouldReturnFalse()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Core.Entities.User)null);

            // Act
            var result = await _service.DeleteAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }
    }
}
