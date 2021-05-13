using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Windows;
using LendingTree.Models;

namespace LendingTree.Controllers
{
    public class AgentController : Controller
    {
        readonly private LendingContext db = new LendingContext();
        readonly private EncryptPassword encryptPassword = new EncryptPassword();

        [Authorize]
        public ActionResult Account(Agent agent)
        {
            ViewBag.AgentId = agent.AgentId;

            if (agent.DepartmentId == 1)
            {
                return View("ApprovalAgency");
            }
            else if (agent.DepartmentId == 2)
            {
                return View("PickUp");
            }
            else if (agent.DepartmentId == 3)
            {
                return View("VerificationAgency");
            }
            else if (agent.DepartmentId == 4)
            {
                return View("LegalDepartment");
            }
            else
            {
                return View("Admin");
            }
        }

        [Authorize]
        public ActionResult Admin(int status)
        {
            if (status == 4)
            {
                var query1 = from loan in db.Loans
                             where loan.Status == status
                             select loan.LoanId;

                return View("AdminSanction", query1);
            }

            if (status == 6)
            {
                var query1 = from loan in db.Loans
                             where loan.Status == status
                             select loan.LoanId;

                ViewBag.Flag = 0;
                ViewBag.Query = query1;

                return View("AdminDroppedInfo");
            }

            var query = from loan in db.Loans
                        where loan.Status == status
                        select loan;

            //ViewBag.query = query;
            ViewBag.status = status;
            ViewBag.flag = 0;

            return View("AdminInfo", query);
        }

        public ActionResult AdminAgentInfo(int loanId, int deptId)
        {
            var query = from agent in db.Agents
                        where agent.DepartmentId == deptId && agent.NoOfApplications < 3
                        select agent;

            ViewBag.flag = 1;
            ViewBag.LoanId = loanId;
            ViewBag.DeptId = deptId;

            return View("AdminInfo", query);
        }

        public ActionResult AdminMapping(int loanId, string agentId, int deptId)
        {
            var queryLoan = (from loan in db.Loans
                             where loan.LoanId == loanId
                             select loan).Single();

            var queryAgent = (from agent in db.Agents
                              where agent.AgentId == agentId
                              select agent).Single();

            queryAgent.ConfirmPassword = queryAgent.Password;
            queryAgent.NoOfApplications++;

            if (deptId == 2)
            {
                queryLoan.PickupAgent = new Agent();
                queryLoan.PickupAgent = queryAgent;
            }
            else if (deptId == 3)
            {
                queryLoan.VerificationAgent = new Agent();
                queryLoan.VerificationAgent = queryAgent;
            }
            else if (deptId == 4)
            {
                queryLoan.LegalAgent = new Agent();
                queryLoan.LegalAgent = queryAgent;
            }

            try
            {
                db.SaveChanges(); 

                System.Windows.Forms.MessageBox.Show("Agent Mapped");

                return View("Admin");
            }
            catch (DbEntityValidationException e)
            {
                Console.WriteLine(e);

                return View("Admin");
            }
        }

        [HttpPost]
        public ActionResult AdminDroppedInfo(Loan loan, FormCollection fc)
        {
            int loanId = int.Parse(fc["LoanId"]);

            var query = (from loans in db.Loans
                        where loans.LoanId == loanId
                        select loans).Single();

            query.Remarks = String.Copy(loan.Remarks);

            db.SaveChanges();

            return View("Admin", new { status = 6 });
        }

        [Authorize]
        public ActionResult ApprovalAgency(string agentId)
        {
            var query = from loan in db.Loans
                    where loan.Status == 0
                    select loan.LoanId;

            ViewBag.AgentId = agentId;

            return View(query);
        }

        [Authorize]
        public ActionResult PickUp(string agentId)
        {
            var query = from loan in db.Loans
                        where loan.Status == 1 && loan.PickupAgent.AgentId == agentId
                        select loan.LoanId;

            ViewBag.AgentId = agentId;

            return View(query);
        }

        [Authorize]
        public ActionResult VerificationAgency(string agentId)
        {
            var query = from loan in db.Loans
                        where loan.Status == 2 && loan.VerificationAgent.AgentId == agentId
                        select loan.LoanId;

            ViewBag.AgentId = agentId;

            return View(query);
        }

        [Authorize]
        public ActionResult LegalDepartment (string agentId)
        {
            var query = from loan in db.Loans
                        where loan.Status == 3 && loan.LegalAgent.AgentId == agentId
                        select loan.LoanId;

            ViewBag.AgentId = agentId;

            return View(query);
        }

        public ActionResult LoanInfo(int loanId, int status, string agentId)
        {
            var query = (from loans in db.Loans
                         where loans.LoanId == loanId
                         orderby loans.LoanId
                         select loans).Single();

            ViewBag.AgentId = agentId;

            if (status == 0)
            {
                return View("ApprovalAgencyInfo", query);
            }
            else if (status == 1)
            {
                return View("PickUpInfo", query);
            }
            else if (status == 2)
            {
                return View("VerificationAgencyInfo", query);
            }
            else if (status == 3)
            {
                return View("LegalDepartmentInfo", query);
            }
            else if (status == 4)
            {
                return View("AdminSanctionInfo", query);
            }
            else
            {
                ViewBag.Flag = 1;
                ViewBag.Query = query;

                return View("AdminDroppedInfo");
            }
        }

        public ActionResult Approved(int loanId, int status, string agentId)
        {
            var queryLoan = (from loan in db.Loans
                         where loan.LoanId == loanId
                         select loan).Single();

            if(status < 4 && status > 0)
            {
                var queryAgent = (from agent in db.Agents
                                  where agent.AgentId.Equals(agentId)
                                  select agent).Single();

                queryAgent.ConfirmPassword = queryAgent.Password;
                queryAgent.NoOfApplications--;
            }
            

            if (status == 0)
            {
                queryLoan.Status = 1;
                queryLoan.Approved = true;
                db.SaveChanges();

                return RedirectToAction("ApprovalAgency", new { agentId });
            }
            else if (status == 1)
            {
                queryLoan.Status = 2;
                queryLoan.VerifiedByPickUpAgent = true;
                db.SaveChanges();

                return RedirectToAction("PickUp", new { agentId });
            }
            else if (status == 2)
            {
                queryLoan.Status = 3;
                queryLoan.VerifiedByVerificationAgency = true;
                db.SaveChanges();

                return RedirectToAction("VerificationAgency", new { agentId });
            }
            else if (status == 3)
            {
                queryLoan.Status = 4;
                queryLoan.VerifiedByLegalAgent = true;
                db.SaveChanges();

                return RedirectToAction("LegalDepartment", new { agentId });
            }
            else
            {
                queryLoan.Status = 5;
                queryLoan.Sanctioned = true;
                db.SaveChanges();

                return RedirectToAction("Admin", new { status });
            }
        }

        public ActionResult Rejected(int loanId, int status, string agentId)
        {
            var queryLoan = (from loan in db.Loans
                         where loan.LoanId == loanId
                         select loan).Single();

            if(status < 4)
            {
                var queryAgent = (from agent in db.Agents
                                  where agent.AgentId == agentId
                                  select agent).Single();

                queryAgent.ConfirmPassword = queryAgent.Password;
                queryAgent.NoOfApplications--;
            }

            queryLoan.Status = 6;
            db.SaveChanges();

            if (status == 0)
            {
                return RedirectToAction("ApprovalAgency");
            }
            else if(status == 1)
            {
                return RedirectToAction("PickUp");
            }
            else if (status == 2)
            {
                return RedirectToAction("VerificationAgency");
            }
            else if (status == 3)
            {
                return RedirectToAction("LegalDepartment");
            }
            else 
            {
                return RedirectToAction("Admin", new { status });
            }
        }

        public ActionResult Notification()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FirstName, LastName, DoB, Gender, ContactNumber, DepartmentId, AgentId, Password, ConfirmPassword")] Agent agent)
        {
            if (ModelState.IsValid)
            {
                if (!db.Agents.Any(x => x.AgentId == agent.AgentId))
                { 
                    var confirmpasskey = encryptPassword.Encode(agent.ConfirmPassword);
                    agent.ConfirmPassword = confirmpasskey;

                    var passkey = encryptPassword.Encode(agent.Password);
                    agent.Password = passkey;

                    db.Agents.Add(agent);

                    try
                    {
                        db.SaveChanges();

                        System.Windows.Forms.MessageBox.Show("New Agent Created Successfully");

                        return View(agent);
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message = e.Message;

                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Agent ID already exists");

                    return View(agent);
                }
            }

            return View(agent);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "AgentId, Password")] Agent agent)
        {
            if (agent.AgentId != null)
            {
                if (agent.Password != null)
                {
                    string password = encryptPassword.Encode(agent.Password);

                    if (db.Agents.Any(b => b.AgentId.Equals(agent.AgentId, StringComparison.InvariantCultureIgnoreCase) && b.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        FormsAuthentication.SetAuthCookie(agent.AgentId, false);


                        Session["AgentId"] = agent.AgentId;


                        IEnumerable<Agent> agent1 = from agentTemp in db.Agents
                                                   where agentTemp.AgentId == agent.AgentId
                                                   select agentTemp;
                  
                        return RedirectToAction("Account", agent1.First());
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

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();

            Session.Abandon();

            return RedirectToAction("Login", "Agent");
        }
    }
}
