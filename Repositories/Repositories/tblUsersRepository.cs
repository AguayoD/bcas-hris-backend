
using Dapper;
using Models.DTOs.UsersDTO;
using Models.Enums;
using Models.Models;
using System.Data;

namespace Repositories.Repositories
{
    public class tblUsersRepository : tblGenericRepository<tblUsers>
    {
  
        public async Task<tblUsers> GetByUsername(string username)
        {
            string procedureName = StoredProcedures.tblUsers_GetByUsername.ToString();
            return await _connection.QueryFirstOrDefaultAsync<tblUsers>
                  (procedureName, new { Username = username }, commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
        }
        public async Task<UserRolesDTO> GetByIdWithRoles(int id)
        {
            tblUserRoleRepository _userRefRoleRepo = new tblUserRoleRepository();
            var user = await GetById(id);
            var roles = await _userRefRoleRepo.GetByUserId(id);
            var result = new UserRolesDTO
            {
                UserId = id,
                EmployeeId = user?.EmployeeId,
                RoleId = user?.RoleId,
                Username = user?.UserName,
                Roles = roles,
            };
            return result;
        }

        //ADDED
        public async Task<tblUsers?> GetByEmail(string email)
        {
            string procedureName = StoredProcedures.tblUsers_GetByEmail.ToString();
            return await _connection.QueryFirstOrDefaultAsync<tblUsers>(
                procedureName, new { Email = email },
                commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
        }

        public async Task<bool> SaveResetToken(int userId, string token)
        {
            string procedureName = StoredProcedures.tblUsers_SaveResetToken.ToString();
            await _connection.ExecuteAsync(procedureName,
                new { UserId = userId, ResetToken = token, Expiry = DateTime.UtcNow.AddMinutes(30) },
                commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);

            return true; // assume success unless exception thrown
        }


        public async Task<tblUsers?> GetUserByResetToken(string token)
        {
            string procedureName = StoredProcedures.tblUsers_GetByResetToken.ToString();
            return await _connection.QueryFirstOrDefaultAsync<tblUsers>(
                procedureName, new { ResetToken = token },
                commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
        }

        public async Task<bool> ClearResetToken(int userId)
        {
            string procedureName = StoredProcedures.tblUsers_ClearResetToken.ToString();
            var result = await _connection.ExecuteAsync(procedureName,
                new { UserId = userId },
                commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
            return result > 0;
        }

        public async Task<bool> UpdatePassword(int userId, string passwordHash, string salt)
        {
            string procedureName = StoredProcedures.tblUsers_UpdatePassword.ToString();
            var result = await _connection.ExecuteAsync(procedureName,
                new { UserId = userId, PasswordHash = passwordHash, Salt = salt },
                commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
            return true;
        }
    }
}
