using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_LeadSwitchTest : DomainEntity
    {
        public int SaleId { get; set; }
        public int StudentId { get; set; }
        public int TestAppointmentId { get; set; }
        [NotMapped]
        public string SaleName { get; set; }
        [NotMapped]
        public string SaleCode { get; set; }
        [NotMapped]
        public string StudentName { get; set; }
        [NotMapped]
        public string StudentCode { get; set; }
        public tbl_LeadSwitchTest() : base() { }
        public tbl_LeadSwitchTest(object model) : base(model) { }
    }
    public class Get_LeadSwitchTest : DomainEntity
    {
        public int SaleId { get; set; }
        public int StudentId { get; set; }
        public int TestAppointmentId { get; set; }
        public string SaleName { get; set; }
        public string SaleCode { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public int TotalRow { get; set; }
    }
}