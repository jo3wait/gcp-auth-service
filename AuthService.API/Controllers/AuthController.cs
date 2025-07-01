using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMemberService _auth;

    public AuthController(IMemberService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var res = await _auth.RegisterAsync(request);
        //if (!res.Success) return BadRequest(res);
        return Ok(res);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var res = await _auth.LoginAsync(request);
        //if (!res.Success) return Unauthorized(res);
        return Ok(res);
    }
}
