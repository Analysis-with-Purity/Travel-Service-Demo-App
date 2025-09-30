namespace TravelPackageService.Core.Models.Request;

public class AuthenticateCustomerRequest
{
    public string Email { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;  
}