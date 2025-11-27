// Models/Models/AuditLog.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogID { get; set; }

        [Required]
        public string TableName { get; set; }

        [Required]
        public string Action { get; set; }

        public string RecordID { get; set; }

        // NEW - Human readable message
        public string Description { get; set; }

        [Required]
        public int UserID { get; set; }

        public string UserName { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }

    public class TransactionEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Action { get; set; }        // CREATE, UPDATE, DELETE

        public string? Description { get; set; }   // e.g., "Employee John Doe updated: FirstName: 'Brent' -> 'Lance'"

        [Required]
        public int? UserID { get; set; }           // Who performed the action

        public string? UserName { get; set; }
        public string? Fullname { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}