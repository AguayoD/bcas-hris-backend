
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class tblRoles
    {
        [Key]
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
