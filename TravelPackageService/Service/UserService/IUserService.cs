using TravelPackageService.Core.Models.Request;

namespace TravelPackageService.Service.UserService;

public interface IUserService
{ 
        Task<(bool Success, string Message)> RegisterAsync(RegisterCustomerRequest request); 
        Task<(bool Success, string Message, string? Token)> AuthenticateAsync( AuthenticateCustomerRequest request); 
}