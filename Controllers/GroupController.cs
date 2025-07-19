namespace UnitedGrid.Controllers;


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnitedGrid.Data;
using UnitedGrid.Models.Chat;
using UnitedGrid.Models.Chat.Groups;
using UnitedGrid.Models.Requests;

[Authorize]
public class GroupController : Controller
{
    private readonly ApplicationDbContext _db;

    private readonly UserManager<IdentityUser> _userManager;

    public GroupController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<Group>> Index()
    {
        var groups = _db.Groups.ToList();

        return groups;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();

            var userId = _userManager.GetUserId(User);

            var group = new Group
            {
                Name = name
            };

            _db.Groups.Add(group);
            await _db.SaveChangesAsync(); // сначала сохраняем, чтобы получить Id

            var groupUser = new GroupUser
            {
                GroupId = group.Id,
                UserId = userId
            };

            _db.GroupUsers.Add(groupUser);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return RedirectToAction("Chat", "Chat", new { id  = group.Id,  type = ChatType.Group});
        }

        return BadRequest();
    }

    [HttpGet]
    public async Task<IActionResult> SearchUsers(string groupId, string query)
    {
        if (string.IsNullOrWhiteSpace(query) || !long.TryParse(groupId, out var id))
            return BadRequest();

        var currentUserId = _userManager.GetUserId(User);

        var groupUsers = _db.GroupUsers.Where(gu => gu.GroupId == id).Select(gu => gu.UserId);

        var users = await _userManager.Users
            .Where(u => u.UserName.Contains(query) && !groupUsers.Contains(u.Id))
            .Select(u => new
            {
                id = u.Id,
                name = u.GetDisplayName()
            })
            .Take(10)
            .ToListAsync();

        return Json(users);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody]GroupAddUserRequest groupAddUser)
    {
        if (string.IsNullOrWhiteSpace(groupAddUser.UserId) || !long.TryParse(groupAddUser.GroupId, out var id))
            return BadRequest();

        var groupUser = new GroupUser
        {
            GroupId = id,
            UserId = groupAddUser.UserId
        };
        
        _db.GroupUsers.Add(groupUser);
        await _db.SaveChangesAsync();
        
        return Ok();
    }
}
