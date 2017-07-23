using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClockCardAplication.DAL;
using Moq;
using ClockCardAplication.Controllers;
using ClockCardAplication.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ClockCardAplication.Tests.Controllers_Tests
{
    [TestClass]
    public class HomeControllerTests
    {
        private Mock<ClockContext> context;
        private HomeController homeController;
        private List<User> userList;

        [TestInitialize]
        public void SetupTests()
        {
            context = new Mock<ClockContext>();
            userList = new List<User>
                {
                    new User { firstName = "BillTest", lastName = "WeasleyTest", email = "bTest.w@mail.com", password = "bill", repeatPassword = "bill" }
                };
            context.Setup(c => c.Set<User>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(userList));

            var uow = new UnitOfWork(context.Object);
            homeController = new HomeController(uow);
            homeController.ControllerContext = UnitTestsHelper.GetContextBase().Object;
        }
        [TestMethod]
        public void HomeController_TestLoginWithValidUser()
        {
            // Arrange                
            var user = new User { email = "bTest.w@mail.com", password = "bill"};

            // Act
            var result = (RedirectToRouteResult)homeController.Login(user);

            // Assert
            Assert.AreEqual("HomeIndex", result.RouteValues["action"]);
        }
        [TestMethod]
        public void HomeController_TestLoginWithInValidUser()
        {
            // Arrange
            var user = new User { email = "bTes.w@mail.com", password = "bill", repeatPassword = "bill" };

            // Act
            var result = homeController.Login(user) as ViewResult;

            // Assert
            Assert.AreEqual("Invalid credentials. Please try again.", result.ViewData["ErrorMessage"]);
            Assert.AreEqual("Login", result.ViewName);
        }
        [TestMethod]
        public void HomeController_TestRegisterUserWithValidDataAndLoginSuccesfully()
        {
            // Arrange
            var userToAdd = new User { firstName = "CharlieTest", lastName = "WeasleyTest", email = "cTest.w@mail.com", password = "charlie", repeatPassword = "charlie" };
            
            //Act            
            var result = homeController.Register(userToAdd) as ViewResult;
            var result1 = (RedirectToRouteResult)homeController.Login(userToAdd);
            
            //Assert
            Assert.AreEqual("Congratulations, Your email:cTest.w@mail.com is registered succesfully.", result.ViewData["Message"]);
            Assert.AreEqual("HomeIndex", result1.RouteValues["action"]);
        }
        [TestMethod]
        public void HomeController_TestRegisterUserWithExistingEmail()
        {
            // Arrange
            var userToAdd = new User { firstName = "CharlieTest", lastName = "WeasleyTest", email = "bTest.w@mail.com", password = "charlie", repeatPassword = "charlie" };

            //Act            
            var result = homeController.Register(userToAdd) as ViewResult;
            var result1 = homeController.Login(userToAdd) as ViewResult;

            //Assert
            Assert.AreEqual("Email already taken. Please use another email.", result.ViewData["Message2"]);

            Assert.AreEqual("Invalid credentials. Please try again.", result1.ViewData["ErrorMessage"]);
            Assert.AreEqual("Login", result1.ViewName);
        }
    }
}
