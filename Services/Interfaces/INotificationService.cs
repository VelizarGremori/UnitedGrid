namespace UnitedGrid.Services.Interfaces;

using Microsoft.AspNetCore.Identity;

public interface INotificationService
{
    public Task<bool> CanNotifyAsync(IdentityUser user);
    public Task NotifyAsync(IdentityUser user, string message);
}
