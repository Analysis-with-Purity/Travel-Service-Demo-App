using Microsoft.EntityFrameworkCore;
using Serilog;
using TravelPackageService.Core.Entity;
using TravelPackageService.Data;
using TravelPackageService.Rpositories.IRepository;

namespace TravelPackageService.Service;

public class TravelPackageService:ITravelPackageService
{
      private readonly IGenericRepository<TravelPackage> _packageRepo;
        private readonly IGenericRepository<Customer> _customerRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Hotel> _hotelRepo;
        private readonly ApplicationDbContext _context;

        public TravelPackageService(
            IGenericRepository<TravelPackage> packageRepo,
            IGenericRepository<Customer> customerRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Hotel> hotelRepo,
            ApplicationDbContext context)
        {
            _packageRepo = packageRepo;
            _customerRepo = customerRepo;
            _bookingRepo = bookingRepo;
            _hotelRepo = hotelRepo;
            _context = context;
        }

        //   View available packages
        public async Task<IEnumerable<TravelPackageDto>> GetAvailablePackagesAsync()
    {
        try
        {
            Log.Information("Fetching all available travel packages...");

            var packages = await _context.TravelPackages
                .Include(tp => tp.Bookings)
                .ThenInclude(b => b.Flight)
                .Include(tp => tp.Bookings)
                .ThenInclude(b => b.HotelRoom)
                .ThenInclude(hr => hr.Hotel)
                .Select(tp => new TravelPackageDto
                {
                    Id = tp.Id,
                    Name = tp.Name,
                    Description = tp.Description,
                    Price = tp.Price,
                    StartDate = tp.StartDate,
                    EndDate = tp.EndDate,
                    Bookings = tp.Bookings.Select(b => new BookingDto
                    {
                        Id = b.Id,
                        TotalAmount = b.TotalAmount,
                        BookingDate = b.BookingDate,
                        Status = b.Status.ToString(),
                        FlightNumber = b.Flight!.FlightNumber,
                        HotelName = b.HotelRoom!.Hotel!.Name,
                        RoomType = b.HotelRoom!.RoomType,
                    }).ToList()
                })
                .ToListAsync();

            Log.Information("Fetched {Count} packages", packages.Count);
            if (!packages.Any()) Log.Warning("No available travel packages found.");

            return packages;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching available packages");
            throw;
        }
    }
        //   Register customer
        // public async Task<Customer> RegisterCustomerAsync(Customer customer)
        // {
        //     await _customerRepo.AddAsync(customer);
        //     return customer;
        // }

        //   Book a package with flight + hotel
        public async Task<BookingDto> BookPackageAsync(int customerId, int packageId, int flightId, int roomId)
        {
            try
            {
                Log.Information("Booking package {PackageId} for customer {CustomerId}", packageId, customerId);

                var package = await _context.TravelPackages.FindAsync(packageId);
                var flight = await _context.Flights.FindAsync(flightId);
                var room = await _context.HotelRooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == roomId);

                if (package == null || flight == null || room == null)
                {
                    Log.Warning("Invalid booking details: Package={PackageId}, Flight={FlightId}, Room={RoomId}", packageId, flightId, roomId);
                    throw new Exception("Invalid booking details");
                }

                var booking = new Booking
                {
                    CustomerId = customerId,
                    TravelPackageId = packageId,
                    FlightId = flightId,
                    HotelRoomId = roomId,
                    BookingDate = DateTime.UtcNow,
                    TotalAmount = package.Price + flight.Price + room.PricePerNight
                };

                await _bookingRepo.AddAsync(booking);

                Log.Information("Booking created successfully: BookingId={BookingId}", booking.Id);

                return new BookingDto
                {
                    Id = booking.Id,
                    TotalAmount = booking.TotalAmount,
                    BookingDate = booking.BookingDate,
                    Status = booking.Status.ToString(),
                    FlightNumber = flight.FlightNumber,
                    HotelName = room.Hotel.Name,
                    RoomType = room.RoomType
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error booking package {PackageId} for customer {CustomerId}", packageId, customerId);
                throw;
            }
        }

        //   View hotels with room availability
        public async Task<IEnumerable<HotelDto>> GetHotelsWithRoomsAsync()
        {
            try
            {
                Log.Information("Fetching hotels with rooms...");

                var hotels = await _context.Hotels
                    .Include(h => h.Rooms)
                    .ToListAsync();

                Log.Information("Fetched {Count} hotels", hotels.Count);
                if (!hotels.Any()) Log.Warning("No hotels found in the database.");

                return hotels.Select(h => new HotelDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Location = h.Location,
                    Rooms = h.Rooms.Select(r => new HotelRoomDto
                    {
                        Id = r.Id,
                        RoomType = r.RoomType,
                        AvailableUnits = r.AvailableUnits,
                        PricePerNight = r.PricePerNight
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching hotels with rooms");
                throw;
            }
        }

         //Get package with details (flights + hotels)
         public async Task<TravelPackageDto?> GetPackageWithDetailsAsync(int id)
         {
             try
             {
                 Log.Information("Fetching package with details for PackageId={PackageId}", id);

                 var package = await _context.TravelPackages
                     .Include(tp => tp.Bookings)
                     .ThenInclude(b => b.Flight)
                     .Include(tp => tp.Bookings)
                     .ThenInclude(b => b.HotelRoom)
                     .ThenInclude(hr => hr.Hotel)
                     .FirstOrDefaultAsync(tp => tp.Id == id);

                 if (package == null)
                 {
                     Log.Warning("Package not found: PackageId={PackageId}", id);
                     return null;
                 }

                 Log.Information("Package fetched successfully: PackageId={PackageId}", id);

                 return new TravelPackageDto
                 {
                     Id = package.Id,
                     Name = package.Name,
                     Description = package.Description,
                     Price = package.Price,
                     StartDate = package.StartDate,
                     EndDate = package.EndDate,
                     Bookings = package.Bookings.Select(b => new BookingDto
                     {
                         Id = b.Id,
                         TotalAmount = b.TotalAmount,
                         BookingDate = b.BookingDate,
                         Status = b.Status.ToString(),
                         FlightNumber = b.Flight?.FlightNumber,
                         HotelName = b.HotelRoom?.Hotel?.Name,
                         RoomType = b.HotelRoom?.RoomType,
                     }).ToList()
                 };
             }
             catch (Exception ex)
             {
                 Log.Error(ex, "Error fetching package details for PackageId={PackageId}", id);
                 throw;
             }
         }

         public async Task<List<TravelPackageDto>> GetPackagesByCustomerIdAsync(int customerId)
    {
        try
        {
            Log.Information("Fetching packages for CustomerId={CustomerId}", customerId);

            var packages = await _context.Bookings
                .Where(b => b.CustomerId == customerId && b.TravelPackage != null)
                .Include(b => b.TravelPackage)
                .ThenInclude(tp => tp.Bookings)
                .ThenInclude(b => b.Flight)
                .Include(b => b.TravelPackage)
                .ThenInclude(tp => tp.Bookings)
                .ThenInclude(b => b.HotelRoom)
                .ThenInclude(hr => hr.Hotel)
                .Select(b => b.TravelPackage!)
                .Distinct()
                .ToListAsync();

            Log.Information("Found {Count} packages for CustomerId={CustomerId}", packages.Count, customerId);
            if (!packages.Any()) Log.Warning("No packages found for CustomerId={CustomerId}", customerId);

            return packages.Select(tp => new TravelPackageDto
            {
                Id = tp.Id,
                Name = tp.Name,
                Description = tp.Description,
                Price = tp.Price,
                StartDate = tp.StartDate,
                EndDate = tp.EndDate,
                Bookings = tp.Bookings
                    .Where(b => b.CustomerId == customerId)
                    .Select(b => new BookingDto
                    {
                        Id = b.Id,
                        TotalAmount = b.TotalAmount,
                        BookingDate = b.BookingDate,
                        Status = b.Status.ToString(),
                        FlightNumber = b.Flight?.FlightNumber,
                        HotelName = b.HotelRoom?.Hotel?.Name,
                        RoomType = b.HotelRoom?.RoomType
                    }).ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching packages for CustomerId={CustomerId}", customerId);
            throw;
        }
    }

}




public class TravelPackageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public List<BookingDto> Bookings { get; set; } = new();
}

public class BookingDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FlightNumber { get; set; }
    public string? HotelName { get; set; }
    public string? RoomType { get; set; }
}
public class HotelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<HotelRoomDto> Rooms { get; set; } = new();
}
public class HotelRoomDto
{
    public int Id { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public int AvailableUnits { get; set; }
    public decimal PricePerNight { get; set; }
}