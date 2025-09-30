namespace TravelPackageService.Core.Entity;

public class TravelPackage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}