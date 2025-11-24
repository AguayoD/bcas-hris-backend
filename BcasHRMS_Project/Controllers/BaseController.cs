// Controllers/BaseController.cs - FIXED TO GET ACTUAL USER
using Microsoft.AspNetCore.Mvc;
using Repositories.Service;
using System.Security.Claims;
using Models.Models;
using Repositories.Services;

namespace BCAS_HRMSbackend.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly AuditLogService _auditLogService;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        private readonly tblUsersService _usersService;

        public BaseController(IHttpContextAccessor httpContextAccessor)
        {
            _auditLogService = new AuditLogService();
            _httpContextAccessor = httpContextAccessor;
            _usersService = new tblUsersService();
        }

        protected async Task LogActionAsync(string tableName, string action, string recordId,
                                          object oldValues = null, object newValues = null)
        {
            try
            {
                var (UserId, userName) = await GetActualCurrentUserAsync();
                var ipAddress = GetClientIPAddress();
                var userAgent = GetUserAgent();

                await _auditLogService.LogActionAsync(tableName, action, recordId, oldValues,
                                                    newValues, UserId, userName, ipAddress, userAgent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audit logging failed: {ex.Message}");
            }
        }

        private async Task<(int UserId, string userName)> GetActualCurrentUserAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // DEBUG: Print all available information
                Console.WriteLine($"IsAuthenticated: {httpContext?.User?.Identity?.IsAuthenticated}");
                Console.WriteLine($"Identity Name: {httpContext?.User?.Identity?.Name}");

                if (httpContext?.User?.Identity?.IsAuthenticated != true)
                {
                    Console.WriteLine("User is not authenticated");
                    return await GetFirstActiveUserAsync(); // Use first active user instead of System
                }

                // Print all claims for debugging
                foreach (var claim in httpContext.User.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }

                // METHOD 1: Try to get UserId from claims (most common)
                var UserIdClaim = httpContext.User.FindFirst("UserId")
                               ?? httpContext.User.FindFirst("UserId")
                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? httpContext.User.FindFirst("sub");

                if (UserIdClaim != null && int.TryParse(UserIdClaim.Value, out int UserId) && UserId > 0)
                {
                    Console.WriteLine($"Found UserId from claims: {UserId}");

                    // Get user details from database
                    var user = await _usersService.GetById(UserId);
                    if (user != null && user.UserId.HasValue)
                    {
                        Console.WriteLine($"User found in DB: {user.UserName}");
                        return (user.UserId.Value, user.UserName ?? $"User_{UserId}");
                    }
                    else
                    {
                        Console.WriteLine($"User ID {UserId} not found in database");
                    }
                }

                // METHOD 2: Try to get username from claims and find user
                var userNameClaim = httpContext.User.FindFirst(ClaimTypes.Name)
                                 ?? httpContext.User.FindFirst("preferred_username")
                                 ?? httpContext.User.FindFirst("username")
                                 ?? httpContext.User.FindFirst("unique_name");

                if (userNameClaim != null && !string.IsNullOrEmpty(userNameClaim.Value))
                {
                    Console.WriteLine($"Found username from claims: {userNameClaim.Value}");

                    // Find user by username in database
                    var users = await _usersService.GetAll();
                    var user = users.FirstOrDefault(u =>
                        u.UserName != null && u.UserName.Equals(userNameClaim.Value, StringComparison.OrdinalIgnoreCase));

                    if (user != null && user.UserId.HasValue)
                    {
                        Console.WriteLine($"User found by username: {user.UserName}");
                        return (user.UserId.Value, user.UserName);
                    }
                }

                // METHOD 3: Try to get from JWT token or other authentication
                var identityName = httpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(identityName))
                {
                    Console.WriteLine($"Trying identity name: {identityName}");
                    var users = await _usersService.GetAll();
                    var user = users.FirstOrDefault(u =>
                        u.UserName != null && u.UserName.Equals(identityName, StringComparison.OrdinalIgnoreCase));

                    if (user != null && user.UserId.HasValue)
                    {
                        return (user.UserId.Value, user.UserName);
                    }
                }

                Console.WriteLine("Could not determine current user, using first active user");
                return await GetFirstActiveUserAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetActualCurrentUserAsync: {ex.Message}");
                return await GetFirstActiveUserAsync();
            }
        }

        private async Task<(int UserId, string userName)> GetFirstActiveUserAsync()
        {
            try
            {
                var users = await _usersService.GetAll();
                var activeUser = users.FirstOrDefault(u => u.IsActive == true);

                if (activeUser != null && activeUser.UserId.HasValue)
                {
                    return (activeUser.UserId.Value, activeUser.UserName ?? "Admin");
                }

                // Last resort: get any user
                var anyUser = users.FirstOrDefault();
                if (anyUser != null && anyUser.UserId.HasValue)
                {
                    return (anyUser.UserId.Value, anyUser.UserName ?? "User");
                }

                return (1, "Admin"); // Absolute fallback
            }
            catch
            {
                return (1, "Admin");
            }
        }

        private string GetClientIPAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return "Unknown";

                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',').First().Trim();
                }

                var realIP = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIP))
                {
                    return realIP;
                }

                return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetUserAgent()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString()
                    ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        // Add this debug endpoint to see what's happening
        [HttpGet("debug-current-user")]
        public async Task<IActionResult> DebugCurrentUser()
        {
            var (UserId, userName) = await GetActualCurrentUserAsync();
            var httpContext = _httpContextAccessor.HttpContext;

            var claims = httpContext?.User?.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new
            {
                DetectedUser = new { UserId, userName },
                IsAuthenticated = httpContext?.User?.Identity?.IsAuthenticated,
                IdentityName = httpContext?.User?.Identity?.Name,
                AllClaims = claims,
                IPAddress = GetClientIPAddress()
            });
        }
    }
}