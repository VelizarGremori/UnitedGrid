using Microsoft.AspNetCore.Mvc;

namespace UnitedGrid.Models.Requests
{
    public record GroupAddUserRequest(string GroupId, string UserId);
}
