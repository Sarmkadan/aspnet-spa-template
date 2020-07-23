// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// API controller for user management and authentication.
/// </summary>
public class UsersController : ApiControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    [ProduceResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return ApiSuccess(user, "User registered successfully", StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    [ProduceResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _userService.AuthenticateAsync(request);
        return ApiSuccess(response, "Login successful");
    }

    [HttpGet("{id:int}")]
    [ProduceResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return ApiSuccess(user);
    }

    [HttpGet("profile")]
    [ProduceResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
    {
        if (!IsAuthenticated)
            return Unauthorized();

        var userId = GetUserId();
        var user = await _userService.GetUserByIdAsync(userId);
        return ApiSuccess(user);
    }

    [HttpPut("{id:int}")]
    [ProduceResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (!IsAuthenticated)
            return Unauthorized();

        var userId = GetUserId();
        if (userId != id)
            return Forbid();

        var user = await _userService.UpdateUserAsync(id, request);
        return ApiSuccess(user, "Profile updated successfully");
    }

    [HttpPut("{id:int}/deactivate")]
    [ProduceResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        if (!IsAuthenticated)
            return Unauthorized();

        await _userService.DeactivateUserAsync(id);
        return NoContent();
    }

    [HttpPut("{id:int}/activate")]
    [ProduceResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivateUser(int id)
    {
        if (!IsAuthenticated)
            return Unauthorized();

        await _userService.ActivateUserAsync(id);
        return NoContent();
    }

    [HttpGet]
    [ProduceResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return ApiSuccess(users);
    }

    [HttpGet("recently-active")]
    [ProduceResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentlyActiveUsers([FromQuery] int days = 30)
    {
        var users = await _userService.GetRecentlyActiveUsersAsync(days);
        return ApiSuccess(users);
    }
}
