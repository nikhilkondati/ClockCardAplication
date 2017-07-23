using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClockCardAplication.Models
{
    public enum CustomerStatus
    {
        Active, Inactive
    }

    public class Customer
    {
        public int customerId { get; set; }
        public int? userId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string firstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string lastName { get; set; }

        [Required]
        [Display(Name = "Status")]
        public CustomerStatus status { get; set; }

        [Display(Name = "Customer")]
        public string fullName
        {
            get
            {
                return firstName + " " + lastName;
            }
        }

        public virtual User user { get; set; }

        [Display(Name = "Projects =>")]
        public virtual ICollection<Project> projects { get; set; }
    }
}