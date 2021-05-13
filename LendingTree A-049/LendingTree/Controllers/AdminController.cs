using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using System.Net;
using System.Dynamic;
using LendingTree.Models;
using Rotativa;

namespace LendingTree.Controllers
{
    public class AdminController : Controller
    {
        readonly private LendingContext db = new LendingContext();
        readonly private EncryptPassword encryptPassword = new EncryptPassword();

        [HttpGet]
        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Admin adminLogin)
        {
            if (adminLogin.AdminId != null)
            {
                if (adminLogin.Password != null)
                {
                    string password = encryptPassword.Encode(adminLogin.Password);

                    if (db.Admins.Any(b => b.AdminId.Equals(adminLogin.AdminId, StringComparison.InvariantCultureIgnoreCase) && b.Password.Equals(adminLogin.Password, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        FormsAuthentication.SetAuthCookie(adminLogin.AdminId, false);

                        Session["AdminId"] = adminLogin.AdminId;

                        return RedirectToAction("Index", new { adminLogin.AdminId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "User Name / Password is Incorrect");

                        return View();
                    }
                }

                else
                {
                    ModelState.AddModelError("", "Please enter Log In credentials");

                    return View();
                }
            }
            else
            {
                ModelState.AddModelError("", "Please enter Log In credentials");

                return View();
            }
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult NewLoans()
        {
            int stat = 0;
            var query = from newloans in db.Loans
                        where newloans.Status == 0 orderby newloans.LoanId descending
                        select newloans;

            ViewBag.stat = stat;

            return View(query);
        }

        [Authorize]
        public ActionResult DroppedLoans()
        {
            var query = from newloans in db.Loans
                        where newloans.Status == 6
                        select newloans;

            return View(query);
        }

        [Authorize]
        public ActionResult ApprovedLoans()
        {
            var query = from newloans in db.Loans
                        where newloans.Status == 5 
                        select newloans;

            return View(query);
        }

        [Authorize]
        public ActionResult GeneratePdf(int status) 
        {
            if (status == 0) 
            {
                return new ActionAsPdf("NewLoans");
            }

            else if(status == 6)
            {
                return new ActionAsPdf("DroppedLoans");
            }

            else
            {
                return new ActionAsPdf("ApprovedLoans");
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult TicketFetch()
        { 
            var query = from Userticket in db.Tickets
                        select Userticket;

            return View(query);
        }

        [Authorize]
        [HttpGet]
        public ActionResult TicketResolution(int requestId) 
        {
            ViewBag.RequestId = requestId;

            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult TicketResolution(Ticket ticket)
        {
            var query = (from Userticket in db.Tickets
                         where Userticket.RequestId == ticket.RequestId
                         select Userticket).Single();

            query.Resolution = string.Copy(ticket.Resolution);
            db.SaveChanges();

            return View("Index");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            Session.Abandon();

            return RedirectToAction("Login");
        }
    }
}
