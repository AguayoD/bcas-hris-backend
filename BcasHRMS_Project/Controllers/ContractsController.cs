using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Model.Models;
using Repositories.Service;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly tblContractsService _tblContractsService = new tblContractsService();

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
            [FromForm] string? contractCategory, // ADDED: contractCategory parameter
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
                    FilePath = uniqueFileName, // Store only filename
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    ContractCategory = contractCategory // ADDED: Set contract category
                };

                var data = await _tblContractsService.Insert(contract);
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
            [FromForm] string? contractCategory, // ADDED: contractCategory parameter
            IFormFile? file)
        {
            try
            {
                var existingContract = await _tblContractsService.GetById(id);
                if (existingContract == null) return NotFound();

                // Handle file upload if provided
                if (file != null && file.Length > 0)
                {
                    // Delete old file if it exists
                    if (!string.IsNullOrEmpty(existingContract.FilePath))
                    {
                        var oldPhysicalPath = GetPhysicalPath(existingContract.FilePath);
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
                    existingContract.FileName = file.FileName;
                    existingContract.FilePath = uniqueFileName;
                    existingContract.FileType = file.ContentType;
                    existingContract.FileSize = file.Length;
                }

                // Update other fields if provided
                if (!string.IsNullOrEmpty(contractType))
                    existingContract.ContractType = contractType;

                if (!string.IsNullOrEmpty(contractStartDate))
                    existingContract.ContractStartDate = DateTime.Parse(contractStartDate);

                // Handle contract category - can be set to null if empty string is provided
                if (contractCategory != null)
                {
                    existingContract.ContractCategory = string.IsNullOrEmpty(contractCategory) ?
                        null : contractCategory;
                }

                // Handle nullable end date - can be set to null if empty string is provided
                if (contractEndDate != null)
                {
                    existingContract.ContractEndDate = string.IsNullOrEmpty(contractEndDate) ?
                        null : DateTime.Parse(contractEndDate);
                }

                existingContract.LastUpdatedBy = lastUpdatedBy;
                existingContract.LastUpdatedAt = DateTime.Now;

                var updatedData = await _tblContractsService.Update(existingContract);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatetblContracts(int id, [FromBody] tblContracts tblContracts)
        {
            try
            {
                if (id != tblContracts.ContractID) return BadRequest("Id mismatched.");

                var data = await _tblContractsService.GetById(id);
                if (data == null) return NotFound();

                var updatedData = await _tblContractsService.Update(tblContracts);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
                    return NotFound();
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
                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting contract {id}: {ex.Message}");
                return BadRequest(ex.Message);
            }
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

        // Add this method to get direct file URL
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
    }
}