// [file name]: tblPendingEmployeeUpdate.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Models
{
    public class tblPendingEmployeeUpdate
    {
        [Key]
        public int PendingUpdateID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        [Required]
        public string UpdateData { get; set; } // JSON string of changes

        [Required]
        public string OriginalData { get; set; } // JSON string of original data

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "pending"; // pending, approved, rejected

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedBy { get; set; }

        [StringLength(100)]
        public string? ReviewerName { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        // Navigation property
        [ForeignKey("EmployeeID")]
        public virtual tblEmployees? Employee { get; set; }

        [ForeignKey("ReviewedBy")]
        public virtual tblUsers? Reviewer { get; set; }
    }
}