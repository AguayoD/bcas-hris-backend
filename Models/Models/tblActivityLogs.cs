using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class tblActivityLogs
    {
        [Key]
        public int? LogID { get; set; }
        public int? UserID { get; set; }
        public string? Action { get; set; }
        public DateTime? timestamp { get; set; }

    }
}
