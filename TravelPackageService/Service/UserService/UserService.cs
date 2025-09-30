using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TravelPackageService.Core.Entity;
using TravelPackageService.Core.Models.Request;
using TravelPackageService.Helper;
using TravelPackageService.Rpositories.IRepository;

namespace TravelPackageService.Service.UserService;

public class UserService : IUserService
{
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly IConfiguration _configuration;

    public UserService(IGenericRepository<Customer> customerRepository, IConfiguration configuration)
    {
        _customerRepository = customerRepository;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterCustomerRequest request)
    {
        // Check if email already exists
        var existingCustomers = await _customerRepository.FindAsync(c => c.Email == request.Email);
        if (existingCustomers.Any())
            return (false, "Email already exists");

        // Create new customer
        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash =  HelperHasherPassword.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer);

       
        return (true, "Registration successful");
    }

    public async Task<(bool Success, string Message, string? Token)> AuthenticateAsync(AuthenticateCustomerRequest request)
    {
        // Find the customer by email
        var customers = await _customerRepository.FindAsync(c => c.Email == request.Email);
        var customer = customers.FirstOrDefault();

        if (customer == null)
            return (false, "Invalid email or password", null);

        // Hash the input password and compare
        var hashedInputPassword = HelperHasherPassword.HashPassword(request.Password);
        if (customer.PasswordHash != hashedInputPassword)
            return (false, "Invalid email or password", null);

        // Generate JWT token if valid
        var token = GenerateJwtToken(customer);
        return (true, "Authentication successful", token);
    }



    private string GenerateJwtToken(Customer customer)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, customer.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}