using System.Security.Claims;
using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EventHub.Controllers;

// Ca controller chi danh cho ADMIN
[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly UserRepository _users;
    private readonly BookingRepository _bookings;

    public AdminController(UserRepository users, BookingRepository bookings)
    {
        _users = users;
        _bookings = bookings;
    }

    private long CurrentUserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public IActionResult Users() => View(_users.GetAll());

    public IActionResult Dashboard() => View(_bookings.GetAll());

    [HttpPost]
    public IActionResult SetRole(long id, string role)
    {
        if (id == CurrentUserId) TempData["Error"] = "You cannot change your own role.";
        else if (!Roles.All.Contains(role)) return BadRequest();
        else { _users.SetRole(id, role); TempData["Success"] = "Role updated."; }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    public IActionResult DeleteUser(long id)
    {
        if (id == CurrentUserId) { TempData["Error"] = "You cannot delete yourself."; return RedirectToAction(nameof(Users)); }
        try { _users.Delete(id); TempData["Success"] = "User deleted."; }
        catch (SqlException) { TempData["Error"] = "Cannot delete: this user has bookings."; }
        return RedirectToAction(nameof(Users));
    }
}
