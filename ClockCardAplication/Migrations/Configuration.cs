namespace ClockCardAplication.Migrations
{
    using ClockCardAplication.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ClockCardAplication.DAL.ClockContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ClockCardAplication.DAL.ClockContext context)
        {
            var users = new List<User>
            {
                new User{firstName="Nikhil",lastName="Kondati",email="n.k@mail.com", password="nikhil", repeatPassword="nikhil"},
                new User{firstName="Bill",lastName="Weasley",email="b.w@mail.com", password="bill", repeatPassword="bill"}

            };
            users.ForEach(u => context.users.AddOrUpdate(p => p.email, u));
            context.SaveChanges();

            var customers = new List<Customer>
            {
                new Customer{userId=1,firstName="Harry",lastName="Potter",status=CustomerStatus.Active},
                new Customer{userId=1,firstName="Hermione",lastName="Granger",status=CustomerStatus.Active},
                new Customer{userId=2,firstName="Ron",lastName="Weasley",status=CustomerStatus.Active}

            };
            customers.ForEach(c => context.customers.AddOrUpdate(p => p.lastName, c));
            context.SaveChanges();

            var projects = new List<Project>
            {
                new Project{userId=1, customerId=1, name="Quidditch", startDate=DateTime.Parse("2017-06-14"), expectedEndDate=DateTime.Parse("2017-08-14"), status=ProjectStatus.InProgress, timeSpentInHours = 20},
                new Project{userId=1, customerId=1, name="Fantastic Beasts", startDate=DateTime.Parse("2017-07-14"), expectedEndDate=DateTime.Parse("2018-08-14"), status=ProjectStatus.InProgress, timeSpentInHours = 5}

            };
            projects.ForEach(p => context.projects.AddOrUpdate(c => c.name, p));
            context.SaveChanges();

            var timeLogs = new List<TimeLog>
            {
                new TimeLog{userId=1, customerId=1,projectId=1,date=DateTime.Parse("2017-06-18"), timeSpentInHours=10},
                new TimeLog{userId=1, customerId=1,projectId=1,date=DateTime.Parse("2017-06-28"), timeSpentInHours=10},
                new TimeLog{userId=1, customerId=1,projectId=2,date=DateTime.Parse("2017-07-15"), timeSpentInHours=5}

            };
            timeLogs.ForEach(t => context.timeLogs.Add(t));
            context.SaveChanges();
        }
    }
}