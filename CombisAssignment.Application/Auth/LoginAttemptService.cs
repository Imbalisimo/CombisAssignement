using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombisAssignment.Application.Auth
{
    public class LoginAttemptService : ILoginAttemptService
    {
        private readonly IMemoryCache _cache;
        private readonly int _maxAttempts;
        private readonly TimeSpan _lockoutDuration;

        public LoginAttemptService(IMemoryCache cache, IConfiguration config)
        {
            _cache = cache;
            var limitSection = config.GetSection("LoginLimiting");
            _maxAttempts = int.Parse(limitSection.GetSection("MaxAttempts").Value);
            _lockoutDuration = TimeSpan.FromMinutes(int.Parse(limitSection.GetSection("LockoutDurationInMinutes").Value));
        }

        public bool IsBlocked(string email)
        {
            var cacheKey = $"login_attempts_{email}";
            if (_cache.TryGetValue(cacheKey, out LoginAttemptInfo attemptInfo))
            {
                if (attemptInfo.Failures >= _maxAttempts && DateTime.UtcNow < attemptInfo.LockoutEnd)
                {
                    return true;
                }
            }
            return false;
        }

        public void RecordFailedAttempt(string email)
        {
            var cacheKey = $"login_attempts_{email}";
            var attemptInfo = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _lockoutDuration;  // Expiry time for cache
                return new LoginAttemptInfo();
            });

            attemptInfo.Failures++;
            attemptInfo.LastFailedAttempt = DateTime.UtcNow;

            if (attemptInfo.Failures >= _maxAttempts)
            {
                attemptInfo.LockoutEnd = DateTime.UtcNow.Add(_lockoutDuration);
            }

            _cache.Set(cacheKey, attemptInfo);
        }

        public void RecordSuccessfulAttempt(string email)
        {
            var cacheKey = $"login_attempts_{email}";
            _cache.Remove(cacheKey);
        }

        private class LoginAttemptInfo
        {
            public int Failures { get; set; } = 0;
            public DateTime LastFailedAttempt { get; set; } = DateTime.MinValue;
            public DateTime LockoutEnd { get; set; } = DateTime.MinValue;
        }
    }
}
