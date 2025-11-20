// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// Base controller for API endpoints.
/// </summary>
[ApiController]
[Route(AppConstants.ApiBaseRoute + "/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ApiSuccess<T>(T data, string message = "Success", int statusCode = 200)
    {
        return StatusCode(statusCode, new SuccessResponse<T>(data, message));
    }

    protected IActionResult ApiError(string message, string errorCode = "ERROR", int statusCode = 400)
    {
        return StatusCode(statusCode, new ErrorResponse(message, errorCode, statusCode));
    }

    protected IActionResult ApiError(string message, Dictionary<string, List<string>> errors, int statusCode = 400)
    {
        return StatusCode(statusCode, new ErrorResponse(message, errors, statusCode));
    }

    protected int GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        throw new UnauthorizedAccessException("User ID not found in claims");
    }

    protected string? GetUserEmail()
    {
        return User.FindFirst("email")?.Value;
    }

    protected bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
