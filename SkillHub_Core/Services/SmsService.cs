//using LMSCore.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities; 
//using Twilio;
//using Twilio.Rest.Api.V2010.Account;

//namespace LMSCore.Services
//{
//    public class SmsService : DomainService
//    {
//        public SmsService(lmsDbContext dbContext) : base(dbContext)
//        {
//        }
//        public static string SendSms(){
//            string accountSid = "AC256d8d1570156d31e433540a5c5daae0";
//            string authToken = "c0cc25be39c56c11626781c8399f3457";

//            TwilioClient.Init(accountSid, authToken);

//            var message = MessageResource.Create(
//                body: "Test sms",
//                from: new Twilio.Types.PhoneNumber("+84373019800"),
//                to: new Twilio.Types.PhoneNumber("+84373019800")
//            );
//            return "hallo";
//        }
//    }
//}
