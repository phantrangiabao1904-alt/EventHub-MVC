using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Controllers;

[Authorize(Roles = Roles.Staff)]
public class CoordinatorController : Controller
{
    private readonly BookingRepository _bookings;
    public CoordinatorController(BookingRepository bookings) => _bookings = bookings;

    // Optional filters: search (venue/user/id) and status
    public IActionResult Index(string? search = null, string? status = null)
    {
        var list = _bookings.GetAll();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            list = list.Where(b => (b.VenueName ?? "").ToLowerInvariant().Contains(s)
                                 || (b.UserName ?? "").ToLowerInvariant().Contains(s)
                                 || b.Id.ToString() == s).ToList();
        }
        if (!string.IsNullOrWhiteSpace(status)) list = list.Where(b => b.Status == status).ToList();
        ViewData["Search"] = search;
        ViewData["Status"] = status;
        return View(list);
    }
}
