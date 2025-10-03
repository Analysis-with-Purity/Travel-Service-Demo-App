using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Serilog;
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
        try
        {
            Log.Information("Registering new customer with email {Email}", request.Email);

            // Check if email already exists
            var existingCustomers = await _customerRepository.FindAsync(c => c.Email == request.Email);
            Log.Information("Found {Count} existing users with email {Email}", existingCustomers.Count(), request.Email);

            if (existingCustomers.Any())
            {
                Log.Warning("Registration failed. Email already exists: {Email}", request.Email);
                return (false, "Email already exists");
            }

            // Create new customer
            var customer = new Customer
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = HelperHasherPassword.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _customerRepository.AddAsync(customer);
            Log.Information("Customer registered successfully: {CustomerId}", customer.Id);

            return (true, "Registration successful");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering customer with email {Email}", request.Email);
            throw;
        }
    }

    public async Task<(bool Success, string Message, string? Token)> AuthenticateAsync(AuthenticateCustomerRequest request)
    {
        try
        {
            Log.Information("Authenticating customer with email {Email}", request.Email);

            // Find the customer by email
            var customers = await _customerRepository.FindAsync(c => c.Email == request.Email);
            Log.Information("Found {Count} customers with email {Email}", customers.Count(), request.Email);

            var customer = customers.FirstOrDefault();
            if (customer == null)
            {
                Log.Warning("Authentication failed. Email not found: {Email}", request.Email);
                return (false, "Invalid email or password", null);
            }

            // Hash the input password and compare
            var hashedInputPassword = HelperHasherPassword.HashPassword(request.Password);
            Log.Debug("Comparing hashed passwords for email {Email}", request.Email);

            if (customer.PasswordHash != hashedInputPassword)
            {
                Log.Warning("Authentication failed. Invalid password for email: {Email}", request.Email);
                return (false, "Invalid email or password", null);
            }

            // Generate token 
            var token = GenerateJwtToken(customer);
            Log.Information("Customer authenticated successfully: {CustomerId}", customer.Id);

            return (true, "Authentication successful", token);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error authenticating customer with email {Email}", request.Email);
            throw;
        }
    }

    private string GenerateJwtToken(Customer customer)
    {
        try
        {
            Log.Information("Generating JWT token for customer {CustomerId}", customer.Id);

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

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Log.Information("JWT token generated successfully for customer {CustomerId}, TokenPreview={TokenPreview}", customer.Id, tokenString.Substring(0, 10) + "...");

            return tokenString;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating JWT token for customer {CustomerId}", customer.Id);
            throw;
        }
    }
}
