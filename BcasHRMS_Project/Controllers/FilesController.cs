using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Repositories;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly tblGenericRepository<FileModel> _repository;

        public FilesController()
        {
            _repository = new tblGenericRepository<FileModel>();
            _repository.tableName = "Files"; // override if class name ≠ table name
        }

        // Upload or update file per employee and document type
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
                // Overwrite existing file
                existingFile.FileName = file.FileName;
                existingFile.ContentType = file.ContentType;
                existingFile.Data = data;

                await _repository.UpdateFileAsync(existingFile);
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
                return Ok(new { FileId = newId, Message = "File uploaded successfully." });
            }
        }

        // Get file list per employee
        [HttpGet("list/{employeeId}")]
        public async Task<IActionResult> GetFilesByEmployee(int employeeId)
        {
            var files = await _repository.GetFilesByEmployeeAsync(employeeId);
            return Ok(files);
        }

        // Download file by id
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