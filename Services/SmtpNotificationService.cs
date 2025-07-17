namespace UnitedGrid.Services;

using Microsoft.AspNetCore.Identity;
using UnitedGrid.Services.Interfaces;
public class SmtpNotificationService : INotificationService
{
    public Task<bool> CanNotifyAsync(IdentityUser user)
    {
        throw new NotImplementedException();
    }

    public Task NotifyAsync(IdentityUser user, string message)
    {
        throw new NotImplementedException();
    }
}
