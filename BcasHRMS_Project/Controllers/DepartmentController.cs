using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Repositories.Service;
using Models.DTOs.UsersDTO;
using Models.Models;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : BaseController
    {
        private readonly tblDepartmentService _tblDepartmentService;
        private readonly TransactionEventService _transactionEventService;

        public DepartmentController(
            IHttpContextAccessor httpContextAccessor,
            tblDepartmentService tblDepartmentService,
            TransactionEventService transactionEventService
        ) : base(httpContextAccessor)
        {
            _tblDepartmentService = tblDepartmentService;
            _transactionEventService = transactionEventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlltblDepartment()
        {
            try
            {
                var data = await _tblDepartmentService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdtblDepartment(int id)
        {
            try
            {
                var data = await _tblDepartmentService.GetById(id);
                if (data == null) return NoContent();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InserttblDepartment([FromBody] tblDepartment tblDepartment)
        {
            try
            {
                var data = await _tblDepartmentService.Insert(tblDepartment);

                if (data?.DepartmentID != null)
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();
                    await LogTransactionEvent("CREATE", user, 0,
                        $"Added department: {data.DepartmentName}",
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
        public async Task<IActionResult> UpdatetblDepartment(int id, [FromBody] tblDepartment tblDepartment)
        {
            try
            {
                if (id != tblDepartment.DepartmentID) return BadRequest("Id mismatched.");

                var oldData = await _tblDepartmentService.GetById(id);
                if (oldData == null) return NotFound();

                var updatedData = await _tblDepartmentService.Update(tblDepartment);

                var user = await _transactionEventService.GetCurrentUserAsync();
                string changes = GetChanges(oldData, updatedData);

                await LogTransactionEvent("UPDATE", user, 0,
                    $"Updated department: {updatedData.DepartmentName}",
                    oldData, updatedData);

                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdtblDepartment(int id)
        {
            try
            {
                var data = await _tblDepartmentService.GetById(id);
                if (data == null) return NotFound();

                await _tblDepartmentService.DeleteById(id);

                var user = await _transactionEventService.GetCurrentUserAsync();
                await LogTransactionEvent("DELETE", user, 0,
                    $"Deleted department: {data.DepartmentName}",
                    oldData: data, newData: null);

                return Ok(new { Message = $"Department {data.DepartmentName} deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- Transaction Event Helper Methods ---
        private async Task LogTransactionEvent(string action, UserRolesDTOV2 user, int employeeId,
            string description, tblDepartment oldData, tblDepartment newData)
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
                    ? $"{newData.DepartmentName}"
                    : oldData != null
                        ? $"{oldData.DepartmentName}"
                        : "Unknown",
                Timestamp = DateTime.Now
            });
        }

        private string GetChanges(tblDepartment oldData, tblDepartment newData)
        {
            var changes = new List<string>();
            var properties = typeof(tblDepartment).GetProperties();

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