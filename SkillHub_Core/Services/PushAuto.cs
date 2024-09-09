using Hangfire;
using LMSCore.Models;
using LMSCore.Services.Class;
using LMSCore.Services.ClassReserve;
using LMSCore.Services.Customer;
using LMSCore.Services.Discount;
using LMSCore.Services.MonthlyTuition;
using LMSCore.Services.PaymentSession;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LMSCore.Services
{
    public class PushAuto
    {
        public static async Task Minutely()
        {
            await ClassNotification.NotifyTeacherAlmostTimeForClass();
            await ClassNotification.NotifyStudentAlmostTimeForClass();
            await CustomerCronjob.AutoUpdateCustomerStatus();
            await ComboService.AutoUpdateComboStatus();
        }
        public static async Task Daily()
        {
            await PaymentSessionNotification.NotifyStudentPaymentDue();
            await PaymentSessionNotification.NotifyParentsPaymentDue();
            await ClassCronjob.UpdateStatus();
            await ClassReserveCronjob.UpdateStatus();
            await DiscountCronjob.Expired();
            await MonthlyTuitionCronjob.CreateTuition();
            await ClassNotification.NotifyStudentLearningOutcomes();
        }
        public static async Task Monthly()
        {
        }
    }
}