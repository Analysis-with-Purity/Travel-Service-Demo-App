using Microsoft.AspNetCore.Mvc;
using TravelPackageService.Core.Models.Request;
using TravelPackageService.Service.UserService;

namespace TravelPackageService.Controllers;

    // GET
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Register a new customer
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterAsync(request);
            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(new
            {
                result.Message,
                
            });
        }

        /// <summary>
        /// Authenticate an existing customer
        /// </summary>
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.AuthenticateAsync(request);
            if (!result.Success)
                return Unauthorized(new { result.Message });

            return Ok(new
            {
                result.Message,
                Token = result.Token
            });
        }
    }
