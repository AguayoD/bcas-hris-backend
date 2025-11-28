using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Model.Models;
using Repositories.Service;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Repositories.Services;
using Models.DTOs.UsersDTO;
using Models.Models;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsController : BaseController
    {
        private readonly tblContractsService _tblContractsService;
        private readonly TransactionEventService _transactionEventService;
        private readonly tblEmployeeService _employeesService;

        public ContractsController(
            IHttpContextAccessor httpContextAccessor,
            TransactionEventService transactionEventService,
            tblEmployeeService employeesService
        ) : base(httpContextAccessor)
        {
            _transactionEventService = transactionEventService;
            _employeesService = employeesService;
            _tblContractsService = new tblContractsService();
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetContractsByEmployeeId(int employeeId)
        {
            try
            {
                var allContracts = await _tblContractsService.GetAll();
                var employeeContracts = allContracts.Where(c => c.EmployeeID == employeeId).ToList();

                return Ok(employeeContracts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAlltblContracts()
        {
            try
            {
                var data = await _tblContractsService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdtblContracts(int id)
        {
            try
            {
                var data = await _tblContractsService.GetById(id);
                if (data == null) return NoContent();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InserttblContracts(
            [FromForm] int employeeID,
            [FromForm] string contractType,
            [FromForm] string contractStartDate,
            [FromForm] string? contractEndDate,
            [FromForm] int lastUpdatedBy,
            [FromForm] string? contractCategory,
            IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                // Use consistent relative path
                var uploadsFolder = Path.Combine("Uploads", "Contracts");
                var fullUploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsFolder);

                if (!Directory.Exists(fullUploadsPath))
                    Directory.CreateDirectory(fullUploadsPath);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(fullUploadsPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Store ONLY the filename in database - not the full path
                var contract = new tblContracts
                {
                    EmployeeID = employeeID,
                    ContractType = contractType,
                    ContractStartDate = DateTime.Parse(contractStartDate),
                    ContractEndDate = string.IsNullOrEmpty(contractEndDate) ? null : DateTime.Parse(contractEndDate),
                    LastUpdatedBy = lastUpdatedBy,
                    LastUpdatedAt = DateTime.Now,
                    FileName = file.FileName,
                    FilePath = uniqueFileName,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    ContractCategory = contractCategory
                };

                var data = await _tblContractsService.Insert(contract);

                // Log transaction event with detailed debugging
                Console.WriteLine($"=== STARTING TRANSACTION LOG FOR CREATE ===");
                try
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();
                    Console.WriteLine($"User retrieved: {user?.Username} (ID: {user?.UserId})");

                    if (user == null)
                    {
                        Console.WriteLine("ERROR: User is null - cannot log transaction");
                    }
                    else
                    {
                        await LogTransactionEvent("CREATE", user, employeeID,
                            $"uploaded {contractType} contract",
                            oldData: null, newData: data);
                        Console.WriteLine($"SUCCESS: CREATE transaction logged for contract {data.ContractID}");
                    }
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"ERROR: Failed to log CREATE transaction: {logEx.Message}");
                    Console.WriteLine($"Stack trace: {logEx.StackTrace}");
                }
                Console.WriteLine($"=== END TRANSACTION LOG FOR CREATE ===");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContractFormData(
            int id,
            [FromForm] string? contractType,
            [FromForm] string? contractStartDate,
            [FromForm] string? contractEndDate,
            [FromForm] int lastUpdatedBy,
            [FromForm] string? contractCategory,
            IFormFile? file = null)
        {
            try
            {
                Console.WriteLine($"Update contract {id} received:");
                Console.WriteLine($"ContractType: {contractType}");
                Console.WriteLine($"ContractStartDate: {contractStartDate}");
                Console.WriteLine($"ContractEndDate: {contractEndDate}");
                Console.WriteLine($"LastUpdatedBy: {lastUpdatedBy}");
                Console.WriteLine($"ContractCategory: {contractCategory}");
                Console.WriteLine($"File: {(file != null ? file.FileName : "null")}");

                var oldData = await _tblContractsService.GetById(id);
                if (oldData == null)
                {
                    Console.WriteLine($"Contract with ID {id} not found");
                    return NotFound($"Contract with ID {id} not found");
                }

                // Handle file upload if provided
                if (file != null && file.Length > 0)
                {
                    // Delete old file if it exists
                    if (!string.IsNullOrEmpty(oldData.FilePath))
                    {
                        var oldPhysicalPath = GetPhysicalPath(oldData.FilePath);
                        if (System.IO.File.Exists(oldPhysicalPath))
                        {
                            System.IO.File.Delete(oldPhysicalPath);
                        }
                    }

                    var uploadsFolder = Path.Combine("Uploads", "Contracts");
                    var fullUploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsFolder);

                    if (!Directory.Exists(fullUploadsPath))
                        Directory.CreateDirectory(fullUploadsPath);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var filePath = Path.Combine(fullUploadsPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Store only filename
                    oldData.FileName = file.FileName;
                    oldData.FilePath = uniqueFileName;
                    oldData.FileType = file.ContentType;
                    oldData.FileSize = file.Length;
                }

                // Update other fields if provided
                if (!string.IsNullOrEmpty(contractType))
                    oldData.ContractType = contractType;

                if (!string.IsNullOrEmpty(contractStartDate))
                    oldData.ContractStartDate = DateTime.Parse(contractStartDate);

                // Handle contract category - can be set to null if empty string is provided
                if (contractCategory != null)
                {
                    oldData.ContractCategory = string.IsNullOrEmpty(contractCategory) ?
                        null : contractCategory;
                }

                // Handle nullable end date - can be set to null if empty string is provided
                if (contractEndDate != null)
                {
                    oldData.ContractEndDate = string.IsNullOrEmpty(contractEndDate) ?
                        null : DateTime.Parse(contractEndDate);
                }

                oldData.LastUpdatedBy = lastUpdatedBy;
                oldData.LastUpdatedAt = DateTime.Now;

                var updatedData = await _tblContractsService.Update(oldData);

                // Log transaction event with detailed debugging
                Console.WriteLine($"=== STARTING TRANSACTION LOG FOR UPDATE ===");
                try
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();
                    Console.WriteLine($"User retrieved: {user?.Username} (ID: {user?.UserId})");

                    if (user == null)
                    {
                        Console.WriteLine("ERROR: User is null - cannot log transaction");
                    }
                    else
                    {
                        await LogTransactionEvent("UPDATE", user, oldData.EmployeeID,
                            $"updated {oldData.ContractType} contract",
                            oldData, updatedData);
                        Console.WriteLine($"SUCCESS: UPDATE transaction logged for contract {id}");
                    }
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"ERROR: Failed to log UPDATE transaction: {logEx.Message}");
                    Console.WriteLine($"Stack trace: {logEx.StackTrace}");
                }
                Console.WriteLine($"=== END TRANSACTION LOG FOR UPDATE ===");

                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating contract {id}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Error updating contract: {ex.Message}");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatetblContracts(int id, [FromBody] tblContracts tblContracts)
        {
            try
            {
                if (id != tblContracts.ContractID) return BadRequest("Id mismatched.");

                var oldData = await _tblContractsService.GetById(id);
                if (oldData == null) return NotFound();

                var updatedData = await _tblContractsService.Update(tblContracts);

                // Log transaction event with detailed debugging
                Console.WriteLine($"=== STARTING TRANSACTION LOG FOR PATCH UPDATE ===");
                try
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();
                    Console.WriteLine($"User retrieved: {user?.Username} (ID: {user?.UserId})");

                    if (user == null)
                    {
                        Console.WriteLine("ERROR: User is null - cannot log transaction");
                    }
                    else
                    {
                        await LogTransactionEvent("UPDATE", user, oldData.EmployeeID,
                            $"updated {oldData.ContractType} contract via PATCH",
                            oldData, updatedData);
                        Console.WriteLine($"SUCCESS: PATCH UPDATE transaction logged for contract {id}");
                    }
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"ERROR: Failed to log PATCH UPDATE transaction: {logEx.Message}");
                    Console.WriteLine($"Stack trace: {logEx.StackTrace}");
                }
                Console.WriteLine($"=== END TRANSACTION LOG FOR PATCH UPDATE ===");

                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task LogTransactionEvent(string v1, UserRolesDTOV2 user, int? employeeID, string v2, tblContracts oldData, tblContracts updatedData)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdtblContracts(int id)
        {
            try
            {
                Console.WriteLine($"Delete request received for contract ID: {id}");

                var data = await _tblContractsService.GetById(id);
                if (data == null)
                {
                    Console.WriteLine($"Contract with ID {id} not found");
                    return NotFound($"Contract with ID {id} not found");
                }

                // Delete the associated file
                if (!string.IsNullOrEmpty(data.FilePath))
                {
                    try
                    {
                        var physicalPath = GetPhysicalPath(data.FilePath);
                        if (System.IO.File.Exists(physicalPath))
                        {
                            System.IO.File.Delete(physicalPath);
                            Console.WriteLine($"File deleted: {physicalPath}");
                        }
                    }
                    catch (Exception fileEx)
                    {
                        Console.WriteLine($"Warning: Failed to delete file {data.FilePath}: {fileEx.Message}");
                        // Continue with database deletion even if file deletion fails
                    }
                }

                var deletedData = await _tblContractsService.DeleteById(id);
                Console.WriteLine($"Contract deleted from database: {id}");

                // Log transaction event with detailed debugging
                Console.WriteLine($"=== STARTING TRANSACTION LOG FOR DELETE ===");
                try
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();
                    Console.WriteLine($"User retrieved: {user?.Username} (ID: {user?.UserId})");

                    if (user == null)
                    {
                        Console.WriteLine("ERROR: User is null - cannot log transaction");
                    }
                    else
                    {
                        await LogTransactionEvent("DELETE", user, data.EmployeeID,
                            $"deleted {data.ContractType} contract",
                            oldData: data, newData: null);
                        Console.WriteLine($"SUCCESS: DELETE transaction logged for contract {id}");
                    }
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"ERROR: Failed to log DELETE transaction: {logEx.Message}");
                    Console.WriteLine($"Stack trace: {logEx.StackTrace}");
                }
                Console.WriteLine($"=== END TRANSACTION LOG FOR DELETE ===");

                return Ok(new { message = "Contract deleted successfully", deletedData });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting contract {id}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Error deleting contract: {ex.Message}");
            }
        }

        private async Task LogTransactionEvent(string v1, UserRolesDTOV2 user, int? employeeID, string v2, tblContracts oldData, object newData)
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadContract(int id)
        {
            try
            {
                var contract = await _tblContractsService.GetById(id);
                if (contract == null || string.IsNullOrEmpty(contract.FilePath))
                    return NotFound();

                var physicalPath = GetPhysicalPath(contract.FilePath);

                if (!System.IO.File.Exists(physicalPath))
                {
                    Console.WriteLine($"File not found: {physicalPath}");
                    return NotFound("File not found on server");
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var contentType = GetContentType(contract.FileName ?? contract.FilePath);
                var fileName = contract.FileName ?? "contract";

                return File(memory, contentType, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading contract {id}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/fileurl")]
        public async Task<IActionResult> GetFileUrl(int id)
        {
            try
            {
                var contract = await _tblContractsService.GetById(id);
                if (contract == null || string.IsNullOrEmpty(contract.FilePath))
                    return NotFound();

                // Return the direct URL to the file through our API
                var fileUrl = Url.Action("DownloadContract", "Contracts", new { id = id }, Request.Scheme);
                return Ok(new
                {
                    fileUrl,
                    fileName = contract.FileName,
                    fileType = contract.FileType
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("check-expirations")]
        public async Task<IActionResult> CheckContractExpirations()
        {
            try
            {
                var notificationService = new ContractNotificationService();
                await notificationService.CheckAndSendContractNotifications();
                return Ok(new { message = "Contract expiration check completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error checking contract expirations: {ex.Message}" });
            }
        }

        // Helper method to convert filename to physical path
        private string GetPhysicalPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            var uploadsFolder = Path.Combine("Uploads", "Contracts");
            var fullUploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsFolder);

            return Path.Combine(fullUploadsPath, fileName);
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".webp", "image/webp"},
                {".bmp", "image/bmp"},
                {".zip", "application/zip"},
                {".rar", "application/x-rar-compressed"},
            };
        }

        // --- Transaction Event Helper Methods ---
        private async Task LogTransactionEvent(string action, UserRolesDTOV2 user, int employeeId,
            string description, tblContracts oldData, tblContracts newData)
        {
            try
            {
                Console.WriteLine($"LogTransactionEvent started for {action}");

                string changes = oldData != null && newData != null ? GetChanges(oldData, newData) : "";
                Console.WriteLine($"Changes detected: {changes}");

                // Get employee name with error handling
                string employeeName = $"Employee {employeeId}"; // Default fallback
                try
                {
                    Console.WriteLine($"Getting employee name for ID: {employeeId}");
                    employeeName = await GetEmployeeName(employeeId);
                    Console.WriteLine($"Employee name retrieved: {employeeName}");
                }
                catch (Exception nameEx)
                {
                    Console.WriteLine($"Warning: Failed to get employee name for ID {employeeId}: {nameEx.Message}");
                    // Use default fallback
                }

                // Create proper description based on action type
                string fullDescription = action switch
                {
                    "CREATE" => $"uploaded {newData?.ContractType} contract for {employeeName}",
                    "UPDATE" => !string.IsNullOrEmpty(changes)
                        ? $"updated {oldData?.ContractType} contract for {employeeName} - {changes}"
                        : $"updated {oldData?.ContractType} contract for {employeeName}",
                    "DELETE" => $"deleted {oldData?.ContractType} contract for {employeeName}",
                    _ => description.ToLower()
                };

                Console.WriteLine($"Creating TransactionEvent with description: {fullDescription}");

                var transactionEvent = new TransactionEvent
                {
                    Action = action,
                    Description = $"{user.Username} {fullDescription}",
                    UserID = user.UserId,
                    UserName = user.Username ?? "Unknown",
                    Fullname = employeeName,
                    Timestamp = DateTime.Now
                };

                Console.WriteLine($"Calling TransactionEventService.InsertAsync...");
                await _transactionEventService.InsertAsync(transactionEvent);
                Console.WriteLine($"TransactionEventService.InsertAsync completed successfully");

                Console.WriteLine($"SUCCESS: {action} transaction logged for employee {employeeName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRITICAL ERROR in LogTransactionEvent: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                // Don't re-throw - let the main operation continue even if logging fails
            }
        }

        private async Task<string> GetEmployeeName(int employeeId)
        {
            try
            {
                Console.WriteLine($"GetEmployeeName called for ID: {employeeId}");
                var employee = await _employeesService.GetById(employeeId);

                if (employee != null)
                {
                    string name = $"{employee.FirstName} {employee.LastName}";
                    Console.WriteLine($"Employee name found: {name}");
                    return name;
                }

                Console.WriteLine($"Employee with ID {employeeId} not found");
                return $"Employee {employeeId}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee name for ID {employeeId}: {ex.Message}");
                return $"Employee {employeeId}";
            }
        }

        private string GetChanges(tblContracts oldData, tblContracts newData)
        {
            try
            {
                var changes = new List<string>();
                var properties = typeof(tblContracts).GetProperties();

                foreach (var prop in properties)
                {
                    // Skip properties that shouldn't be compared or are complex objects
                    if (prop.Name == "EmployeeID" || prop.Name == "Employee" ||
                        prop.Name == "LastUpdatedBy" || prop.Name == "LastUpdatedAt" ||
                        prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                        continue;

                    var oldValue = prop.GetValue(oldData)?.ToString() ?? "";
                    var newValue = prop.GetValue(newData)?.ToString() ?? "";

                    if (oldValue != newValue)
                    {
                        changes.Add($"{prop.Name}: {oldValue} → {newValue}");
                    }
                }

                string result = changes.Count > 0 ? string.Join(" | ", changes) : "No changes detected";
                Console.WriteLine($"GetChanges result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetChanges: {ex.Message}");
                return "Error detecting changes";
            }
        }
    }
}