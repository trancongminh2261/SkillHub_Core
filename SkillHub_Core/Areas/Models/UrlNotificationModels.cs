using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMSCore.Areas.Models
{
    public class UrlNotificationModels
    {
        /// <summary>
        /// https://monalms.monamedia.net
        /// </summary>
        public string url = "https://monalms.monamedia.net";
        /// <summary>
        /// /class/list-class/detail/?
        /// </summary>
        public string urlDetailClass = "/class/list-class/detail/?";
        /// <summary>
        /// /exam-result/?test=
        /// </summary>
        public string urlDetailIeltsExamResult = "/exam-result/?test=";
        /// <summary>
        /// /finance/payment/
        /// </summary>
        public string urlBill = "/finance/payment/";
        /// <summary>
        /// /entry-test/
        /// </summary>
        public string urlTestAppointment = "/entry-test/";
        /// <summary>
        /// /leads/
        /// </summary>
        public string urlLeads = "/leads/";
        /// <summary>
        /// /info-course/feedbacks/detail/?feedbackId=
        /// </summary>
        public string urlDetailFeedBack = "/info-course/feedbacks/detail/?feedbackId=";
        /// <summary>
        /// /users/personnel/
        /// </summary>
        public string urlEmployee = "/users/personnel/";
        /// <summary>
        /// /info-course/student/
        /// </summary>
        public string urlStudent = "/info-course/student/";
        /// <summary>
        /// /info-course/parents/
        /// </summary>
        public string urlParent = "/info-course/parents/";
        /// <summary>
        /// /info-course/student/detail/?StudentID=
        /// </summary>
        public string urlDetailStudent = "/info-course/student/detail/?StudentID=";
        /// <summary>
        /// /finance/verification/
        /// </summary>
        public string urlPayMentApprove = "/finance/verification/";
        /// <summary>
        /// /finance/cash-flow/print/?payment=
        /// </summary>
        public string urlPayMentSessionDetail = "/finance/cash-flow/print/?payment=";
        /// <summary>
        /// /info-course/feedbacks/
        /// </summary>
        public string urlFeedback = "/info-course/feedbacks/";
    }
}