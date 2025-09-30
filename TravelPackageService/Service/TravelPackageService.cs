using Microsoft.EntityFrameworkCore;
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
            return await _context.TravelPackages
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
                        HotelName = b.HotelRoom!.Hotel.Name,
                        RoomType = b.HotelRoom!.RoomType,  
                    }).ToList()
                })
                .ToListAsync();
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
            var package = await _context.TravelPackages.FindAsync(packageId);
            var flight = await _context.Flights.FindAsync(flightId);
            var room = await _context.HotelRooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == roomId);

            if (package == null || flight == null || room == null)
                throw new Exception("Invalid booking details");

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

        //   View hotels with room availability
        public async Task<IEnumerable<HotelDto>> GetHotelsWithRoomsAsync()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .ToListAsync();

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
                    // Notice: no Hotel here, because hotel info is already at parent level
                }).ToList()
            });
        }

         //Get package with details (flights + hotels)
         public async Task<TravelPackageDto?> GetPackageWithDetailsAsync(int id)
         {
             var package = await _context.TravelPackages
                 .Include(tp => tp.Bookings)
                 .ThenInclude(b => b.Flight)
                 .Include(tp => tp.Bookings)
                 .ThenInclude(b => b.HotelRoom)
                 .ThenInclude(hr => hr.Hotel)
                 .FirstOrDefaultAsync(tp => tp.Id == id);

             if (package == null) return null;

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