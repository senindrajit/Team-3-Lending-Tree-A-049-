using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using System.Windows;
using System.Windows.Forms;
using LendingTree.Models;

namespace LendingTree.Controllers
{
    public class UserController : Controller
    {
        readonly private LendingContext db = new LendingContext();

        readonly private EncryptPassword encryptPassword = new EncryptPassword();

        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FirstName, LastName, DoB, Gender, ContactNumber, Email, UserId, Password, ConfirmPassword, Answer1, Answer2, Answer3")] User user)
        {
            if (ModelState.IsValid)
            {
                if (!db.Users.Any(x => x.UserId == user.UserId))
                {
                    var confrmpasskey = encryptPassword.Encode(user.ConfirmPassword);
                    user.ConfirmPassword = confrmpasskey;

                    var passkey = encryptPassword.Encode(user.Password);
                    user.Password = passkey;

                    db.Users.Add(user);

                    try
                    {
                        db.SaveChanges();

                        MessageBox.Show("New User Created Successfully");

                        return View(user);
                        //return RedirectToAction("UserHome", "Home");
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message = e.Message;

                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User ID already exists");

                    return View(user);
                }
            }

            return View(user);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "UserId, Password")] User user)
        {
            if (user.UserId != null)
            {
                if (user.Password != null)
                {
                    string password = encryptPassword.Encode(user.Password);

                    if (db.Users.Any(b => b.UserId.Equals(user.UserId, StringComparison.InvariantCultureIgnoreCase) && b.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        FormsAuthentication.SetAuthCookie(user.UserId, false);

                        Session["UserId"] = user.UserId;

                        return RedirectToAction("Account", new { user.UserId });
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
        public ActionResult Account(string userId)
        {
            var entity = db.Users.Find(userId);
            ViewBag.Gender = entity.Gender;
            ViewBag.Message = entity.FirstName + " " +  entity.LastName;

            ViewBag.UserId = userId;

            return View();
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FirstName,LastName,DoB,Gender,ContactNumber,Email,UserId,Password,ConfirmPassword,Answer1, Answer2,Answer3")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(user);
        }

        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult ForgotUserId()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotUserId(ForgotUserId ob)
        {
            string message = "";
            string Status = "false";
            if (ModelState.IsValid)
            {
                var data = db.Users.FirstOrDefault(x => x.ContactNumber == ob.ContactNumber);
                if (data != null)
                {
                    if (string.Compare(ob.Answer1, data.Answer1) == 0 && string.Compare(ob.Answer2, data.Answer2) == 0 && string.Compare(ob.Answer3, data.Answer3) == 0)
                    {
                        Status = "true";
                        message = $"User ID is {data.UserId} ";
                    }
                    else
                    {
                        message = "Wrong Answers to the Questions";
                    }
                }
                else
                {
                    message = "Wrong Contact Number";
                }
            }
            ViewBag.Status = Status;
            ViewBag.Message = message;

            return View(ob);
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPassword ob)
        {
            string message = "";

            if (ModelState.IsValid)
            {

                var data = db.Users.FirstOrDefault(x => x.UserId == ob.UserId);

                if (data != null)
                {
                    if (string.Compare(ob.Answer1, data.Answer1) == 0 && string.Compare(ob.Answer2, data.Answer2) == 0 && string.Compare(ob.Answer3, data.Answer3) == 0)
                    {
                        return RedirectToAction("ResetPassword", new { UserId = data.UserId });
                    }
                    else
                    {
                        message = "Wrong Answers to the Questions";
                    }
                }
                else
                {
                    message = "User ID does not Exist";
                }
            }
            ViewBag.Message = message;

            return View(ob);
        }

        [HttpGet]
        public ActionResult ResetPassword(string UserId)
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string UserId, ResetPassword ob)
        {
            string message = "";
            if (ModelState.IsValid)
            {
                var data = db.Users.Find(UserId);
                data.Password = encryptPassword.Encode(ob.NewPassword);
                data.ConfirmPassword = encryptPassword.Encode(ob.ConfirmPassword);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();

                message = "Password Reset Sucessfull";
            }
            ViewBag.Message = message;

            return View(ob);
        }

        [Authorize]
        [HttpGet]
        public ActionResult ApplyLoan(string userId)
        {
            ViewBag.UserId = userId;

            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ApplyLoan(Loan loan, System.Web.Mvc.FormCollection fc)
        {

            string userId = fc["UserId"];

            if (ModelState.IsValid)
            {
                var queryUser = (from user in db.Users
                                where user.UserId == userId
                                 select user).Single();

                loan.User = queryUser;

                db.Loans.Add(loan);

                var col1 = db.Loans.Where(w => w.PANNo.Equals(loan.PANNo));

                try
                {
                    db.SaveChanges();

                    MessageBox.Show("Loan Request Submitted Successfully");

                    return View(loan);
                    //return RedirectToAction("UserHome", "Home");
                }
                catch (Exception e)
                {
                    ViewBag.Message = e.Message;

                    return View("Error");
                }
            }

            return View(loan);
        }

        [Authorize]
        [HttpGet]
        public ActionResult ApplyTicket(string userId)
        {
            ViewBag.UserId = userId;

            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ApplyTicket(Ticket ticket, System.Web.Mvc.FormCollection fc)
        {
            string userId = fc["UserId"];

            var queryUser = (from user in db.Users
                                where user.UserId == userId
                                select user).Single();

            ticket.User = queryUser;
            ticket.DateTicket = DateTime.Now;

            db.Tickets.Add(ticket);

            try
            {
                db.SaveChanges();

                MessageBox.Show("Ticket Submitted Successfully");

                return RedirectToAction("Account", new { userId });
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;

                return View("Error");
            }
        }

        [Authorize]
        public ActionResult CheckStatus(string userId)
        {
            if (db.Loans.Any(l => l.User.UserId == userId))
            {
                var query = from loan in db.Loans
                             where loan.User.UserId == userId
                             select loan;

                return View(query);
            }
            else
            {
                ViewBag.Message = "You don't have any applied loans.";

                return View();
            }
        }

        [Authorize]
        public ActionResult CheckTicket(string userId)
        {
            if (db.Tickets.Any(l => l.User.UserId == userId))
            {
                var query = from ticket in db.Tickets
                            where ticket.User.UserId == userId
                            select ticket;

                return View(query);
            }
            else
            {
                ViewBag.Message = "You don't have any submitted tickets.";

                return View();
            }
        }
    }
}