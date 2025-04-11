using CombisAssignment.Application.User.DTOs;
using CombisAssignment.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombisAssignment.Application.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            });
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

        public async Task<Guid> CreateAsync(CreateUserDto dto)
        {
            var user = new Core.Entities.User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email
            };

            await _repository.AddAsync(user);
            return user.Id;
        }

        public async Task<bool> UpdateAsync(Guid id, CreateUserDto dto)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null) return false;

            user.Name = dto.Name;
            user.Email = dto.Email;

            await _repository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
