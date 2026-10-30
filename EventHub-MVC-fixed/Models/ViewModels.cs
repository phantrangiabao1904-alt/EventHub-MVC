using System.ComponentModel.DataAnnotations;

namespace EventHub.Models;

public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}

public class RegisterVm
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6), DataType(DataType.Password)] public string Password { get; set; } = "";
}

public class BookingCreateVm
{
    public long VenueId { get; set; }
    public string? VenueName { get; set; }
    public int VenueCapacity { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Event date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(1);

    [Required, Display(Name = "Start time")]
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);

    [Required, Display(Name = "End time")]
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);

    [Range(1, 100000), Display(Name = "Guests")]
    public int GuestCount { get; set; } = 10;

    public string? Notes { get; set; }
}
