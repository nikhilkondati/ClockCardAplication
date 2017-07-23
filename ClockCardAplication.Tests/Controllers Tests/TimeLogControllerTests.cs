using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClockCardAplication.DAL;
using Moq;
using ClockCardAplication.Controllers;
using ClockCardAplication.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System;

namespace ClockCardAplication.Tests.Controllers_Tests
{
    [TestClass]
    public class TimeLogControllerTests
    {
        private Mock<ClockContext> context;

        private HomeController homeController;
        private CustomerController customerController;
        private ProjectController projectController;
        private TimeLogController timeLogController;

        private List<Customer> customerList;
        private List<Project> projectList;
        private List<TimeLog> timeLogList;

        [TestInitialize]
        public void SetupTests()
        {
            context = new Mock<ClockContext>();
            List<User> userList = new List<User>
                {
                    new User { userId = 1, firstName = "BillTest", lastName = "WeasleyTest", email = "bTest.w@mail.com", password = "bill", repeatPassword = "bill" },
                    new User { userId = 2, firstName = "CharlieTest", lastName = "WeasleyTest", email = "cTest.w@mail.com", password = "charlie", repeatPassword = "charlie" }
                };
            customerList = new List<Customer>
                {
                    new Customer { userId = 1, customerId=1, firstName = "HarryTest", lastName = "PotterTest", status = CustomerStatus.Active },
                    new Customer { userId = 1, customerId=2, firstName = "HermioneTest", lastName = "GrangerTest", status = CustomerStatus.Active },
                    new Customer { userId = 2, customerId=3, firstName = "RonTest", lastName = "WeasleyTest", status = CustomerStatus.Active }

                };
            projectList = new List<Project>
                {
                    new Project{userId=1, customerId=1, projectId=1, name="Quidditch", startDate=DateTime.Parse("2017-06-14"), expectedEndDate=DateTime.Parse("2017-08-14"), status=ProjectStatus.InProgress, timeSpentInHours = 20},
                    new Project{userId=2, customerId=1, projectId=2, name="Fantastic Beasts", startDate=DateTime.Parse("2017-07-14"), expectedEndDate=DateTime.Parse("2018-08-14"), status=ProjectStatus.InProgress, timeSpentInHours = 5}
                };
            timeLogList = new List<TimeLog>
            {
                new TimeLog{userId=1, customerId=1,projectId=1,timeLogId=1,date=DateTime.Parse("2017-06-18"), timeSpentInHours=10, project=projectList.Where(c => c.projectId == 1).FirstOrDefault()},
                new TimeLog{userId=1, customerId=1,projectId=1,timeLogId=2,date=DateTime.Parse("2017-06-28"), timeSpentInHours=10, project=projectList.Where(c => c.projectId == 1).FirstOrDefault()},
                new TimeLog{userId=2, customerId=1,projectId=2,timeLogId=3,date=DateTime.Parse("2017-07-15"), timeSpentInHours=5, project=projectList.Where(c => c.projectId == 2).FirstOrDefault()}
            };
            context.Setup(c => c.Set<User>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(userList));
            context.Setup(c => c.Set<Customer>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(customerList));

            Func<Project, int> getClassId = project1 => project1.projectId;
            context.Setup(c => c.Set<Project>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(projectList, getClassId));

            Func<TimeLog, int> getClassId1 = timeLog1 => timeLog1.timeLogId;
            context.Setup(c => c.Set<TimeLog>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(timeLogList, getClassId1));

            var uow = new UnitOfWork(context.Object);

            homeController = new HomeController(uow);
            homeController.ControllerContext = UnitTestsHelper.GetContextBase().Object;

            customerController = new CustomerController(uow);
            customerController.ControllerContext = UnitTestsHelper.GetContextBase().Object;

            projectController = new ProjectController(uow);
            projectController.ControllerContext = UnitTestsHelper.GetContextBase().Object;

            timeLogController = new TimeLogController(uow);
            timeLogController.ControllerContext = UnitTestsHelper.GetContextBase().Object;
        }
        [TestMethod]
        public void TimeLogController_TestRedirectViewInIndexPageWithoutLogin()
        {
            // Act
            var result = (RedirectToRouteResult)timeLogController.Index("","");

            // Assert
            Assert.AreEqual("Login", result.RouteValues["action"]);
        }

        [TestMethod]
        public void TimeLogController_TestRepositoryCallToReturnOnlyUserRelatedTimeLogsInIndexView()
        {
            // Arrange
            timeLogController.ControllerContext.HttpContext.Session["UserID"] = 1;

            //Act
            var result = timeLogController.Index("", "") as ViewResult;

            //Assert
            var timeLogs = (IEnumerable<TimeLog>)result.ViewData.Model;
            Assert.AreEqual(2, timeLogs.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs, "Quidditch", "6/18/2017"), true);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs, "Fantastic Beasts", "7/15/2017"), false);

        }
        [TestMethod]
        public void TimeLogController_TestCreateTimeLogWithLogDateBeforeProjectStart()
        {
            // Arrange
            timeLogController.ControllerContext.HttpContext.Session["UserID"] = 1;
            var timeLogToAdd = new TimeLog { userId = 1, customerId = 1, projectId = 1, date = DateTime.Parse("2017-06-13"), timeSpentInHours = 10, project = projectList.Where(c => c.projectId == 1).FirstOrDefault() };
            
            //Check
            var result1 = timeLogController.Index("", "") as ViewResult;
            var timeLogs1 = (IEnumerable<TimeLog>)result1.ViewData.Model;
            Assert.AreEqual(2, timeLogs1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs1, "Quidditch", "6/13/2017"), false);

            //Act
            var result2 = timeLogController.Create(timeLogToAdd) as ViewResult;
            var result = timeLogController.Index("","") as ViewResult;

            //Assert
            var timeLogs = (IEnumerable<TimeLog>)result.ViewData.Model;
            Assert.AreEqual(2, timeLogs.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs, "Quidditch", "6/13/2017"), false);

            Assert.AreEqual("Log date cannot be before project start.", result2.ViewData["Message"]);
        }
        [TestMethod]
        public void TimeLogController_TestCreateTimeLogWithLogDateAfterProjectStartAlsoUpdatesProjectTime()
        {
            // Arrange
            timeLogController.ControllerContext.HttpContext.Session["UserID"] = 1;
            var timeLogToAdd = new TimeLog { userId = 1, customerId = 1, projectId = 1, date = DateTime.Parse("2017-06-19"), timeSpentInHours = 10, project = projectList.Where(c => c.projectId == 1).FirstOrDefault() };
            
            //Check
            var result1 = timeLogController.Index("", "") as ViewResult;
            var timeLogs1 = (IEnumerable<TimeLog>)result1.ViewData.Model;
            Assert.AreEqual(2, timeLogs1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs1, "Quidditch", "6/19/2017"), false);
            Assert.AreEqual(timeLogToAdd.project.timeSpentInHours, 20);

            //Act
            timeLogController.Create(timeLogToAdd);
            var result = timeLogController.Index("", "") as ViewResult;

            //Assert
            var timeLogs = (IEnumerable<TimeLog>)result.ViewData.Model;
            Assert.AreEqual(3, timeLogs.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs, "Quidditch", "6/19/2017"), true);
            Assert.AreEqual(timeLogToAdd.project.timeSpentInHours, 30);
        }
        [TestMethod]
        public void TimeLogController_TestEditTimeLogAlsoUpdatesProjectTime()
        {
            // Arrange
            timeLogController.ControllerContext.HttpContext.Session["UserID"] = 1;
            var timeLogToAddAndEdit = new TimeLog { userId = 1, customerId = 1, projectId = 1, date = DateTime.Parse("2017-06-19"), timeSpentInHours = 10, project = projectList.Where(c => c.projectId == 1).FirstOrDefault() };
            timeLogController.Create(timeLogToAddAndEdit);

            //Check
            var result1 = timeLogController.Index("", "") as ViewResult;
            var timeLogs1 = (IEnumerable<TimeLog>)result1.ViewData.Model;
            Assert.AreEqual(3, timeLogs1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs1, "Quidditch", "6/19/2017"), true);

            //Act
            timeLogToAddAndEdit.date = DateTime.Parse("2017-06-20");
            var result2 = timeLogController.Edit(timeLogToAddAndEdit) as ViewResult;
            var result = timeLogController.Index("", "") as ViewResult;

            //Assert
            var timeLogs = (IEnumerable<TimeLog>)result.ViewData.Model;
            Assert.AreEqual(3, timeLogs.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs, "Quidditch", "6/19/2017"), false);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs, "Quidditch", "6/20/2017"), true);
        }
        [TestMethod]
        public void TimeLogController_TestDeleteTimeLog()
        {
            // Arrange
            timeLogController.ControllerContext.HttpContext.Session["UserID"] = 1;
            int idToDelete = timeLogList.Where(c => c.project.name == "Quidditch" && c.date.Date.ToShortDateString() == "6/18/2017").FirstOrDefault().timeLogId;

            //Check
            var result1 = timeLogController.Index("", "") as ViewResult;
            var timeLogs1 = (IEnumerable<TimeLog>)result1.ViewData.Model;
            Assert.AreEqual(2, timeLogs1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs1, "Quidditch", "6/18/2017"), true);

            //Act
            timeLogController.Delete(idToDelete);
            var result = timeLogController.Index("","") as ViewResult;

            //Assert
            var timeLogs = (IEnumerable<TimeLog>)result.ViewData.Model;
            Assert.AreEqual(1, timeLogs.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(timeLogs, "Quidditch","6/18/2017"), false);
        }
    }
}
