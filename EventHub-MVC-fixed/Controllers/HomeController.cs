using EventHub.Data;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Controllers;

public class HomeController : Controller
{
    private readonly VenueRepository _venues;
    public HomeController(VenueRepository venues) => _venues = venues;

    public IActionResult Index() => View(_venues.GetAll().Take(3).ToList());

    public IActionResult Error() => View();
}
