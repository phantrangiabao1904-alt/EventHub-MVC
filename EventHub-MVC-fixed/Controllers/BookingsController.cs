using System.Security.Claims;
using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly BookingRepository _bookings;
    private readonly VenueRepository _venues;

    public BookingsController(BookingRepository bookings, VenueRepository venues)
    {
        _bookings = bookings;
        _venues = venues;
    }

    private long CurrentUserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsStaff => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Coordinator);

    // CLIENT thay booking cua minh; ADMIN/COORDINATOR thay tat ca
    public IActionResult Index() =>
        View(IsStaff ? _bookings.GetAll() : _bookings.GetByUser(CurrentUserId));

    // ---- Chi CLIENT dat venue ----
    [Authorize(Roles = Roles.Client)]
    public IActionResult Create(long venueId)
    {
        var venue = _venues.GetById(venueId);
        if (venue == null) return NotFound();
        return View(ToVm(venue));
    }

    [HttpPost, Authorize(Roles = Roles.Client)]
    public IActionResult Create(BookingCreateVm vm)
    {
        var venue = _venues.GetById(vm.VenueId);
        if (venue == null) return NotFound();

        // validate nghiep vu
        if (venue.Status != "AVAILABLE")
            ModelState.AddModelError("", "This venue is under maintenance.");
        if (vm.EventDate.Date < DateTime.Today)
            ModelState.AddModelError(nameof(vm.EventDate), "Event date cannot be in the past.");
        if (vm.EndTime <= vm.StartTime)
            ModelState.AddModelError(nameof(vm.EndTime), "End time must be after start time.");
        if (vm.GuestCount > venue.Capacity)
            ModelState.AddModelError(nameof(vm.GuestCount), $"Maximum {venue.Capacity} guests for this venue.");
        if (ModelState.IsValid && _bookings.HasConflict(venue.Id, vm.EventDate, vm.StartTime, vm.EndTime))
            ModelState.AddModelError("", "This venue is already booked for that time slot.");

        if (!ModelState.IsValid)
        {
            vm.VenueName = venue.Name;
            vm.VenueCapacity = venue.Capacity;
            return View(vm);
        }

        _bookings.Create(new Booking
        {
            UserId = CurrentUserId, VenueId = venue.Id, EventDate = vm.EventDate,
            StartTime = vm.StartTime, EndTime = vm.EndTime, GuestCount = vm.GuestCount, Notes = vm.Notes
        });
        TempData["Success"] = "Booking submitted. Waiting for confirmation.";
        return RedirectToAction(nameof(Index));
    }

    // ---- Huy: CLIENT huy booking cua minh, staff huy bat ky ----
    [HttpPost]
    public IActionResult Cancel(long id)
    {
        var b = _bookings.GetById(id);
        if (b == null) return NotFound();
        if (!IsStaff && b.UserId != CurrentUserId) return Forbid();

        if (b.Status is BookingStatus.Pending or BookingStatus.Confirmed)
        {
            _bookings.UpdateStatus(id, BookingStatus.Cancelled);
            TempData["Success"] = "Booking cancelled.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ---- Doi trang thai: chi ADMIN / COORDINATOR ----
    [HttpPost, Authorize(Roles = Roles.Staff)]
    public IActionResult SetStatus(long id, string status)
    {
        if (!BookingStatus.All.Contains(status)) return BadRequest();
        var b = _bookings.GetById(id);
        if (b == null) return NotFound();

        if (b.Status is BookingStatus.Cancelled or BookingStatus.Completed)
            TempData["Error"] = $"Booking #{id} is already {b.Status.ToLower()} and cannot be changed.";
        else
        {
            _bookings.UpdateStatus(id, status);
            TempData["Success"] = "Status updated.";
        }

        // quay lai trang vua bam (Coordinator hoac Admin Dashboard)
        var referer = Request.Headers.Referer.ToString();
        if (Uri.TryCreate(referer, UriKind.Absolute, out var uri) && uri.Host == Request.Host.Host)
            return LocalRedirect(uri.PathAndQuery);
        return RedirectToAction(nameof(Index));
    }

    private static BookingCreateVm ToVm(Venue v) => new()
    {
        VenueId = v.Id, VenueName = v.Name, VenueCapacity = v.Capacity
    };
}
