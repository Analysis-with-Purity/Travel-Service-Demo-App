namespace TravelPackageService.Core.Entity;

public class HotelRoom
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string RoomType { get; set; } = string.Empty; // e.g., "Single", "Double"
    public int AvailableUnits { get; set; } // For availability checks
    public decimal PricePerNight { get; set; }
    public Hotel Hotel { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}