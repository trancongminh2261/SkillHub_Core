//using LMSCore.Models;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
//using ZaloDotNetSDK;

//namespace LMSCore.Services
//{
//    public class ZaloZnsService : DomainService
//    {
//        public ZaloZnsService(lmsDbContext dbContext) : base(dbContext)
//        {
//        }
//        public static string TestZNS( )
//        {
//            long appId= 2495867655461509605;
//            string secretKey = "8L7cqD4TM44IeNA2XpXj";
//            //string accessToken = "";
//            // Khởi tạo ZaloClient
//            ZaloClient client = new ZaloClient("dEbi8oVRRsoIwc4W9DjhR9RbPYKXp2Gxn9fK4XEiFXhvwNC68vOIEUxp41CsbamQyjPQ4oE82Z_Mp0W46lz_Jl2UDqy3qrHEpOKjJWxRUbRhwnjVERr3TetICcD2bqigXg8L05FkJn6mlZ84GlDdEeMCCI0sttmZxh828nZ4RMdnaoLM1ljyUVQLSrP7m2neig1dH4B8C4IYYJXxPDT6Ij3fDrKLW7vdhi4JNrhnMn6DfYfTVizoRhQOVd00wWPeuPTkQ1hbF23-r4um7eaK4UdjIY0cY4aDmxaxGtM21pjvAi1bRW");
//            //Lấy danh sách người quan tâm
//            JObject result = client.getListFollower(0, 20);
//            //Lấy thông tin người quan tâm
//            //string userId = "6782084084065628891";
//            //JObject userProfile = client.getProfileOfFollower(userId); 
//            //// Gửi tin nhắn cho user id
//            //string yourMessage = "this is a message";
//            //JObject resultSendMessage = client.sendTextMessageToUserId(userId, yourMessage);
             
//            return result.ToString();
//        }
//    }
//}