using ConnectSphere.Auth.API.Services;
using ConnectSphere.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConnectSphere.Auth.API.DTOs;

namespace ConnectSphere.Auth.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    public UserController(IUserService service) => _service = service;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (token, refresh) = await _service.RegisterAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { token, refreshToken = refresh }));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (token, refresh) = await _service.LoginAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { token, refreshToken = refresh }));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ",
"");
        await _service.LogoutAsync(CurrentUserId, token);
        return Ok(ApiResponse<string>.Ok("Logged out."));
    }

    [HttpPost("oauth/google")]
    public async Task<IActionResult> GoogleOAuth([FromBody] GoogleOAuthDto dto)
    {
        var (token, refresh) = await _service.GoogleOAuthAsync(dto.IdToken);
        return Ok(ApiResponse<object>.Ok(new { token, refreshToken = refresh }));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var token = await _service.RefreshTokenAsync(dto.RefreshToken);
        return Ok(ApiResponse<object>.Ok(new { token }));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpGet("username/{name}")]
    public async Task<IActionResult> GetByUsername(string name)
    {
        var user = await _service.GetByUserNameAsync(name);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var users = await _service.SearchAsync(q);
        return Ok(ApiResponse<IList<UserDto>>.Ok(users));
    }

    [HttpGet("{id:int}/suggested")]
    [Authorize]
    public async Task<IActionResult> GetSuggested(int id)
    {
        var users = await _service.GetSuggestedUsersAsync(id);
        return Ok(ApiResponse<IList<UserDto>>.Ok(users));
    }

    [HttpPut("{id:int}/profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(int id, [FromForm]
UpdateProfileDto dto)
    {
        if (CurrentUserId != id) return Forbid();
        var user = await _service.UpdateProfileAsync(id, dto);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpPut("{id:int}/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(int id, [FromBody]
ChangePasswordDto dto)
    {
        if (CurrentUserId != id) return Forbid();
        await _service.ChangePasswordAsync(id, dto);
        return Ok(ApiResponse<string>.Ok("Password changed."));
    }

    [HttpPut("{id:int}/privacy")]
    [Authorize]
    public async Task<IActionResult> TogglePrivacy(int id)
    {
        if (CurrentUserId != id) return Forbid();
        var isPrivate = await _service.TogglePrivacyAsync(id);
        return Ok(ApiResponse<bool>.Ok(isPrivate));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        if (CurrentUserId != id) return Forbid();
        await _service.DeactivateAccountAsync(id);
        return Ok(ApiResponse<string>.Ok("Account deactivated."));
    }
    // Internal endpoint for service-to-service calls — no auth required
    // Only reachable within the internal network, not exposed through the Gateway
    [HttpGet("{id:int}/internal")]
    public async Task<IActionResult> GetByIdInternal(int id)
    {
        var user = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }
}