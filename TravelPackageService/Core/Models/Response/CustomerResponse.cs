using TravelPackageService.Core.Entity;

namespace TravelPackageService.Core.Models.Response;

public class CustomerResponse
{
    public int Id { get; set; }
     
    public string Name { get; set; } = string.Empty;
     
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;  
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}