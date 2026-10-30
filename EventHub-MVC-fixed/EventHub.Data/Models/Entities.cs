using System.ComponentModel.DataAnnotations;

namespace EventHub.Models;

public static class Roles
{
    public const string Admin = "ADMIN";
    public const string Coordinator = "COORDINATOR";
    public const string Client = "CLIENT";
    public const string Staff = "ADMIN,COORDINATOR";          // dung cho [Authorize(Roles = Roles.Staff)]
    public static readonly string[] All = { Admin, Coordinator, Client };
}

public static class BookingStatus
{
    public const string Pending = "PENDING";
    public const string Confirmed = "CONFIRMED";
    public const string Completed = "COMPLETED";
    public const string Cancelled = "CANCELLED";
    public static readonly string[] All = { Pending, Confirmed, Completed, Cancelled };
}

public class User
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = Roles.Client;
}

public class Venue
{
    public long Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    [Required, StringLength(500)]
    public string Address { get; set; } = "";

    [Range(1, 100000)]
    public int Capacity { get; set; }

    [Range(0, 1000000000)]
    public decimal Price { get; set; }

    public string Status { get; set; } = "AVAILABLE";   // AVAILABLE | MAINTENANCE

    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }
}

public class Booking
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long VenueId { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int GuestCount { get; set; }
    public string Status { get; set; } = BookingStatus.Pending;
    public string? Notes { get; set; }

    // lay tu JOIN, chi de hien thi
    public string VenueName { get; set; } = "";
    public string UserName { get; set; } = "";
}
