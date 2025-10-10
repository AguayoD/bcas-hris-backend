using System.ComponentModel.DataAnnotations;

namespace Models.DTOs.UsersDTO
{
    public class UserUpdateDTO
    {
        [Required]
        public int UserId { get; set; }

        public int? EmployeeId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; }

        // Optional - only include if password is being changed
        public string? NewPassword { get; set; }

        public bool IsActive { get; set; } = true;
    }
}