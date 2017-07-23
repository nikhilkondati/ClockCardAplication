using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClockCardAplication.DAL;
using ClockCardAplication.Models;

namespace ClockCardAplication.Controllers
{
    public class TimeLogController : Controller
    {
        private UnitOfWork unitOfWork;

        public TimeLogController()
        {
            this.unitOfWork = new UnitOfWork();
        }

        public TimeLogController(UnitOfWork uow)
        {
            this.unitOfWork = uow;
        }

        // GET: TimeLog
        public ActionResult Index(string sortOrder, string searchString)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

                int sessionUserId = int.Parse(Session["UserID"].ToString());
                var timeLogs = from c in unitOfWork.TimeLogRepository.Get(c => c.userId == sessionUserId)
                               select c;
                //var timeLogs = from c in unitOfWork.TimeLogRepository.Get(c => c.project.customer.user.userId == sessionUserId)
                //               select c;

                if (!String.IsNullOrEmpty(searchString))
                {
                    timeLogs = timeLogs.Where(s => s.project.name.ToUpper().Contains(searchString.ToUpper()));
                }
                switch (sortOrder)
                {
                    case "name_desc":
                        timeLogs = timeLogs.OrderByDescending(s => s.project.name);
                        break;
                    case "Date":
                        timeLogs = timeLogs.OrderBy(s => s.date);
                        break;
                    case "date_desc":
                        timeLogs = timeLogs.OrderByDescending(s => s.date);
                        break;
                    default:
                        timeLogs = timeLogs.OrderBy(s => s.project.name);
                        break;
                }
                ViewBag.projectId = new SelectList(unitOfWork.ProjectRepository.Get(c => c.userId == sessionUserId), "projectId", "name");

                return View(timeLogs.ToList());
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }
        
        // GET: TimeLog/Create
        public ActionResult Create()
        {
            if (Session["UserID"] != null)
            {
                int sessionUserId = int.Parse(Session["UserID"].ToString());
                ViewBag.projectId = new SelectList(unitOfWork.ProjectRepository.Get(c => c.userId == sessionUserId), "projectId", "name");

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // POST: TimeLog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "userId,customerId,projectId,date,timeSpentInHours")] TimeLog timeLog)
        {
            int sessionUserId = int.Parse(Session["UserID"].ToString());
            try
            {
                Project project = unitOfWork.ProjectRepository.GetByID(timeLog.projectId.Value);


                if (timeLog.date.Date < project.startDate)
                {
                    ViewBag.Message = "Log date cannot be before project start.";
                }
                else if (ModelState.IsValid)
                {
                    timeLog.userId = sessionUserId;

                    timeLog.customerId = project.customerId;

                    unitOfWork.TimeLogRepository.Insert(timeLog);
                    
                    unitOfWork.Save();

                    updateProject(project);

                    return RedirectToAction("Index");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewBag.projectId = new SelectList(unitOfWork.ProjectRepository.Get(c => c.userId == sessionUserId), "projectId", "name", timeLog.projectId);

            return View(timeLog);
        }

        private void updateProject(Project project)
        {
            var timeLogs = from c in unitOfWork.TimeLogRepository.Get(c => c.projectId == project.projectId)
                           select c;
            int totalTime = 0;
            foreach (TimeLog timeLog in timeLogs)
            {
                totalTime += timeLog.timeSpentInHours;
            }
            project.timeSpentInHours = totalTime;
            unitOfWork.ProjectRepository.Update(project);
            unitOfWork.Save();
        }

        // GET: TimeLog/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["UserID"] != null)
            {
                int sessionUserId = int.Parse(Session["UserID"].ToString());

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                TimeLog timeLog = unitOfWork.TimeLogRepository.GetByID(id.Value);
                if (timeLog == null)
                {
                    return HttpNotFound();
                }
                ViewBag.projectId = new SelectList(unitOfWork.ProjectRepository.Get(c => c.userId == sessionUserId), "Project", "name", timeLog.projectId);

                return View(timeLog);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // POST: TimeLog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "timeLogId,userId,customerId,projectId,date,timeSpentInHours")] TimeLog timeLog)
        {
            int sessionUserId = int.Parse(Session["UserID"].ToString());
            try
            {
                if (ModelState.IsValid)
                {
                    timeLog.userId = int.Parse(Session["UserID"].ToString());
                    unitOfWork.TimeLogRepository.Update(timeLog);

                    Project project = unitOfWork.ProjectRepository.GetByID(timeLog.projectId.Value);

                    unitOfWork.Save();

                    updateProject(project);

                    return RedirectToAction("Index");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewBag.projectId = new SelectList(unitOfWork.ProjectRepository.Get(c => c.userId == sessionUserId), "Project", "name", timeLog.projectId);

            return View(timeLog);
        }

        // GET: TimeLog/Delete/5
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
                TimeLog timeLog = unitOfWork.TimeLogRepository.GetByID(id.Value);

                if (timeLog == null)
                {
                    return HttpNotFound();
                }
                return View(timeLog);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // POST: TimeLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                TimeLog timeLog = unitOfWork.TimeLogRepository.GetByID(id);
                Project project = unitOfWork.ProjectRepository.GetByID(timeLog.projectId.Value);

                unitOfWork.TimeLogRepository.Delete(id);                

                unitOfWork.Save();

                updateProject(project);

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
