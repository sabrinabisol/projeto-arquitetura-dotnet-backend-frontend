using System;
using AppProject.Core.Contracts;

namespace AppProject.Core.API.Auth;

public class UserContext : IUserContext
{
    public Task<UserInfo> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UserInfo> GetSystemAdminUserAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
