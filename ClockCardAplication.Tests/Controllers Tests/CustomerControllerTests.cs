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
    public class CustomerControllerTests
    {
        private Mock<ClockContext> context;
        private HomeController homeController;
        private CustomerController customerController;
        private List<User> userList;
        private List<Customer> customerList;

        [TestInitialize]
        public void SetupTests()
        {
            context = new Mock<ClockContext>();
            userList = new List<User>
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
            context.Setup(c => c.Set<User>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(userList));

            Func<Customer, int> getClassId = customer1 => customer1.customerId;
            context.Setup(c => c.Set<Customer>()).Returns(() => UnitTestsHelper.GetQueryableMockDbSet(customerList, getClassId));

            var uow = new UnitOfWork(context.Object);
            homeController = new HomeController(uow);
            homeController.ControllerContext = UnitTestsHelper.GetContextBase().Object;
            customerController = new CustomerController(uow);
            customerController.ControllerContext = UnitTestsHelper.GetContextBase().Object;
        }
        [TestMethod]
        public void CustomerController_TestRedirectViewInIndexPageWithoutLogin()
        {
            // Act
            var result = (RedirectToRouteResult)customerController.Index("");

            // Assert
            Assert.AreEqual("Login", result.RouteValues["action"]);
        }

        [TestMethod]
        public void CustomerController_TestRepositoryCallToReturnOnlyUserRelatedCustomersInIndexView()
        {
            // Arrange
            customerController.ControllerContext.HttpContext.Session["UserID"] = 2;

            //Act
            var result = customerController.Index("") as ViewResult;

            //Assert
            var customers = (IEnumerable<Customer>)result.ViewData.Model;
            Assert.AreEqual(1, customers.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers, "RonTest"), true);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers, "HarryTest"), false);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers, "HermioneTest"), false);
        }
        [TestMethod]
        public void CustomerController_TestGetDetails()
        {
            // Arrange
            customerController.ControllerContext.HttpContext.Session["UserID"] = 1;
            int idToGetDetails = customerList.Where(c => c.firstName == "HarryTest").FirstOrDefault().customerId;

            //Act
            var result = customerController.Details(idToGetDetails) as ViewResult;

            //Assert
            var customer = (Customer)result.ViewData.Model;
            Assert.AreEqual("HarryTest", customer.firstName);
        }
        [TestMethod]
        public void CustomerController_TestCreateCustomer()
        {
            // Arrange
            customerController.ControllerContext.HttpContext.Session["UserID"] = 1;
            var customerToAdd = new Customer { firstName = "SiriusTest", lastName = "BlackTest", status = CustomerStatus.Active };

            //Check
            var result1 = customerController.Index("") as ViewResult;
            var customers1 = (IEnumerable<Customer>)result1.ViewData.Model;
            Assert.AreEqual(2, customers1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers1, "SiriusTest"), false);

            //Act
            customerController.Create(customerToAdd);
            var result = customerController.Index("") as ViewResult;

            //Assert
            var customers = (IEnumerable<Customer>)result.ViewData.Model;
            Assert.AreEqual(3, customers.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers, "SiriusTest"), true);

        }
        [TestMethod]
        public void CustomerController_TestEditCustomer()
        {
            // Arrange
            customerController.ControllerContext.HttpContext.Session["UserID"] = 1;
            var customerToAddAndEdit = new Customer { firstName = "SiriusTest", lastName = "BlackTest", status = CustomerStatus.Active };
            customerController.Create(customerToAddAndEdit);

            //Check
            var result1 = customerController.Index("") as ViewResult;
            var customers1 = (IEnumerable<Customer>)result1.ViewData.Model;
            Assert.AreEqual(3, customers1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers1, "SiriusTest"), true);

            //Act
            customerToAddAndEdit.firstName = "SiriusTest1";
            customerController.Edit(customerToAddAndEdit);
            var result = customerController.Index("") as ViewResult;

            //Assert
            var customers = (IEnumerable<Customer>)result.ViewData.Model;
            Assert.AreEqual(3, customers.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers, "SiriusTest"), false);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers, "SiriusTest1"), true);

        }
        [TestMethod]
        public void CustomerController_TestDeleteCustomer()
        {
            // Arrange
            customerController.ControllerContext.HttpContext.Session["UserID"] = 1;
            int idToDelete = customerList.Where(c => c.firstName == "HarryTest").FirstOrDefault().customerId;

            //Check
            var result1 = customerController.Index("") as ViewResult;
            var customers1 = (IEnumerable<Customer>)result1.ViewData.Model;
            Assert.AreEqual(2, customers1.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers1, "HarryTest"), true);

            //Act
            customerController.Delete(idToDelete);
            var result = customerController.Index("") as ViewResult;

            //Assert
            var customers = (IEnumerable<Customer>)result.ViewData.Model;
            Assert.AreEqual(1, customers.ToList().Count);
            Assert.AreEqual(UnitTestsHelper.ListContains(customers, "HarryTest"), false);
        }
    }
}
