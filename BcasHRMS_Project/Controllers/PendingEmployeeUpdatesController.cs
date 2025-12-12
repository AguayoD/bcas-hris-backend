// [file name]: PendingEmployeeUpdatesController.cs
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.UsersDTO;
using Models.DTOs;
using Models.Models;
using Repositories.Service;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs;
using Models.DTOs.UsersDTO;
using Models.Models;
using Repositories.Service;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PendingEmployeeUpdatesController : BaseController
    {
        private readonly PendingEmployeeUpdateService _pendingUpdateService;
        private readonly tblEmployeeService _employeeService;
        private readonly TransactionEventService _transactionEventService;

        public PendingEmployeeUpdatesController(
            IHttpContextAccessor httpContextAccessor,
            PendingEmployeeUpdateService pendingUpdateService,
            tblEmployeeService employeeService,
            TransactionEventService transactionEventService
        ) : base(httpContextAccessor)
        {
            _pendingUpdateService = pendingUpdateService;
            _employeeService = employeeService;
            _transactionEventService = transactionEventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _pendingUpdateService.GetAllAsync();
                var response = new List<PendingUpdateResponseDTO>();

                foreach (var item in data)
                {
                    var dto = new PendingUpdateResponseDTO
                    {
                        PendingUpdateID = item.PendingUpdateID,
                        EmployeeID = item.EmployeeID,
                        UpdateData = JsonSerializer.Deserialize<Dictionary<string, object>>(item.UpdateData) ?? new Dictionary<string, object>(),
                        OriginalData = JsonSerializer.Deserialize<Dictionary<string, object>>(item.OriginalData) ?? new Dictionary<string, object>(),
                        Status = item.Status,
                        SubmittedAt = item.SubmittedAt,
                        ReviewedAt = item.ReviewedAt,
                        ReviewedBy = item.ReviewedBy,
                        ReviewerName = item.ReviewerName,
                        Comments = item.Comments
                    };

                    if (item.Employee != null)
                    {
                        dto.Employee = new EmployeeBasicDTO
                        {
                            EmployeeID = item.Employee.EmployeeID ?? 0,
                            FirstName = item.Employee.FirstName,
                            LastName = item.Employee.LastName,
                            Email = item.Employee.Email,
                            PositionID = item.Employee.PositionID,
                            DepartmentID = item.Employee.DepartmentID
                        };
                    }

                    response.Add(dto);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAll: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                var data = await _pendingUpdateService.GetPendingUpdatesAsync();
                var response = new List<PendingUpdateResponseDTO>();

                foreach (var item in data)
                {
                    var dto = new PendingUpdateResponseDTO
                    {
                        PendingUpdateID = item.PendingUpdateID,
                        EmployeeID = item.EmployeeID,
                        UpdateData = JsonSerializer.Deserialize<Dictionary<string, object>>(item.UpdateData) ?? new Dictionary<string, object>(),
                        OriginalData = JsonSerializer.Deserialize<Dictionary<string, object>>(item.OriginalData) ?? new Dictionary<string, object>(),
                        Status = item.Status,
                        SubmittedAt = item.SubmittedAt
                    };

                    if (item.Employee != null)
                    {
                        dto.Employee = new EmployeeBasicDTO
                        {
                            EmployeeID = item.Employee.EmployeeID ?? 0,
                            FirstName = item.Employee.FirstName,
                            LastName = item.Employee.LastName
                        };
                    }

                    response.Add(dto);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPending: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(int employeeId)
        {
            try
            {
                var data = await _pendingUpdateService.GetEmployeeHistoryAsync(employeeId);
                var response = new List<PendingUpdateResponseDTO>();

                foreach (var item in data)
                {
                    var dto = new PendingUpdateResponseDTO
                    {
                        PendingUpdateID = item.PendingUpdateID,
                        EmployeeID = item.EmployeeID,
                        UpdateData = JsonSerializer.Deserialize<Dictionary<string, object>>(item.UpdateData) ?? new Dictionary<string, object>(),
                        OriginalData = JsonSerializer.Deserialize<Dictionary<string, object>>(item.OriginalData) ?? new Dictionary<string, object>(),
                        Status = item.Status,
                        SubmittedAt = item.SubmittedAt,
                        ReviewedAt = item.ReviewedAt,
                        ReviewedBy = item.ReviewedBy,
                        ReviewerName = item.ReviewerName,
                        Comments = item.Comments
                    };

                    response.Add(dto);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByEmployeeId: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _pendingUpdateService.GetByIdAsync(id);
                if (item == null) return NotFound();

                var dto = new PendingUpdateResponseDTO
                {
                    PendingUpdateID = item.PendingUpdateID,
                    EmployeeID = item.EmployeeID,
                    UpdateData = JsonSerializer.Deserialize<Dictionary<string, object>>(item.UpdateData) ?? new Dictionary<string, object>(),
                    OriginalData = JsonSerializer.Deserialize<Dictionary<string, object>>(item.OriginalData) ?? new Dictionary<string, object>(),
                    Status = item.Status,
                    SubmittedAt = item.SubmittedAt,
                    ReviewedAt = item.ReviewedAt,
                    ReviewedBy = item.ReviewedBy,
                    ReviewerName = item.ReviewerName,
                    Comments = item.Comments
                };

                if (item.Employee != null)
                {
                    dto.Employee = new EmployeeBasicDTO
                    {
                        EmployeeID = item.Employee.EmployeeID ?? 0,
                        FirstName = item.Employee.FirstName,
                        LastName = item.Employee.LastName,
                        Email = item.Employee.Email,
                        PositionID = item.Employee.PositionID,
                        DepartmentID = item.Employee.DepartmentID
                    };
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitUpdate([FromBody] SubmitUpdateRequestDTO request)
        {
            try
            {
                Console.WriteLine($"=== SUBMIT UPDATE START ===");
                Console.WriteLine($"EmployeeID: {request.EmployeeId}");
                Console.WriteLine($"Raw UpdateData JSON: {JsonSerializer.Serialize(request.UpdateData)}");

                // Get original employee data
                var employee = await _employeeService.GetById(request.EmployeeId);
                if (employee == null)
                    return NotFound($"Employee with ID {request.EmployeeId} not found");

                // Create original data and update data dictionaries
                var originalData = new Dictionary<string, object>();
                var updateData = new Dictionary<string, object>();
                var changedFields = new List<string>();

                // Use reflection to get all properties
                var properties = typeof(tblEmployees).GetProperties();

                // Helper function to normalize date strings
                string NormalizeDateString(object dateObj)
                {
                    if (dateObj == null) return "null";

                    try
                    {
                        DateTime? date = null;

                        if (dateObj is DateTime dt)
                            date = dt;
                        else if (dateObj is string dateStr && !string.IsNullOrWhiteSpace(dateStr))
                        {
                            // Try multiple date formats
                            if (DateTime.TryParse(dateStr, out DateTime parsedDate))
                                date = parsedDate;
                        }

                        return date?.ToString("yyyy-MM-dd") ?? dateObj.ToString();
                    }
                    catch
                    {
                        return dateObj.ToString();
                    }
                }

                // Check each field in the update data
                foreach (var kvp in request.UpdateData)
                {
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                    if (prop != null)
                    {
                        var originalValue = prop.GetValue(employee);
                        var newValue = kvp.Value;

                        // Skip if both are null/empty
                        if ((originalValue == null || string.IsNullOrEmpty(originalValue.ToString())) &&
                            (newValue == null || string.IsNullOrEmpty(newValue.ToString())))
                        {
                            Console.WriteLine($"Field unchanged: {kvp.Key} - Both null/empty");
                            continue;
                        }

                        bool isChanged = false;
                        string normalizedOriginal = "";
                        string normalizedNew = "";

                        // Handle date comparisons specially
                        if (prop.PropertyType == typeof(DateTime?) || prop.PropertyType == typeof(DateTime))
                        {
                            // Normalize both dates to yyyy-MM-dd format
                            normalizedOriginal = NormalizeDateString(originalValue);
                            normalizedNew = NormalizeDateString(newValue);

                            isChanged = normalizedOriginal != normalizedNew;
                        }
                        // Handle nullable int comparisons
                        else if (prop.PropertyType == typeof(int?))
                        {
                            int? originalInt = originalValue as int?;
                            int? newInt = null;

                            if (newValue is int)
                                newInt = (int)newValue;
                            else if (newValue is int?)
                                newInt = (int?)newValue;
                            else if (newValue is string intString && !string.IsNullOrEmpty(intString))
                            {
                                if (int.TryParse(intString, out int parsedInt))
                                    newInt = parsedInt;
                            }

                            isChanged = originalInt != newInt;
                            normalizedOriginal = originalInt?.ToString() ?? "null";
                            normalizedNew = newInt?.ToString() ?? "null";
                        }
                        // Handle regular int comparisons
                        else if (prop.PropertyType == typeof(int))
                        {
                            int originalInt = originalValue is int ? (int)originalValue : 0;
                            int newInt = 0;

                            if (newValue is int)
                                newInt = (int)newValue;
                            else if (newValue is string intString && !string.IsNullOrEmpty(intString))
                            {
                                if (int.TryParse(intString, out int parsedInt))
                                    newInt = parsedInt;
                            }

                            isChanged = originalInt != newInt;
                            normalizedOriginal = originalInt.ToString();
                            normalizedNew = newInt.ToString();
                        }
                        // For string and other types
                        else
                        {
                            // Convert both to strings for comparison
                            normalizedOriginal = originalValue?.ToString()?.Trim() ?? "null";
                            normalizedNew = newValue?.ToString()?.Trim() ?? "null";

                            // Only include if there's an actual change
                            isChanged = normalizedOriginal != normalizedNew;
                        }

                        if (isChanged)
                        {
                            // Store the original value as-is from database
                            originalData[kvp.Key] = originalValue ?? "";
                            // Store the new value as-is from request
                            updateData[kvp.Key] = newValue ?? "";
                            changedFields.Add(kvp.Key);

                            Console.WriteLine($"✅ Field CHANGED: {kvp.Key} - Original: '{normalizedOriginal}', New: '{normalizedNew}'");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Field UNCHANGED: {kvp.Key} - Original: '{normalizedOriginal}', New: '{normalizedNew}'");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Property {kvp.Key} not found in tblEmployees");
                    }
                }

                Console.WriteLine($"Changed fields count: {changedFields.Count}");
                Console.WriteLine($"Changed fields: {string.Join(", ", changedFields)}");

                // If no fields actually changed, return an error
                if (changedFields.Count == 0)
                {
                    return BadRequest(new
                    {
                        message = "No changes detected",
                        details = "All submitted values are the same as current values"
                    });
                }

                // Submit the update
                var pendingUpdate = await _pendingUpdateService.SubmitUpdateAsync(
                    request.EmployeeId,
                    updateData,
                    originalData
                );

                // Create response DTO
                var response = new PendingUpdateResponseDTO
                {
                    PendingUpdateID = pendingUpdate.PendingUpdateID,
                    EmployeeID = pendingUpdate.EmployeeID,
                    UpdateData = updateData,
                    OriginalData = originalData,
                    Status = pendingUpdate.Status,
                    SubmittedAt = pendingUpdate.SubmittedAt
                };

                // Log transaction
                try
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();
                    await LogTransactionEvent("SUBMIT_UPDATE", user, request.EmployeeId,
                        $"Submitted update request for employee: {employee.FirstName} {employee.LastName}. Changed fields: {string.Join(", ", changedFields)}",
                        oldData: null, newData: null);
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Warning: Failed to log transaction: {logEx.Message}");
                }

                Console.WriteLine($"=== SUBMIT UPDATE END ===");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SubmitUpdate: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveUpdate(int id, [FromBody] ReviewUpdateRequestDTO request)
        {
            try
            {
                Console.WriteLine($"=== APPROVE UPDATE START ===");
                Console.WriteLine($"PendingUpdateID: {id}");
                Console.WriteLine($"Comments: {request.Comments}");

                var user = await _transactionEventService.GetCurrentUserAsync();

                // Ensure user.UserId is not null before passing it to ApproveUpdateAsync
                if (!user.UserId.HasValue)
                {
                    return BadRequest("User ID is required to approve the update.");
                }

                // Get the pending update first
                var pendingUpdate = await _pendingUpdateService.GetByIdAsync(id);
                if (pendingUpdate == null)
                    return NotFound($"Pending update with ID {id} not found");

                Console.WriteLine($"Found pending update for EmployeeID: {pendingUpdate.EmployeeID}");

                // Get the employee for updating
                var employee = await _employeeService.GetById(pendingUpdate.EmployeeID);
                if (employee == null)
                    return NotFound($"Employee with ID {pendingUpdate.EmployeeID} not found");

                Console.WriteLine($"Found employee: {employee.FirstName} {employee.LastName}");

                // Get the update data
                var updateData = JsonSerializer.Deserialize<Dictionary<string, object>>(pendingUpdate.UpdateData);
                if (updateData == null || updateData.Count == 0)
                {
                    return BadRequest("No update data found in the pending update.");
                }

                Console.WriteLine($"Will apply {updateData.Count} updates to employee {employee.EmployeeID}");

                // Store old values for logging
                var oldValues = new Dictionary<string, object>();
                var properties = typeof(tblEmployees).GetProperties();

                // First pass: collect old values
                foreach (var kvp in updateData)
                {
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                    if (prop != null)
                    {
                        var oldValue = prop.GetValue(employee);
                        oldValues[kvp.Key] = oldValue ?? "";
                        Console.WriteLine($"Old value for {kvp.Key}: {oldValue}");
                    }
                }

                // Second pass: apply updates
                foreach (var kvp in updateData)
                {
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                    if (prop != null && kvp.Value != null)
                    {
                        try
                        {
                            var value = kvp.Value;
                            Console.WriteLine($"Applying update: {kvp.Key} = {value} (Type: {value?.GetType().Name})");

                            // Convert value to the correct type
                            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                            if (targetType == typeof(string))
                            {
                                prop.SetValue(employee, value.ToString());
                                Console.WriteLine($"✅ Set string property {kvp.Key} to: {value}");
                            }
                            else if (targetType == typeof(int))
                            {
                                int intValue = 0;
                                bool success = false;

                                if (value is JsonElement jsonElement)
                                {
                                    if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out int parsedInt))
                                    {
                                        intValue = parsedInt;
                                        success = true;
                                    }
                                    else if (jsonElement.ValueKind == JsonValueKind.String && int.TryParse(jsonElement.GetString(), out parsedInt))
                                    {
                                        intValue = parsedInt;
                                        success = true;
                                    }
                                }
                                else if (value is int)
                                {
                                    intValue = (int)value;
                                    success = true;
                                }
                                else if (int.TryParse(value.ToString(), out int parsedInt))
                                {
                                    intValue = parsedInt;
                                    success = true;
                                }

                                if (success)
                                {
                                    prop.SetValue(employee, intValue);
                                    Console.WriteLine($"✅ Set int property {kvp.Key} to: {intValue}");
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ Could not parse int for property {kvp.Key}: {value}");
                                }
                            }
                            else if (targetType == typeof(DateTime))
                            {
                                DateTime? dateValue = null;

                                if (value is JsonElement jsonElement)
                                {
                                    if (jsonElement.ValueKind == JsonValueKind.String)
                                    {
                                        if (DateTime.TryParse(jsonElement.GetString(), out DateTime parsedDate))
                                        {
                                            dateValue = parsedDate;
                                        }
                                    }
                                }
                                else if (value is DateTime dt)
                                {
                                    dateValue = dt;
                                }
                                else if (value is string dateStr && !string.IsNullOrEmpty(dateStr))
                                {
                                    if (DateTime.TryParse(dateStr, out DateTime parsedDate))
                                    {
                                        dateValue = parsedDate;
                                    }
                                }

                                if (dateValue.HasValue)
                                {
                                    prop.SetValue(employee, dateValue.Value);
                                    Console.WriteLine($"✅ Set DateTime property {kvp.Key} to: {dateValue.Value:yyyy-MM-dd}");
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ Could not parse DateTime for property {kvp.Key}: {value}");
                                }
                            }
                            else if (targetType == typeof(bool))
                            {
                                if (bool.TryParse(value.ToString(), out bool boolValue))
                                {
                                    prop.SetValue(employee, boolValue);
                                    Console.WriteLine($"✅ Set bool property {kvp.Key} to: {boolValue}");
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ Could not parse bool for property {kvp.Key}: {value}");
                                }
                            }
                            else if (targetType.IsEnum)
                            {
                                try
                                {
                                    var enumValue = Enum.Parse(targetType, value.ToString());
                                    prop.SetValue(employee, enumValue);
                                    Console.WriteLine($"✅ Set enum property {kvp.Key} to: {enumValue}");
                                }
                                catch
                                {
                                    Console.WriteLine($"⚠️ Could not parse enum for property {kvp.Key}: {value}");
                                }
                            }
                            else
                            {
                                // Try generic conversion
                                try
                                {
                                    var convertedValue = Convert.ChangeType(value, targetType);
                                    prop.SetValue(employee, convertedValue);
                                    Console.WriteLine($"✅ Set property {kvp.Key} to (converted): {convertedValue}");
                                }
                                catch (Exception convEx)
                                {
                                    Console.WriteLine($"⚠️ Failed to convert property {kvp.Key}: {convEx.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Error applying property {kvp.Key}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Property {kvp.Key} not found in tblEmployees or value is null");
                    }
                }

                // Update the employee in database
                Console.WriteLine($"Updating employee {employee.EmployeeID} in database...");
                var updatedEmployee = await _employeeService.Update(employee);

                if (updatedEmployee == null)
                {
                    Console.WriteLine($"❌ Failed to update employee in database");
                    return StatusCode(500, "Failed to update employee record in database.");
                }

                Console.WriteLine($"✅ Employee updated successfully in database");

                // Now approve the update in the pending updates table
                Console.WriteLine($"Approving pending update {id}...");
                var approvedUpdate = await _pendingUpdateService.ApproveUpdateAsync(
                    id,
                    user.UserId.Value,
                    user.Username ?? "Unknown",
                    request.Comments
                );

                Console.WriteLine($"✅ Pending update approved");

                // Create response DTO
                var response = new PendingUpdateResponseDTO
                {
                    PendingUpdateID = approvedUpdate.PendingUpdateID,
                    EmployeeID = approvedUpdate.EmployeeID,
                    UpdateData = updateData,
                    OriginalData = JsonSerializer.Deserialize<Dictionary<string, object>>(approvedUpdate.OriginalData) ?? new Dictionary<string, object>(),
                    Status = approvedUpdate.Status,
                    SubmittedAt = approvedUpdate.SubmittedAt,
                    ReviewedAt = approvedUpdate.ReviewedAt,
                    ReviewedBy = approvedUpdate.ReviewedBy,
                    ReviewerName = approvedUpdate.ReviewerName,
                    Comments = approvedUpdate.Comments
                };

                // Log transaction
                try
                {
                    await LogTransactionEvent("APPROVE_UPDATE", user, approvedUpdate.EmployeeID,
                        $"Approved update request for employee: {employee.FirstName} {employee.LastName}. Applied changes: {string.Join(", ", updateData.Keys)}",
                        oldData: null, newData: null);
                    Console.WriteLine($"✅ Transaction logged");
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"⚠️ Failed to log transaction: {logEx.Message}");
                }

                Console.WriteLine($"=== APPROVE UPDATE END ===");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ApproveUpdate: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectUpdate(int id, [FromBody] ReviewUpdateRequestDTO request)
        {
            try
            {
                Console.WriteLine($"RejectUpdate called for ID: {id}");

                var user = await _transactionEventService.GetCurrentUserAsync();

                // Ensure user.UserId is not null before passing it to RejectUpdateAsync
                if (!user.UserId.HasValue)
                {
                    return BadRequest("User ID is required to reject the update.");
                }

                // Get the pending update first
                var pendingUpdate = await _pendingUpdateService.GetByIdAsync(id);
                if (pendingUpdate == null)
                    return NotFound($"Pending update with ID {id} not found");

                // Get the employee for logging
                var employee = await _employeeService.GetById(pendingUpdate.EmployeeID);

                var rejectedUpdate = await _pendingUpdateService.RejectUpdateAsync(
                    id,
                    user.UserId.Value, // Use Value to convert nullable int to int
                    user.Username ?? "Unknown",
                    request.Comments
                );

                // Create response DTO
                var response = new PendingUpdateResponseDTO
                {
                    PendingUpdateID = rejectedUpdate.PendingUpdateID,
                    EmployeeID = rejectedUpdate.EmployeeID,
                    UpdateData = JsonSerializer.Deserialize<Dictionary<string, object>>(rejectedUpdate.UpdateData) ?? new Dictionary<string, object>(),
                    OriginalData = JsonSerializer.Deserialize<Dictionary<string, object>>(rejectedUpdate.OriginalData) ?? new Dictionary<string, object>(),
                    Status = rejectedUpdate.Status,
                    SubmittedAt = rejectedUpdate.SubmittedAt,
                    ReviewedAt = rejectedUpdate.ReviewedAt,
                    ReviewedBy = rejectedUpdate.ReviewedBy,
                    ReviewerName = rejectedUpdate.ReviewerName,
                    Comments = rejectedUpdate.Comments
                };

                // Log transaction
                await LogTransactionEvent("REJECT_UPDATE", user, rejectedUpdate.EmployeeID,
                    $"Rejected update request for employee: {employee?.FirstName} {employee?.LastName}",
                    oldData: null, newData: null);

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RejectUpdate: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("cleanup")]
        public async Task<IActionResult> CleanupOldUpdates()
        {
            try
            {
                await _pendingUpdateService.CleanupOldUpdatesAsync();
                return Ok(new { message = "Old updates cleaned up successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CleanupOldUpdates: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var allUpdates = await _pendingUpdateService.GetAllAsync();

                var stats = new
                {
                    Total = allUpdates.Count(),
                    Pending = allUpdates.Count(u => u.Status == "pending"),
                    Approved = allUpdates.Count(u => u.Status == "approved"),
                    Rejected = allUpdates.Count(u => u.Status == "rejected"),
                    Last30Days = allUpdates.Count(u => u.SubmittedAt >= DateTime.Now.AddDays(-30))
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStatistics: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("debug/employee/{id}")]
        public async Task<IActionResult> DebugEmployee(int id)
        {
            try
            {
                var employee = await _employeeService.GetById(id);
                if (employee == null) return NotFound();

                return Ok(new
                {
                    employeeId = employee.EmployeeID,
                    firstName = employee.FirstName,
                    lastName = employee.LastName,
                    dateOfBirth = employee.DateOfBirth,
                    hireDate = employee.HireDate,
                    yearGraduated = employee.YearGraduated,
                    departmentID = employee.DepartmentID,
                    positionID = employee.PositionID,
                    // Add other fields you're trying to update
                    rawData = employee
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task LogTransactionEvent(string action, UserRolesDTOV2 user, int employeeId,
            string description, tblEmployees oldData, tblEmployees newData)
        {
            try
            {
                await _transactionEventService.InsertAsync(new TransactionEvent
                {
                    Action = action,
                    Description = $"{user.Username} {action}: {description}",
                    UserID = user.UserId,
                    UserName = user.Username ?? "Unknown",
                    Fullname = user.Username ?? "Unknown",
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to log transaction: {ex.Message}");
            }
        }
    }
}