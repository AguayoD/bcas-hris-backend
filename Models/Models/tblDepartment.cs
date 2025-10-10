using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class tblDepartment
    {
        [Key]
        public int? DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public string? Description { get; set; }

    }
}
