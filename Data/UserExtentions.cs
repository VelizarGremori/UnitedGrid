namespace UnitedGrid.Data;

using Microsoft.AspNetCore.Identity;

public static class UserExtentions
{
    public static string GetDisplayName(this IdentityUser user) => user.UserName ?? user.Email ?? "Unknown";
}