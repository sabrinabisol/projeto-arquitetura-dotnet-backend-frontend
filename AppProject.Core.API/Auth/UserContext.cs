using System;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AppProject.Core.API.Auth;

public class UserContext(
    ApplicationDbContext applicationDbContext)
    : IUserContext
{
    public Task<UserInfo> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<UserInfo> GetSystemAdminUserAsync(CancellationToken cancellationToken = default)
    {
        var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.IsSystemAdmin, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("System admin user not found");
        }

        return new UserInfo
        {
            UserId = user.Id,
            UserName = user.Name,
            Email = user.Email,
            IsSystemAdmin = true
        };
    }
}
