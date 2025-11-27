using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionEventsController : ControllerBase
    {
        private readonly TransactionEventService _transactionEventService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionEventsController(
            TransactionEventService transactionEventService,
            IHttpContextAccessor httpContextAccessor)
        {
            _transactionEventService = transactionEventService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactionEvents()
        {
            try
            {
                // Get current user details
                var user = await _transactionEventService.GetCurrentUserAsync();

                IEnumerable<TransactionEvent> events;

                // Check if any of the roles is Admin
                bool isAdmin = user.Roles.Any(r => r.RoleName == "Admin");

                if (isAdmin)
                {
                    // Admin sees all events
                    events = await _transactionEventService.GetAllAsync();
                }
                else
                {
                    // Employees see only events related to their EmployeeId
                    events = await _transactionEventService.GetByEmployeeIdAsync((int)user.EmployeeId);
                }

                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}