using Models.Enums;
using System.Data;
using Models.Models;
using Dapper;

namespace Repositories.Repositories
{
    public class tblUserRoleRepository : tblGenericRepository<tblRoles>
    {
        public async Task<IEnumerable<tblRoles>> GetByUserId(int userId)
        {
            var procedureName = StoredProcedures.tblRoles_GetByUserId;
            return await _connection.QueryAsync<tblRoles>(procedureName.ToString(), new { UserId = userId }, commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
        }
    }
}
