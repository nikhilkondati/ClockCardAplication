using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClockCardAplication.DAL;
using ClockCardAplication.Models;

namespace ClockCardAplication.Controllers
{
    public class CustomerController : Controller
    {
        private UnitOfWork unitOfWork;

        public CustomerController()
        {
            this.unitOfWork = new UnitOfWork();
        }

        public CustomerController(UnitOfWork uow)
        {
            this.unitOfWork = uow;
        }        
        
        // GET: Customer
        public ActionResult Index(string sortOrder)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

                int sessionUserId = int.Parse(Session["UserID"].ToString());
                var userCustomers = from c in unitOfWork.CustomerRepository.Get(c => c.userId == sessionUserId)
                                    select c;

                switch (sortOrder)
                {
                    case "name_desc":
                        userCustomers = userCustomers.OrderByDescending(s => s.lastName);
                        break;
                    default:
                        userCustomers = userCustomers.OrderBy(s => s.lastName);
                        break;
                }

                return View(userCustomers.ToList());
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // GET: Project/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["UserID"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Customer customer = unitOfWork.CustomerRepository.GetByID(id.Value);

                if (customer == null)
                {
                    return HttpNotFound();
                }
                return View(customer);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // GET: Customer/Create
        public ActionResult Create()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "firstName,lastName,status")] Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    customer.userId = int.Parse(Session["UserID"].ToString());
                    unitOfWork.CustomerRepository.Insert(customer);
                    unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(customer);
        }

        // GET: Customer/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["UserID"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Customer customer = unitOfWork.CustomerRepository.GetByID(id.Value);

                if (customer == null)
                {
                    return HttpNotFound();
                }
                return View(customer);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "userId, customerId, firstName,lastName,status")] Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    customer.userId = int.Parse(Session["UserID"].ToString());
                    unitOfWork.CustomerRepository.Update(customer);
                    unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(customer);
        }

        // GET: Customer/Delete/5
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (Session["UserID"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if (saveChangesError.GetValueOrDefault())
                {
                    ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
                }
                Customer customer = unitOfWork.CustomerRepository.GetByID(id.Value);
                if (customer == null)
                {
                    return HttpNotFound();
                }
                return View(customer);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                unitOfWork.CustomerRepository.Delete(id);
                unitOfWork.Save();
                
            }
            catch (DataException)
            {
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
