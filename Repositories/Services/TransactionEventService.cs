// Services/TransactionEventService.cs
using Dapper;
using Microsoft.AspNetCore.Http;
using Models.DTOs.UsersDTO;
using Models.Models;
using Repositories.Repositories;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Repositories.Service
{
    public class TransactionEventService
    {
        private readonly IDbConnection _connection;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly tblGenericRepository<tblEmployees> _repository;

        public TransactionEventService(IDbConnection connection, IHttpContextAccessor contextAccessor, tblGenericRepository<tblEmployees> repository)
        {
            _connection = connection;
            _contextAccessor = contextAccessor;
            _repository = repository;
        }

        public async Task<int> InsertAsync(TransactionEvent log)
        {
            var sql = @"
                INSERT INTO TransactionEvent (Action, Description, UserID, UserName, Fullname, Timestamp)
                VALUES (@Action, @Description, @UserID, @UserName, @Fullname, @Timestamp);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await _connection.ExecuteScalarAsync<int>(sql, log);
        }

        public async Task<UserRolesDTOV2> GetCurrentUserAsync()
        {
            var userClaims = _contextAccessor.HttpContext?.User;

            if (userClaims == null || !userClaims.Identity!.IsAuthenticated)
                throw new Exception("User not authenticated");

            var userId = userClaims.FindFirst("UserId")?.Value;
            var employeeId = userClaims.FindFirst("EmployeeId")?.Value;
            var departmentId = userClaims.FindFirst("DepartmentID")?.Value;
            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;

            int.TryParse(userId, out int parsedUserId);
            int.TryParse(employeeId, out int parsedEmployeeId);
            int.TryParse(departmentId, out int parsedDepartmentId);

            // ✅ Get employee info via Dapper
            var employee = await _repository.GetEmployeeByIdAsync(parsedEmployeeId);
            var user = await _repository.GetUserDetails(parsedUserId);


            if (employee == null)
                throw new Exception("Employee not found.");

            return new UserRolesDTOV2
            {
                UserId = parsedUserId,
                EmployeeId = parsedEmployeeId,
                DepartmentID = parsedDepartmentId,
                Username = user.UserName,
                Fullname = $"{employee.FirstName} {employee.LastName}",

                Roles = new List<tblRoles>
        {
            new tblRoles { RoleName = role }
        }
            };
        }

        public async Task<IEnumerable<TransactionEvent>> GetAllAsync()
        {
            var sql = "SELECT * FROM TransactionEvent ORDER BY Timestamp DESC";
            return await _connection.QueryAsync<TransactionEvent>(sql);
        }

        public async Task<IEnumerable<TransactionEvent>> GetByEmployeeIdAsync(int employeeId)
        {
            var sql = @"SELECT * FROM TransactionEvent 
                WHERE Fullname IN (SELECT FirstName + ' ' + LastName FROM tblEmployees WHERE EmployeeID = @EmployeeId)
                ORDER BY Timestamp DESC";
            return await _connection.QueryAsync<TransactionEvent>(sql, new { EmployeeId = employeeId });
        }


    }
}