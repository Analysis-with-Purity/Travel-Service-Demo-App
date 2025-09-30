using Microsoft.EntityFrameworkCore;
using TravelPackageService.Core.Entity;

namespace TravelPackageService.Data;

public class ApplicationDbContext: DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TravelPackage> TravelPackages { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<HotelRoom> HotelRooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.TravelPackage)
                .WithMany(tp => tp.Bookings)
                .HasForeignKey(b => b.TravelPackageId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Flight)
                .WithMany(f => f.Bookings)
                .HasForeignKey(b => b.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.HotelRoom)
                .WithMany(hr => hr.Bookings)
                .HasForeignKey(b => b.HotelRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HotelRoom>()
                .HasOne(hr => hr.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(hr => hr.HotelId)
                .OnDelete(DeleteBehavior.Cascade); 

            
            modelBuilder.Entity<TravelPackage>().HasData(
                new TravelPackage { Id = 1, Name = "Beach Getaway", Description = "Relax in paradise", Price = 500m, StartDate = DateTime.UtcNow.AddDays(30), EndDate = DateTime.UtcNow.AddDays(35) }
            );

            modelBuilder.Entity<Flight>().HasData(
                new Flight { Id = 1, FlightNumber = "AA123", DepartureCity = "NYC", ArrivalCity = "MIA", DepartureTime = DateTime.UtcNow.AddDays(30), Price = 200m }
            );

            modelBuilder.Entity<Hotel>().HasData(
                new Hotel { Id = 1, Name = "Seaside Inn", Location = "Miami Beach" }
            );

            modelBuilder.Entity<HotelRoom>().HasData(
                new HotelRoom { Id = 1, HotelId = 1, RoomType = "Double", AvailableUnits = 10, PricePerNight = 150m }
            );
        }
}