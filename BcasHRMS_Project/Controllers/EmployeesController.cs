using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.UsersDTO;
using Models.Models;
using Repositories.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : BaseController
    {
        private readonly tblEmployeeService _tblEmployeeService;
        private readonly TransactionEventService _transactionEventService;

        public EmployeesController(
            IHttpContextAccessor httpContextAccessor,
            tblEmployeeService tblEmployeeService,
            TransactionEventService transactionEventService
        ) : base(httpContextAccessor)
        {
            _tblEmployeeService = tblEmployeeService;
            _transactionEventService = transactionEventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlltblEmployees()
        {
            try
            {
                var data = await _tblEmployeeService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdtblEmployees(int id)
        {
            try
            {
                var data = await _tblEmployeeService.GetById(id);
                if (data == null) return NoContent();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InserttblEmployees([FromBody] tblEmployees tblEmployees)
        {
            try
            {
                var data = await _tblEmployeeService.Insert(tblEmployees);

                if (data?.EmployeeID != null)
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();

                    await LogTransactionEvent("CREATE", user, data.EmployeeID.Value,
                        $"Added employee: {data.FirstName} {data.LastName}",
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
        public async Task<IActionResult> UpdatetblEmployees(int id, [FromBody] tblEmployees tblEmployees)
        {
            try
            {
                if (id != tblEmployees.EmployeeID)
                    return BadRequest("Id mismatched.");

                var oldData = await _tblEmployeeService.GetById(id);
                if (oldData == null) return NotFound();

                var updatedData = await _tblEmployeeService.Update(tblEmployees);
                var user = await _transactionEventService.GetCurrentUserAsync();

                string changes = GetChanges(oldData, updatedData);

                await LogTransactionEvent("UPDATE", user, updatedData.EmployeeID.Value,
                    $"Updated Employee: {updatedData.FirstName} {updatedData.LastName}",
                    oldData, updatedData);

                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdtblEmployees(int id)
        {
            try
            {
                var data = await _tblEmployeeService.GetById(id);
                if (data == null) return NotFound();

                await _tblEmployeeService.DeleteById(id);

                var user = await _transactionEventService.GetCurrentUserAsync();

                await LogTransactionEvent("DELETE", user, id,
                    $"Deleted employee: {data.FirstName} {data.LastName}",
                    oldData: data, newData: null);

                return Ok(new { Message = $"Employee {data.FirstName} {data.LastName} deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- Helper Methods ---

        private async Task LogTransactionEvent(string action, UserRolesDTOV2 user, int employeeId,
            string description, tblEmployees oldData, tblEmployees newData)
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
                    ? $"{newData.FirstName} {newData.LastName}"
                    : oldData != null
                        ? $"{oldData.FirstName} {oldData.LastName}"
                        : "Unknown",
                Timestamp = DateTime.Now
            });
        }

        private string GetChanges(tblEmployees oldData, tblEmployees newData)
        {
            var changes = new List<string>();
            var properties = typeof(tblEmployees).GetProperties();

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