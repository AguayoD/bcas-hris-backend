using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class tblPositions
    {
        [Key]
        public int? PositionID { get; set; }
        public string? PositionName { get; set; }
        public string? Description { get; set; }

    }
}
