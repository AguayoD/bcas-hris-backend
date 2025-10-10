
using Models.Models;

namespace Models.DTOs.UsersDTO
{
    public class UserRolesDTO
    {
        public int? UserId { get; set; }
        public int? EmployeeId { get; set; }
        public int? RoleId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public IEnumerable<tblRoles>? Roles { get; set; }
    }
}
