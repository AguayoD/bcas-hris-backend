
namespace Models.DTOs.UsersDTO
{
    public class UserInsertDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? NewPassword { get; set; }
        public int? RoleId { get; set; }
        public int? EmployeeId { get; set; }
    }
}
