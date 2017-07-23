using ClockCardAplication.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
namespace ClockCardAplication.DAL
{
    public class ClockContext : DbContext
    {
        public ClockContext() : base("ClockContext")
        {
        }
        public DbSet<User> users { get; set; }
        public DbSet<Customer> customers { get; set; }
        public DbSet<Project> projects { get; set; }
        public DbSet<TimeLog> timeLogs { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}