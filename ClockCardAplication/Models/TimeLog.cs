using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClockCardAplication.Models
{
    public class TimeLog
    {
        public int timeLogId { get; set; }
        public int? userId { get; set; }
        public int? customerId { get; set; }
        public int? projectId { get; set; }

        [Required]
        [Display(Name = "Log Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime date { get; set; }

        [Required]
        [Display(Name = "Log Time (In Hours)")]
        public int timeSpentInHours { get; set; }

        public virtual User user { get; set; }
        public virtual Customer customer { get; set; }
        public virtual Project project { get; set; }

    }
}