namespace TravelPackageService.Core.Entity;

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public ICollection<HotelRoom> Rooms { get; set; } = new List<HotelRoom>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>(); // For hotel packages
}