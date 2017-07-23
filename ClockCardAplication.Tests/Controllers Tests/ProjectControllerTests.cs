using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClockCardAplication.DAL;
using Moq;
using ClockCardAplication.Controllers;
using ClockCardAplication.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System;
using System.Linq;

namespace ClockCardAplication.Tests.Controllers_Tests
{
    [TestClass]
    public class ProjectControllerTests
    {
        private Mock<ClockContext> context;
        private HomeController homeController;
        private CustomerController customerController;
        private ProjectController projectController;
        private List<Customer> customerList;
        private List<Project> projectList;


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
                    new Project{userId=1, customerId=1, projectId=1, name="Quidditch", startDate=DateTime.Parse("2017-06-14"), expectedEndDate=DateTime.Parse("2017-08-14"), status=ProjectStatus.InProgress, timeSpentInHours = 20, customer=customerList.Where(c => c.customerId == 1).FirstOrDefault()},
                    new Project{userId=2, customerId=3, projectId=2, name="Fantastic Beasts", startDate=DateTime.Parse("2017-07-14"), expectedEndDate=DateTime.Parse("2018-08-14"), status=ProjectStatus.InProgress, timeSpentInHours = 5, customer=customerList.Where(c => c.customerId == 3).FirstOrDefault()}
                };
            context.Setup(c => c.Set<User>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(userList));
            context.Setup(c => c.Set<Customer>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(customerList));

            Func<Project, int> getClassId = project1 => project1.projectId;
            context.Setup(c => c.Set<Project>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(projectList, getClassId));


            var uow = new UnitOfWork(context.Object);
            homeController = new HomeController(uow);
            homeController.ControllerContext = UnitTestsHelper.GetContextBase().Object;
            customerController = new CustomerController(uow);
            customerController.ControllerContext = UnitTestsHelper.GetContextBase().Object;
            projectController = new ProjectController(uow);
            projectController.ControllerContext = UnitTestsHelper.GetContextBase().Object;
        }
        [TestMethod]
        public void ProjectController_TestRedirectViewInIndexPageWithoutLogin()
        {
            // Act
            var result = (RedirectToRouteResult)projectController.Index("");

            // Assert
            Assert.AreEqual("Login", result.RouteValues["action"]);
        }

        [TestMethod]
        public void ProjectController_TestRepositoryCallToReturnOnlyUserRelatedProjectsInIndexView()
        {
            // Arrange
            projectController.ControllerContext.HttpContext.Session["UserID"] = 1;

            //Act
            var result = projectController.Index("") as ViewResult;

            //Assert
            var projects = (IEnumerable<Project>)result.ViewData.Model;
            Assert.AreEqual(1, projects.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(projects, "Quidditch"), true);
            Assert.AreEqual(UnitTestsHelper.ListContains(projects, "Fantastic Beasts"), false);

        }
        [TestMethod]
        public void ProjectController_TestGetDetails()
        {
            // Arrange
            projectController.ControllerContext.HttpContext.Session["UserID"] = 1;
            int idToGetDetails = projectList.Where(c => c.name == "Quidditch").FirstOrDefault().projectId;

            //Act
            var result = projectController.Details(idToGetDetails) as ViewResult;

            //Assert
            var project = (Project)result.ViewData.Model;
            Assert.AreEqual("Quidditch", project.name);
        }
        [TestMethod]
        public void ProjectController_TestCreateProject()
        {
            // Arrange
            projectController.ControllerContext.HttpContext.Session["UserID"] = 1;
            var projectToAdd = new Project { customerId = 2,userId=1, name = "Elf Liberation", startDate = DateTime.Parse("2017-07-14"), expectedEndDate = DateTime.Parse("2018-08-14"), status = ProjectStatus.InProgress, customer = customerList.Where(c => c.customerId == 2).FirstOrDefault() };

            //Check
            var result1 = projectController.Index("") as ViewResult;
            var projects1 = (IEnumerable<Project>)result1.ViewData.Model;
            Assert.AreEqual(1, projects1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(projects1, "Elf Liberation"), false);

            //Act
            projectController.Create(projectToAdd);
            var result = projectController.Index("") as ViewResult;

            //Assert
            var projects = (IEnumerable<Project>)result.ViewData.Model;
            Assert.AreEqual(2, projects.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(projects, "Elf Liberation"), true);

        }
        [TestMethod]
        public void ProjectController_TestEditProject()
        {
            // Arrange
            projectController.ControllerContext.HttpContext.Session["UserID"] = 1;
            var projectToAddAndEdit = new Project { customerId = 2, userId = 1, name = "Elf Liberation", startDate = DateTime.Parse("2017-07-14"), expectedEndDate = DateTime.Parse("2018-08-14"), status = ProjectStatus.InProgress, customer = customerList.Where(c => c.customerId == 2).FirstOrDefault() };
            projectController.Create(projectToAddAndEdit);

            //Check
            var result1 = projectController.Index("") as ViewResult;
            var projects1 = (IEnumerable<Project>)result1.ViewData.Model;
            Assert.AreEqual(2, projects1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(projects1, "Elf Liberation"), true);

            //Act
            projectToAddAndEdit.name = "Elf Liberation1";
            projectController.Edit(projectToAddAndEdit);
            var result = projectController.Index("") as ViewResult;

            //Assert
            var projects = (IEnumerable<Project>)result.ViewData.Model;
            Assert.AreEqual(2, projects.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(projects, "Elf Liberation"), false);
            Assert.AreEqual(UnitTestsHelper.ListContains(projects, "Elf Liberation1"), true);
        }
        [TestMethod]
        public void ProjectController_TestDeleteProject()
        {
            // Arrange
            projectController.ControllerContext.HttpContext.Session["UserID"] = 1;
            int idToDelete = projectList.Where(c => c.name == "Quidditch").FirstOrDefault().projectId;

            //Check
            var result1 = projectController.Index("") as ViewResult;
            var projects1 = (IEnumerable<Project>)result1.ViewData.Model;
            Assert.AreEqual(1, projects1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(projects1, "Quidditch"), true);

            //Act
            projectController.Delete(idToDelete);
            var result = projectController.Index("") as ViewResult;

            //Assert
            var projects = (IEnumerable<Project>)result.ViewData.Model;
            Assert.AreEqual(0, projects.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(projects, "Quidditch"), false);
        }
    }
}
