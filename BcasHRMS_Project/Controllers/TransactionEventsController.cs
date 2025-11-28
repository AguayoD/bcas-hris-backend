using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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

                // Check if the user has Admin or HR role
                bool isPrivileged =
                    user.Roles.Any(r => r.RoleName == "Admin" || r.RoleName == "HR");

                if (isPrivileged)
                {
                    // Admin & HR see all events
                    events = await _transactionEventService.GetAllAsync();
                }
                else
                {
                    // Regular employees only see their own events
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
