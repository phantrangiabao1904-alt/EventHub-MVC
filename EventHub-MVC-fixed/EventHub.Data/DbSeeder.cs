using EventHub.Models;

namespace EventHub.Data;

/// <summary>Tao tai khoan + venue mau neu bang con trong (chay luc khoi dong).</summary>
public static class DbSeeder
{
    public static void Seed(UserRepository users, VenueRepository venues)
    {
        if (users.CountAll() == 0)
        {
            users.Create(new User { Name = "Admin", Email = "admin@eventhub.com", PasswordHash = UserRepository.HashPassword("Admin@123"), Role = Roles.Admin });
            users.Create(new User { Name = "Coordinator", Email = "coordinator@eventhub.com", PasswordHash = UserRepository.HashPassword("Coord@123"), Role = Roles.Coordinator });
            users.Create(new User { Name = "Client", Email = "client@eventhub.com", PasswordHash = UserRepository.HashPassword("Client@123"), Role = Roles.Client });
        }

        if (venues.CountAll() == 0)
        {
            venues.Create(new Venue { Name = "Grand Ballroom", Description = "Elegant ballroom for weddings and galas.", Address = "12 Nguyen Hue, District 1, HCMC", Capacity = 500, Price = 25000000 });
            venues.Create(new Venue { Name = "Sunset Garden", Description = "Open-air garden venue.", Address = "88 Tran Phu, Da Nang", Capacity = 200, Price = 12000000 });
            venues.Create(new Venue { Name = "Meeting Room A", Description = "Small room for workshops.", Address = "5 Le Loi, Hanoi", Capacity = 30, Price = 2000000 });
        }
    }
}
