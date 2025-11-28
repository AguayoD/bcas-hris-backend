using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Service;
using Models.DTOs.UsersDTO;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmploymentStatusController : BaseController
    {
        private readonly tblEmploymentStatusService _employmentStatusService;
        private readonly TransactionEventService _transactionEventService;

        public EmploymentStatusController(
            IHttpContextAccessor httpContextAccessor,
            tblEmploymentStatusService employmentStatusService,
            TransactionEventService transactionEventService
        ) : base(httpContextAccessor)
        {
            _employmentStatusService = employmentStatusService;
            _transactionEventService = transactionEventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmploymentStatus()
        {
            try
            {
                var data = await _employmentStatusService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdEmploymentStatus(int id)
        {
            try
            {
                var data = await _employmentStatusService.GetById(id);
                if (data == null) return NoContent();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertEmploymentStatus([FromBody] tblEmploymentStatus employmentStatus)
        {
            try
            {
                var data = await _employmentStatusService.Insert(employmentStatus);

                if (data?.EmploymentStatusID != null)
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();
                    await LogTransactionEvent("CREATE", user, 0,
                        $"Added employment status: {data.StatusName}",
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
        public async Task<IActionResult> UpdateEmploymentStatus(int id, [FromBody] tblEmploymentStatus employmentStatus)
        {
            try
            {
                if (id != employmentStatus.EmploymentStatusID) return BadRequest("Id mismatched.");

                var oldData = await _employmentStatusService.GetById(id);
                if (oldData == null) return NotFound();

                var updatedData = await _employmentStatusService.Update(employmentStatus);

                var user = await _transactionEventService.GetCurrentUserAsync();
                string changes = GetChanges(oldData, updatedData);

                await LogTransactionEvent("UPDATE", user, 0,
                    $"Updated employment status: {updatedData.StatusName}",
                    oldData, updatedData);

                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdEmploymentStatus(int id)
        {
            try
            {
                var data = await _employmentStatusService.GetById(id);
                if (data == null) return NotFound();

                await _employmentStatusService.DeleteById(id);

                var user = await _transactionEventService.GetCurrentUserAsync();
                await LogTransactionEvent("DELETE", user, 0,
                    $"Deleted employment status: {data.StatusName}",
                    oldData: data, newData: null);

                return Ok(new { Message = $"Employment status {data.StatusName} deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- Transaction Event Helper Methods ---
        private async Task LogTransactionEvent(string action, UserRolesDTOV2 user, int employeeId,
            string description, tblEmploymentStatus oldData, tblEmploymentStatus newData)
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
                    ? $"{newData.StatusName}"
                    : oldData != null
                        ? $"{oldData.StatusName}"
                        : "Unknown",
                Timestamp = DateTime.Now
            });
        }

        private string GetChanges(tblEmploymentStatus oldData, tblEmploymentStatus newData)
        {
            var changes = new List<string>();
            var properties = typeof(tblEmploymentStatus).GetProperties();

            foreach (var prop in properties)
            {
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