using Microsoft.AspNetCore.Mvc;
using Hospital_Patient_Intake_Scheduling_API_604._23E.DTOs;
using Hospital_Patient_Intake_Scheduling_API_604._23E.Interfaces;

namespace Hospital_Patient_Intake_Scheduling_API_604._23E.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (result == null) return Unauthorized("Invalid username or password");

            return Ok(result);
        }
    }
}