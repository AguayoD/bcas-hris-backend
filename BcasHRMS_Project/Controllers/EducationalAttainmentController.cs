using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Service;
using Models.DTOs.UsersDTO;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EducationalAttainmentController : BaseController
    {
        private readonly tblEducationalAttainmentService _educationalAttainmentService;
        private readonly TransactionEventService _transactionEventService;

        public EducationalAttainmentController(
            IHttpContextAccessor httpContextAccessor,
            tblEducationalAttainmentService educationalAttainmentService,
            TransactionEventService transactionEventService
        ) : base(httpContextAccessor)
        {
            _educationalAttainmentService = educationalAttainmentService;
            _transactionEventService = transactionEventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEducationalAttainment()
        {
            try
            {
                var data = await _educationalAttainmentService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdEducationalAttainment(int id)
        {
            try
            {
                var data = await _educationalAttainmentService.GetById(id);
                if (data == null) return NoContent();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertEducationalAttainment([FromBody] EducationalAttainmentCreateRequest request)
        {
            try
            {
                var educationalAttainment = new tblEducationalAttainment
                {
                    AttainmentName = request.AttainmentName,
                    Description = request.Description,
                    IsActive = request.IsActive ?? true
                };

                var data = await _educationalAttainmentService.Insert(educationalAttainment);

                if (data?.EducationalAttainmentID != null)
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();
                    await LogTransactionEvent("CREATE", user, 0,
                        $"Added educational attainment: {data.AttainmentName}",
                        oldData: null, newData: data);
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateEducationalAttainment(int id, [FromBody] EducationalAttainmentUpdateRequest request)
        {
            try
            {
                var oldData = await _educationalAttainmentService.GetById(id);
                if (oldData == null) return NotFound();

                var educationalAttainment = new tblEducationalAttainment
                {
                    EducationalAttainmentID = id,
                    AttainmentName = request.AttainmentName,
                    Description = request.Description,
                    IsActive = request.IsActive
                };

                var updatedData = await _educationalAttainmentService.Update(educationalAttainment);

                var user = await _transactionEventService.GetCurrentUserAsync();
                await LogTransactionEvent("UPDATE", user, 0,
                    $"Updated educational attainment: {updatedData.AttainmentName}",
                    oldData, updatedData);

                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdEducationalAttainment(int id)
        {
            try
            {
                var data = await _educationalAttainmentService.GetById(id);
                if (data == null) return NotFound();

                await _educationalAttainmentService.DeleteById(id);

                var user = await _transactionEventService.GetCurrentUserAsync();
                await LogTransactionEvent("DELETE", user, 0,
                    $"Deleted educational attainment: {data.AttainmentName}",
                    oldData: data, newData: null);

                return Ok(new { Message = $"Educational attainment {data.AttainmentName} deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- DTO Classes ---
        public class EducationalAttainmentCreateRequest
        {
            public string AttainmentName { get; set; }
            public string Description { get; set; }
            public bool? IsActive { get; set; }
        }

        public class EducationalAttainmentUpdateRequest
        {
            public string AttainmentName { get; set; }
            public string Description { get; set; }
            public bool? IsActive { get; set; }
        }

        // --- Transaction Event Helper Methods ---
        private async Task LogTransactionEvent(string action, UserRolesDTOV2 user, int employeeId,
            string description, tblEducationalAttainment oldData, tblEducationalAttainment newData)
        {
            string changes = oldData != null && newData != null ? GetChanges(oldData, newData) : "";

            await _transactionEventService.InsertAsync(new TransactionEvent
            {
                Action = action,
                Description = !string.IsNullOrEmpty(changes)
                    ? $"{user.Username} {action}: {changes}"
                    : $"{user.Username} {action}: {description}",
                UserID = user.UserId,
                UserName = user.Username ?? "Unknown",
                Fullname = newData != null
                    ? $"{newData.AttainmentName}"
                    : oldData != null
                        ? $"{oldData.AttainmentName}"
                        : "Unknown",
                Timestamp = DateTime.Now
            });
        }

        private string GetChanges(tblEducationalAttainment oldData, tblEducationalAttainment newData)
        {
            var changes = new List<string>();
            var properties = typeof(tblEducationalAttainment).GetProperties();

            foreach (var prop in properties)
            {
                // Skip properties that shouldn't be compared
                if (prop.Name == "EmployeeID" || prop.Name == "Employee") continue;

                var oldValue = prop.GetValue(oldData)?.ToString() ?? "";
                var newValue = prop.GetValue(newData)?.ToString() ?? "";

                if (oldValue != newValue)
                {
                    changes.Add($"{prop.Name}: {oldValue} → {newValue}");
                }
            }

            return changes.Count > 0 ? string.Join(" | ", changes) : "No changes detected";
        }
    }
}