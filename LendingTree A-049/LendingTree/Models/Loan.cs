using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingTree.Models
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [Required]
        [Display(Name = "Loan Amount")]
        public double LoanAmount { get; set; }

        [Display(Name = "Due Time (Years)")]
        public int DueTime { get; set; }

        [Range(0, 6)]
        //[System.ComponentModel.DefaultValue(0)]
        public int Status { get; set; }

        [Required]
        [Display(Name = "Salary Income")]
        public double Income { get; set; }

        [Required]
        [Display(Name = "Loan type")]
        public string LoanType { get; set; }

        /*[Display(Name = "User Request Id")]
        public int SerialNo { get; set; }*/
        [Required]
        [Display(Name = "PAN No.")]
        [RegularExpression("^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN Number")]
        public string PANNo { get; set; }

        [Display(Name = "Adhaar No.")]
        [RegularExpression("^[2-9]{1}[0-9]{11}$", ErrorMessage = "Invalid Adhaar Number")]
        public long AadharNo { get; set; }

        [Display(Name = "Bank Account No.")]
        [RegularExpression("^[1-9]{1}[0-9]{10}$", ErrorMessage = "Invalid Bank Account Number")]
        public long BankAccountNo { get; set; }

        public bool Approved { get; set; }

        public bool VerifiedByVerificationAgency { get; set; }

        public bool VerifiedByPickUpAgent { get; set; }

        public bool VerifiedByLegalAgent { get; set; }

        public bool Sanctioned { get; set; }

        public string Remarks { get; set; }

        public string FK_VerificationAgent { get; set; }

        public string FK_ApprovalAgencyAgent { get; set; }

        public string FK_PickupAgent { get; set; }

        public string FK_LegalAgent { get; set; }

        public string FK_User { get; set; }

        //Agent Foreign Keys
        [ForeignKey("FK_VerificationAgent")]
        public Agent VerificationAgent { get; set; }

        [ForeignKey("FK_ApprovalAgencyAgent")]
        public Agent ApprovalAgencyAgent { get; set; }

        [ForeignKey("FK_PickupAgent")]
        public Agent PickupAgent { get; set; }

        [ForeignKey("FK_LegalAgent")]
        public Agent LegalAgent { get; set; }

        //User Foreign Key
        [ForeignKey("FK_User")]
        public User User { get; set; }
    }
}