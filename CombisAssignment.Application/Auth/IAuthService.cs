using CombisAssignment.Application.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombisAssignment.Application.Auth
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(string name, string email, string password);
        Task<string?> Login(LoginRequestDto request);
    }
}
