namespace TravelPackageService.Core.Models.Request;

public class RegisterCustomerRequest
{
    public string Name { get; set; } = string.Empty;
     
    public string Email { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty; 
}