using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClockCardAplication.Models
{
    public enum ProjectStatus
    {
        NotStarted, InProgress, Completed, Abandoned
    }

    public class Project
    {
        public int projectId { get; set; }
        public int? userId { get; set; }
        public int? customerId { get; set; }

        [Required]
        [Display(Name = "Project")]
        public string name { get; set; }

        [Required]
        [Display(Name = "Project Status")]
        public ProjectStatus status { get; set; }

        [Required]
        [Display(Name = "Project Start")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime startDate { get; set; }

        [Required]
        [Display(Name = "Expected End")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime expectedEndDate { get; set; }


        [Display(Name = "Time Spent (In hours)")]
        public int timeSpentInHours { get; set; }        

        public virtual User user { get; set; }
        public virtual Customer customer { get; set; }

        [Display(Name = "Project Logs =>")]
        public virtual ICollection<TimeLog> timeLogs { get; set; }


    }
}