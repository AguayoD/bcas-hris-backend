using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class tblEmploymentStatus
    {
        [Key]
        public int? EmploymentStatusID { get; set; }
        public string? StatusName { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}