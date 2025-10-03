using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPackageService.Core.Entity;
using TravelPackageService.Service;

namespace TravelPackageService.Controllers;

[Authorize]
public class TravelPackageController : Controller
{
    private readonly ITravelPackageService _service;

    public TravelPackageController(ITravelPackageService service)
    {
        _service = service;
    }

    [HttpGet("packages")]
    public async Task<IActionResult> GetPackages() =>
        Ok(await _service.GetAvailablePackagesAsync());

    

    [HttpPost("book")]
    public async Task<IActionResult> Book(int customerId, int packageId, int flightId, int roomId) =>
        Ok(await _service.BookPackageAsync(customerId, packageId, flightId, roomId));

    [HttpGet("hotels")]
    public async Task<IActionResult> GetHotels() =>
        Ok(await _service.GetHotelsWithRoomsAsync());

    [HttpGet("packages/{id}")]
    public async Task<IActionResult> GetPackageDetails(int id) =>
        Ok(await _service.GetPackageWithDetailsAsync(id));
    
    [HttpGet("Allpackages/{customerId}")]
    public async Task<IActionResult> GetPackagesByDetailsCustomerIdAsync(int customerId) =>
        Ok(await _service.GetPackagesByCustomerIdAsync(customerId));
    
    
}
