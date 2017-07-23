using ClockCardAplication.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClockCardAplication.Tests.Controllers_Tests
{
    class UnitTestsHelper
    {
        public static bool ListContains(IEnumerable<Customer> customers, string v)
        {
            return customers.Any(c => c.firstName == v);
        }
        public static bool ListContains(IEnumerable<Project> projects, string v)
        {
            return projects.Any(c => c.name == v);
        }
        public static bool ListContains(IEnumerable<TimeLog> timeLogs, string v, string d)
        {
            return timeLogs.Any(c => c.project.name == v && c.date.Date.ToShortDateString() == d);
        }
        public static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList, Func<T, int> getClassId = null) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            mockDbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));
            mockDbSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>((entity) => sourceList.Remove(entity));
            if (getClassId != null)
            {
                mockDbSet.Setup(x => x.Find(It.IsAny<object[]>())).Returns<object[]>(x => sourceList.FirstOrDefault(y => getClassId(y) == (int)x[0]) as T);
            }
            return mockDbSet.Object;
        }
        public static Mock<ControllerContext> GetContextBase()
        {
            var fakeHttpContext = new Mock<System.Web.HttpContextBase>();
            var session = new MockHttpSession();
            fakeHttpContext.Setup(ctx => ctx.Session).Returns(session);

            var controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(t => t.HttpContext).Returns(fakeHttpContext.Object);

            return controllerContext;
        }
        public class MockHttpSession : HttpSessionStateBase
        {
            Dictionary<string, object> m_SessionStorage = new Dictionary<string, object>();

            public override object this[string name]
            {
                get
                {
                    try
                    {
                        return m_SessionStorage[name];
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                set { m_SessionStorage[name] = value; }
            }

        }
    }
}
