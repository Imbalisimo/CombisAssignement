using CombisAssignment.Application.User;
using CombisAssignment.Core.Entities;
using CombisAssignment.Core.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CombisAssignment.Tests
{
    public class UserRepositoryTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserRepositoryTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Name = "John Doe", Email = "john.doe@example.com" };
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("John Doe");
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
            _userRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<User>().Id));

            // Act
            await _userService.DeleteAsync(userId);

            // Assert
            _userRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<User>().Id), Times.Once);
        }
    }
}
