using System.Linq;
using System.Web.Mvc;
using ClockCardAplication.Models;
using ClockCardAplication.DAL;
using System.Data;

namespace ClockCardAplication.Controllers
{
    public class HomeController : Controller
    {

        private UnitOfWork unitOfWork;

        public HomeController()
        {
            this.unitOfWork = new UnitOfWork();
        }

        public HomeController(UnitOfWork uow)
        {
            this.unitOfWork = uow;
        }


        public ActionResult HomeIndex()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Login()
        {
            ViewBag.Message = "Login";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {
            try
            {
                User dbUser = unitOfWork.UserRepository.Get(c => c.email.Equals(user.email) && c.password.Equals(user.password)).FirstOrDefault();
                if (dbUser != null)
                {
                    Session["UserID"] = dbUser.userId.ToString();
                    Session["UserName"] = dbUser.firstName.ToString();
                    return RedirectToAction("HomeIndex");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid credentials. Please try again.";
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to Login. Try again, and if the problem persists see your system administrator.");
            }
            return View("Login", user);
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            try
            {
                User existingUser = unitOfWork.UserRepository.Get(c => c.email.Equals(user.email)).FirstOrDefault();

                if (existingUser != null)
                {
                    ViewBag.Message2 = "Email already taken. Please use another email.";
                    return View();
                }
                else if (user.password != user.repeatPassword)
                {
                    ViewBag.Message3 = "Repeat password is not same as password. Please try again.";
                    return View();
                }
                else if (ModelState.IsValid)
                {
                    unitOfWork.UserRepository.Insert(user);
                    unitOfWork.Save();
                    ModelState.Clear();
                    ViewBag.Message = "Congratulations, Your email:" + user.email + " is registered succesfully.";
                    ViewBag.Message1 = "to login. And start using FreelancerTimeLog Application";
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View();
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