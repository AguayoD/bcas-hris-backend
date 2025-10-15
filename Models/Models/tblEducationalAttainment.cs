using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class tblEducationalAttainment
    {
        [Key]
        public int? EducationalAttainmentID { get; set; }
        public string? AttainmentName { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}