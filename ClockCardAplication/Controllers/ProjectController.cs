using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClockCardAplication.DAL;
using ClockCardAplication.Models;

namespace ClockCardAplication.Controllers
{
    public class ProjectController : Controller
    {
        private UnitOfWork unitOfWork;

        public ProjectController()
        {
            this.unitOfWork = new UnitOfWork();
        }

        public ProjectController(UnitOfWork uow)
        {
            this.unitOfWork = uow;
        }

        // GET: Project
        public ActionResult Index(string sortOrder)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

                int sessionUserId = int.Parse(Session["UserID"].ToString());

                var projects = from c in unitOfWork.ProjectRepository.Get(c => c.userId == sessionUserId)
                               select c;

                switch (sortOrder)
                {
                    case "name_desc":
                        projects = projects.OrderByDescending(s => s.customer.fullName);
                        break;
                    case "Date":
                        projects = projects.OrderBy(s => s.startDate);
                        break;
                    case "date_desc":
                        projects = projects.OrderByDescending(s => s.startDate);
                        break;
                    default:
                        projects = projects.OrderBy(s => s.customer.fullName);
                        break;
                }
                return View(projects.ToList());
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
                Project project = unitOfWork.ProjectRepository.GetByID(id.Value);

                if (project == null)
                {
                    return HttpNotFound();
                }
                return View(project);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // GET: Project/Create
        public ActionResult Create()
        {
            if (Session["UserID"] != null)
            {
                int sessionUserId = int.Parse(Session["UserID"].ToString());
                ViewBag.customerId = new SelectList(unitOfWork.CustomerRepository.Get(c => c.userId == sessionUserId), "customerId", "fullName");

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "userId,customerId,name,status,startDate,expectedEndDate,timeSpentInHours")] Project project)
        {
            int sessionUserId = int.Parse(Session["UserID"].ToString());
            try
            {
                if (ModelState.IsValid)
                {
                    project.userId = sessionUserId;

                    unitOfWork.ProjectRepository.Insert(project);
                    unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewBag.customerId = new SelectList(unitOfWork.CustomerRepository.Get(c => c.userId == sessionUserId), "customerId", "fullName", project.customerId);

            return View(project);
        }

        // GET: Project/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["UserID"] != null)
            {
                int sessionUserId = int.Parse(Session["UserID"].ToString());
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Project project = unitOfWork.ProjectRepository.GetByID(id.Value);
                if (project == null)
                {
                    return HttpNotFound();
                }
                ViewBag.customerId = new SelectList(unitOfWork.CustomerRepository.Get(c => c.userId == sessionUserId), "customerId", "fullName", project.customerId);

                return View(project);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "projectId,userId,customerId,name,status,startDate,expectedEndDate,timeSpentInHours")] Project project)
        {
            int sessionUserId = int.Parse(Session["UserID"].ToString());

            try
            {
                if (ModelState.IsValid)
                {
                    project.userId = int.Parse(Session["UserID"].ToString());

                    unitOfWork.ProjectRepository.Update(project);
                    unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewBag.customerId = new SelectList(unitOfWork.CustomerRepository.Get(c => c.userId == sessionUserId), "customerId", "fullName", project.customerId);

            return View(project);
        }

        // GET: Project/Delete/5
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
                Project project = unitOfWork.ProjectRepository.GetByID(id.Value);
                if (project == null)
                {
                    return HttpNotFound();
                }
                return View(project);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                unitOfWork.ProjectRepository.Delete(id);
                unitOfWork.Save();
            }
            catch (DataException/* dex */)
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
