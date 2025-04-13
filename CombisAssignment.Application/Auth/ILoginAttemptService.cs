using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombisAssignment.Application.Auth
{
    public interface ILoginAttemptService
    {
        bool IsBlocked(string email);
        void RecordFailedAttempt(string email);
        void RecordSuccessfulAttempt(string email);
    }
}
