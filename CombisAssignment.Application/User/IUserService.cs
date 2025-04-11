using CombisAssignment.Application.User.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombisAssignment.Application.User
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(CreateUserDto dto);
        Task<bool> UpdateAsync(Guid id, CreateUserDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
