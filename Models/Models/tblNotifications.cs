using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class tblNotifications
    {
        [Key]
        public int? NotificationID { get; set; }
        public int? EmployeeID { get; set; }
        public string? Message { get;set; }
        public string? NotificationType { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}
