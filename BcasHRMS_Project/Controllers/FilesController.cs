// Controllers/FilesController.cs
using BCAS_HRMSbackend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Repositories;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : BaseController
    {
        private readonly tblGenericRepository<FileModel> _repository;

        public FilesController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = new tblGenericRepository<FileModel>();
            _repository.tableName = "Files";
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            [FromForm] IFormFile file,
            [FromForm] int employeeId,
            [FromForm] string documentType)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (string.IsNullOrEmpty(documentType))
                return BadRequest("Document type is required.");

            byte[] data;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                data = ms.ToArray();
            }

            var existingFile = await _repository.GetFileByEmployeeAndDocumentTypeAsync(employeeId, documentType);

            if (existingFile != null)
            {
                // Store old file info for audit
                var oldFileInfo = new
                {
                    existingFile.FileName,
                    existingFile.ContentType,
                    FileSize = existingFile.Data?.Length
                };

                // Overwrite existing file
                existingFile.FileName = file.FileName;
                existingFile.ContentType = file.ContentType;
                existingFile.Data = data;

                await _repository.UpdateFileAsync(existingFile);

                // Log the UPDATE action
                await LogActionAsync("Files", "UPDATE", existingFile.Id.ToString(),
                    oldFileInfo,
                    new
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileSize = data.Length,
                        EmployeeId = employeeId,
                        DocumentType = documentType
                    });

                return Ok(new { FileId = existingFile.Id, Message = "File updated successfully." });
            }
            else
            {
                // Insert new
                var fileModel = new FileModel
                {
                    EmployeeId = employeeId,
                    DocumentType = documentType,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Data = data
                };

                int newId = await _repository.InsertFileAsync(fileModel);

                // Log the INSERT action
                await LogActionAsync("Files", "INSERT", newId.ToString(), null, new
                {
                    FileId = newId,
                    EmployeeId = employeeId,
                    DocumentType = documentType,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = data.Length
                });

                return Ok(new { FileId = newId, Message = "File uploaded successfully." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            try
            {
                var file = await _repository.GetFileByIdAsync(id);
                if (file == null)
                    return NotFound();

                await _repository.DeleteById(id);

                // Log the DELETE action
                await LogActionAsync("Files", "DELETE", id.ToString(), new
                {
                    file.FileName,
                    file.ContentType,
                    file.EmployeeId,
                    file.DocumentType,
                    FileSize = file.Data?.Length
                }, null);

                return Ok(new { Message = "File deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }

        // ... existing GET methods remain the same
        [HttpGet("list/{employeeId}")]
        public async Task<IActionResult> GetFilesByEmployee(int employeeId)
        {
            var files = await _repository.GetFilesByEmployeeAsync(employeeId);
            return Ok(files);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _repository.GetFileByIdAsync(id);
            if (file == null)
                return NotFound();

            return File(file.Data, file.ContentType, file.FileName);
        }
    }
}