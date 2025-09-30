using TravelPackageService.Core.Enum;

namespace TravelPackageService.Core.Entity;

public class Booking
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? TravelPackageId { get; set; }  
    public TravelPackage? TravelPackage { get; set; }
    public int? FlightId { get; set; }
    public Flight? Flight { get; set; }
    public int? HotelRoomId { get; set; }
    public HotelRoom? HotelRoom { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }  
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;  
}