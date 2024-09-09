using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMSCore.Areas.Models
{
    public static class VnPayModel
    {
        public static string TmnCode = "395X2X7L";
        public static string HashSecret = "UBACRHDINDGLMOSBMSNLNQVZIYFPSQIO";
        public static string BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public static string Command = "pay";
        public static string CurrCode = "VND";
        public static string Version = "2.1.0";
        public static string Locale = "vn";
        public static string ReturnUrl = "https://mail.google.com/mail/u/0/#inbox";
        public static TimeZoneInfo TimeZoneId = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // If do not us Windown OS change it to: Asia/Bangkok 
    } 
} 