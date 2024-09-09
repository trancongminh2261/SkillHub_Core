//using LMSCore.Services;
//using LMSCore.Users;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Web;
//using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

//namespace LMSCore.Areas.ControllerAPIs
//{
//    [Route("api/Test")]
//    [ClaimsAuthorize]
//    public class TestController : BaseController
//    {
//        [HttpGet]
//        [Route("")]
//        public string Index()
//        {
//            var data = SmsService.SendSms();
//            return data;

//        }
//        const string APIKey = "691474C8E980018C70EC04A34666F7";//Login to eSMS.vn to get this";//Dang ky tai khoan tai esms.vn de lay key//Register account at esms.vn to get key
//        const string SecretKey = "C6D7D1416FD906BB5AF561AD4E8DA5";//Login to eSMS.vn to get this";
//        //Send SMS with Sender is a number
//        public string SendJson(string phone, string message)
//        {
//            //Sample Request
//            //http://rest.esms.vn/SendMultipleMessage_V4_get?Phone={Phone}&Content={Content}&ApiKey={ApiKey}&SecretKey={SecretKey}&IsUnicode={IsUnicode}&Brandname={Brandname}&SmsType={SmsType}&Sandbox={Sandbox}&Priority={Priority}&RequestId={RequestId}&SendDate={SendDate}

//            // Create URL, method 1:
//            string URL = "http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?Phone=" + phone + "&Content=" + message + "&ApiKey=" + APIKey + "&SecretKey=" + SecretKey + "&IsUnicode=0&Brandname=Baotrixemay&SmsType=2";
//            //De dang ky brandname rieng vui long lien he hotline 0901.888.484 hoac nhan vien kinh Doanh cua ban
//            //-----------------------------------

//            //-----------------------------------
//            string result = SendGetRequest(URL);
//            JObject ojb = JObject.Parse(result);
//            int CodeResult = (int)ojb["CodeResult"];//100 is successfull

//            string SMSID = (string)ojb["SMSID"];//id of SMS
//            return result;
//        }

//        private string SendGetRequest(string RequestUrl)
//        {
//            Uri address = new Uri(RequestUrl);
//            HttpWebRequest request;
//            HttpWebResponse response = null;
//            StreamReader reader;
//            if (address == null) { throw new ArgumentNullException("address"); }
//            try
//            {
//                request = WebRequest.Create(address) as HttpWebRequest;
//                request.UserAgent = ".NET Sample";
//                request.KeepAlive = false;
//                request.Timeout = 15 * 1000;
//                response = request.GetResponse() as HttpWebResponse;
//                if (request.HaveResponse == true && response != null)
//                {
//                    reader = new StreamReader(response.GetResponseStream());
//                    string result = reader.ReadToEnd();
//                    result = result.Replace("</string>", "");
//                    return result;
//                }
//            }
//            catch (WebException wex)
//            {
//                if (wex.Response != null)
//                {
//                    using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
//                    {
//                        Console.WriteLine(
//                            "The server returned '{0}' with the status code {1} ({2:d}).",
//                            errorResponse.StatusDescription, errorResponse.StatusCode,
//                            errorResponse.StatusCode);
//                    }
//                }
//            }
//            finally
//            {
//                if (response != null) { response.Close(); }
//            }
//            return null;
//        }

//        //Send SMS with Alpha Sender
//        public string SendBrandnameJson(string phone, string message, string brandname)
//        {
//            //http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?Phone={Phone}&Content={Content}&BrandnameCode={BrandnameCode}&ApiKey={ApiKey}&SecretKey={SecretKey}&SmsType={SmsType}&SendDate={SendDate}&SandBox={SandBox}
//            //url vi du
//            // sử dụng cách 1:
//            brandname = "Baotrixemay";
//            //De dang ky brandname rieng vui long lien he hotline 0901.888.484 hoac nhan vien kinh Doanh cua ban
//            string URL = "http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?Phone=" + phone + "&Content=" + message + "&Brandname=" + brandname + "&ApiKey=" + APIKey + "&SecretKey=" + SecretKey + "&IsUnicode=0&SmsType=2";

//            string result = SendGetRequest(URL);
//            JObject ojb = JObject.Parse(result);
//            int CodeResult = (int)ojb["CodeResult"];//trả về 100 là thành công
//            int CountRegenerate = (int)ojb["CountRegenerate"];
//            string SMSID = (string)ojb["SMSID"];//id của tin nhắn
//            return result ;
//        }

//        //Get Account Balance - Lay so du tai khoan
//        public string GetBalance()
//        {
//            string data = "http://rest.esms.vn/MainService.svc/json/GetBalance/" + APIKey + "/" + SecretKey + "";
//            string result = SendGetRequest(data);
//            JObject ojb = JObject.Parse(result);
//            int CodeResult = (int)ojb["CodeResponse"];//trả về 100 là thành công
//            int UserID = (int)ojb["UserID"];//id tài khoản
//            long Balance = (long)ojb["Balance"];//tiền trong tài khoản
//            return "cc";
//        }

//    }
//}