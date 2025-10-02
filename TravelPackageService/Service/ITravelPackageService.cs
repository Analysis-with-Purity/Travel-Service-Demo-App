using TravelPackageService.Core.Entity;

namespace TravelPackageService.Service;

public interface ITravelPackageService
{
    Task<IEnumerable<TravelPackageDto>> GetAvailablePackagesAsync();
     
    Task<BookingDto> BookPackageAsync(int customerId, int travelPackageId, int flightId, int hotelRoomId);
    Task<IEnumerable<HotelDto>> GetHotelsWithRoomsAsync();
    Task<TravelPackageDto?> GetPackageWithDetailsAsync(int id);
    Task<List<TravelPackageDto>> GetPackagesByCustomerIdAsync(int customerId);
}