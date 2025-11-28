// Controllers/BaseController.cs - UPDATED FOR TRANSACTION EVENT AUDITING
using Microsoft.AspNetCore.Mvc;
using Repositories.Service;
using System.Security.Claims;
using Models.Models;
using Repositories.Services;
using Models.DTOs.UsersDTO;

namespace BCAS_HRMSbackend.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly AuditLogService _auditLogService;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly TransactionEventService _transactionEventService;
        private readonly tblUsersService _usersService;

        public BaseController(IHttpContextAccessor httpContextAccessor)
        {
            _auditLogService = new AuditLogService();
            _httpContextAccessor = httpContextAccessor;
            _usersService = new tblUsersService();
            _transactionEventService = null; // Will be set by derived controllers that need it
        }

        public BaseController(IHttpContextAccessor httpContextAccessor, TransactionEventService transactionEventService)
        {
            _auditLogService = new AuditLogService();
            _httpContextAccessor = httpContextAccessor;
            _usersService = new tblUsersService();
            _transactionEventService = transactionEventService;
        }

        // Keep your existing LogActionAsync method for backward compatibility
        protected async Task LogActionAsync(string tableName, string action, string recordId,
                                          object oldValues = null, object newValues = null)
        {
            try
            {
                var (UserId, userName) = await GetActualCurrentUserAsync();
                var ipAddress = GetClientIPAddress();
                var userAgent = GetUserAgent();

                string description = BuildAuditDescription(
                    action,
                    "EmployeeName",     // or pass dynamic column
                    oldValues?.ToString(),
                    newValues?.ToString()
                );

                await _auditLogService.LogActionAsync(
                    tableName,
                    action,
                    recordId,
                    description,
                    UserId,
                    userName,
                    ipAddress,
                    userAgent
                );

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audit logging failed: {ex.Message}");
            }
        }

        // NEW: Transaction Event logging method for the updated controllers
        protected async Task LogTransactionEvent(string action, UserRolesDTOV2 user, int employeeId,
            string description, object oldData, object newData)
        {
            if (_transactionEventService != null)
            {
                try
                {
                    string changes = "";

                    // Generate changes description if both old and new data are provided
                    if (oldData != null && newData != null)
                    {
                        changes = GenerateChangesDescription(oldData, newData);
                    }

                    await _transactionEventService.InsertAsync(new TransactionEvent
                    {
                        Action = action,
                        Description = !string.IsNullOrEmpty(changes)
                            ? $"{user.Username} {action}: {changes}"
                            : $"{user.Username} {action}: {description}",
                        UserID = user.UserId,
                        UserName = user.Username ?? "Unknown",
                        Fullname = GetFullnameFromData(newData ?? oldData),
                        Timestamp = DateTime.Now
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Transaction event logging failed: {ex.Message}");
                }
            }
        }

        // Helper method to generate changes description
        private string GenerateChangesDescription(object oldData, object newData)
        {
            try
            {
                var changes = new List<string>();
                var properties = oldData.GetType().GetProperties();

                foreach (var prop in properties)
                {
                    // Skip sensitive properties
                    if (prop.Name == "PasswordHash" || prop.Name == "Salt" ||
                        prop.Name == "LastUpdatedAt" || prop.Name == "LastUpdatedBy")
                        continue;

                    var oldValue = prop.GetValue(oldData)?.ToString() ?? "";
                    var newValue = prop.GetValue(newData)?.ToString() ?? "";

                    if (oldValue != newValue)
                    {
                        changes.Add($"{prop.Name}: {oldValue} → {newValue}");
                    }
                }

                return changes.Count > 0 ? string.Join(" | ", changes) : "No changes detected";
            }
            catch
            {
                return "Changes detection failed";
            }
        }

        // Helper method to extract fullname from data objects
        private string GetFullnameFromData(object data)
        {
            if (data == null) return "Unknown";

            try
            {
                var type = data.GetType();

                // Try to get FirstName and LastName properties
                var firstNameProp = type.GetProperty("FirstName");
                var lastNameProp = type.GetProperty("LastName");

                if (firstNameProp != null && lastNameProp != null)
                {
                    var firstName = firstNameProp.GetValue(data)?.ToString() ?? "";
                    var lastName = lastNameProp.GetValue(data)?.ToString() ?? "";
                    return $"{firstName} {lastName}".Trim();
                }

                // Try to get UserName property
                var userNameProp = type.GetProperty("UserName");
                if (userNameProp != null)
                {
                    return userNameProp.GetValue(data)?.ToString() ?? "Unknown";
                }

                // Try to get DepartmentName property
                var deptNameProp = type.GetProperty("DepartmentName");
                if (deptNameProp != null)
                {
                    return deptNameProp.GetValue(data)?.ToString() ?? "Unknown";
                }

                // Try to get PositionName property
                var positionNameProp = type.GetProperty("PositionName");
                if (positionNameProp != null)
                {
                    return positionNameProp.GetValue(data)?.ToString() ?? "Unknown";
                }

                // Try to get StatusName property
                var statusNameProp = type.GetProperty("StatusName");
                if (statusNameProp != null)
                {
                    return statusNameProp.GetValue(data)?.ToString() ?? "Unknown";
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
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

        protected string BuildAuditDescription(string action, string columnName, string oldValue, string newValue)
        {
            return action.ToUpper() switch
            {
                "UPDATE" =>
                    $"{columnName}, \"{oldValue}\" → \"{newValue}\"",

                "DELETE" =>
                    $"Deleted: \"{oldValue}\"",

                "INSERT" =>
                    $"Added: \"{newValue}\"",

                _ =>
                    $"{action}: \"{newValue ?? oldValue}\""
            };
        }
    }
}