using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EventHub.Controllers;

public class VenuesController : Controller
{
    private readonly VenueRepository _venues;
    public VenuesController(VenueRepository venues) => _venues = venues;

    [Authorize(Roles = Roles.Staff)]
    public IActionResult Manage(string? search) {
        ViewData["Search"] = search;
        return View("Manage", _venues.GetAll(search));
    }

    // ---- Ai cung xem duoc ----
    public IActionResult Index(string? search, int? capacity)
    {
        ViewData["Search"] = search;
        ViewData["Capacity"] = capacity;
        var list = _venues.GetAll(search);
        if (capacity.HasValue) list = list.Where(v => v.Capacity >= capacity.Value).ToList();
        return View(list);
    }

    public IActionResult Details(long id)
    {
        var venue = _venues.GetById(id);
        return venue == null ? NotFound() : View(venue);
    }

    // ---- Chi ADMIN / COORDINATOR ----
    [Authorize(Roles = Roles.Staff)]
    public IActionResult Create() => View("Form", new Venue());

    [HttpPost, Authorize(Roles = Roles.Staff)]
    public IActionResult Create(Venue venue)
    {
        if (!ModelState.IsValid) return View("Form", venue);
        _venues.Create(venue);
        TempData["Success"] = "Venue created.";
        return RedirectToAction(nameof(Manage));
    }

    [Authorize(Roles = Roles.Staff)]
    public IActionResult Edit(long id)
    {
        var venue = _venues.GetById(id);
        return venue == null ? NotFound() : View("Form", venue);
    }

    [HttpPost, Authorize(Roles = Roles.Staff)]
    public IActionResult Edit(long id, Venue venue)
    {
        venue.Id = id;
        if (!ModelState.IsValid) return View("Form", venue);
        _venues.Update(venue);
        TempData["Success"] = "Venue updated.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost, Authorize(Roles = Roles.Staff)]
    public IActionResult Delete(long id)
    {
        try
        {
            _venues.Delete(id);
            TempData["Success"] = "Venue deleted.";
        }
        catch (SqlException)
        {
            TempData["Error"] = "Cannot delete: this venue already has bookings. Set it to MAINTENANCE instead.";
        }
        return RedirectToAction(nameof(Manage));
    }
}
