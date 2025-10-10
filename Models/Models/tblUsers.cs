
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class tblUsers
    {
        [Key]
        public int? UserId { get; set; }
        public int? EmployeeId { get; set; }
        public int? RoleId { get; set; }
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public string? Salt { get; set; }
        public bool? IsActive { get; set; } = true;
    }
}
