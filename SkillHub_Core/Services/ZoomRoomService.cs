//using LMSCore.Areas.Models;
//using LMSCore.Areas.Request;
//using LMSCore.LMS;
//using LMSCore.Models;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using Microsoft.EntityFrameworkCore;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using static LMSCore.Models.lmsEnum;
//using Microsoft.IdentityModel.Tokens;
//using System.Security.Cryptography;
//
//using Newtonsoft.Json.Linq;

//namespace LMSCore.Services
//{
//    public class ZoomRoomService
//    {

//        static readonly char[] padding = { '=' };
//        ///Tạo phòng zoom
//        public static async Task<AppDomainResult> CreateRoom(int seminarId, tbl_UserInformation user)
//        {
//            using (var db = new lmsDbContext())
//            {
//                try
//                {
//                    var seminar = await db.tbl_Seminar.SingleOrDefaultAsync(x => x.Id == seminarId);
//                    if (seminar == null)
//                        return new AppDomainResult { Success = false, ResultMessage = "Không tìm thấy buổi hội thảo" };
//                    if (user.RoleId != ((int)RoleEnum.admin) && user.UserInformationId != seminar.LeaderId)
//                        return new AppDomainResult { Success = false, ResultMessage = "Bạn không có quyền tạo phòng" };
//                    var checkRoom = await db.tbl_ZoomRoom.Where(x => x.SeminarId == seminar.Id && x.Enable == true).FirstOrDefaultAsync();
//                    if (checkRoom != null)
//                    {
//                        checkRoom.Enable = false;
//                        await db.SaveChangesAsync();
//                    }
//                    var zoomConfig = db.tbl_ZoomConfig
//                        .Where(x => x.Active == false && x.Enable == true)
//                        .FirstOrDefault();
//                    if (zoomConfig == null)
//                        return new AppDomainResult { Success = false, ResultMessage = "Tất cả tài khoản zoom đang hoạt động" };
//                    ZoomModel zoomModel = new ZoomModel
//                    {
//                        Status = true,
//                        RoomId = "",
//                        RoomPass = "",
//                        ApiKey = zoomConfig.APIKey,
//                        SignatureTeacher = "",
//                        SignatureStudent = "",
//                        UserName = user.FullName,
//                    };
//                    string tokenString = AccessToken(zoomConfig.APISecret, zoomConfig.APIKey);
//                    var client = new RestClient($"https://api.zoom.us/v2/users/{zoomConfig.UserZoom}/meetings");
//                    var request = new RestRequest(Method.POST);
//                    request.RequestFormat = DataFormat.Json;
//                    request.AddJsonBody(new { topic = seminar.Name, type = "1" });

//                    request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
//                    IRestResponse restResponse = client.Execute(request);
//                    HttpStatusCode statusCode = restResponse.StatusCode;
//                    int numericStatusCode = (int)statusCode;
//                    var jObject = JObject.Parse(restResponse.Content);

//                    if (numericStatusCode == 201)
//                    {
//                        if (string.IsNullOrEmpty(jObject["id"].ToString()) || string.IsNullOrEmpty(jObject["encrypted_password"].ToString()))
//                            return new AppDomainResult { Success = false, ResultMessage = "Tạo phòng không thành công" };
//                        else
//                        {
//                            zoomModel.RoomId = jObject["id"].ToString();
//                            zoomModel.RoomPass = jObject["encrypted_password"].ToString();
//                        }
//                    }
//                    else
//                        return new AppDomainResult { Success = false, ResultMessage = "Tạo phòng không thành công" };
//                    String ts = (ToTimestamp(DateTime.UtcNow.ToUniversalTime()) - 30000).ToString();
//                    zoomModel.SignatureTeacher = GenerateToken(zoomConfig.APIKey, zoomConfig.APISecret, zoomModel.RoomId, ts, "1");
//                    zoomModel.SignatureStudent = GenerateToken(zoomConfig.APIKey, zoomConfig.APISecret, zoomModel.RoomId, ts, "0");
//                    ///Thêm vào danh sách phòng học
//                    db.tbl_ZoomRoom.Add(new tbl_ZoomRoom
//                    {
//                        CreatedBy = user.FullName,
//                        CreatedOn = DateTime.Now,
//                        Enable = true,
//                        LeaderId = user.UserInformationId,
//                        ModifiedBy = user.FullName,
//                        ModifiedOn = DateTime.Now,
//                        RoomId = zoomModel.RoomId,
//                        RoomPass = zoomModel.RoomPass,
//                        SeminarId = seminar.Id,
//                        ZoomConfigId = zoomConfig.Id,
//                        SignatureStudent = zoomModel.SignatureStudent,
//                        SignatureTeacher = zoomModel.SignatureTeacher
//                    });
//                    zoomConfig.Active = true;
//                    seminar.Status = 2;
//                    seminar.StatusName = "Đang diễn ra";
//                    await db.SaveChangesAsync();
//                    return new AppDomainResult { Success = true, Data = zoomModel } ;
//                }
//                catch (Exception e)
//                {
//                    throw e;
//                }
//            }

//        }
//        ///Tắt phòng 
//        public static async Task CloseRoom(int seminarId, tbl_UserInformation user)
//        {
//            using (var db = new lmsDbContext())
//            {
//                try
//                {
//                    var seminar = await db.tbl_Seminar.SingleOrDefaultAsync(x => x.Id == seminarId);
//                    if (seminar == null)
//                        throw new Exception("Không tìm thấy buổi hội thảo");
//                    if (seminar.Status == 3)
//                        throw new Exception("Buổi hội thảo đã kết thúc");
//                    if (user.RoleId != ((int)RoleEnum.admin) && user.UserInformationId != seminar.LeaderId)
//                        throw new Exception("Bạn không có quyền tắt phòng");
//                    var zoomRoom = await db.tbl_ZoomRoom
//                        .Where(x => x.SeminarId == seminar.Id && x.Enable ==true).FirstOrDefaultAsync();
//                    if (zoomRoom != null)
//                    {
//                        var zoomConfig = await db.tbl_ZoomConfig.SingleOrDefaultAsync(x => x.Id == zoomRoom.ZoomConfigId);
//                        if (zoomConfig != null)
//                        {
//                            zoomConfig.Active = false;
//                            string tokenString = AccessToken(zoomConfig.APISecret, zoomConfig.APIKey);
//                            var client = new RestClient($"https://api.zoom.us/v2/meetings/{zoomRoom.RoomId}/status");
//                            var request = new RestRequest(Method.PUT);
//                            request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
//                            request.AddParameter("application/json", "{\"action\":\"end\"}", ParameterType.RequestBody);
//                            IRestResponse response = client.Execute(request);
//                            HttpStatusCode statusCode = response.StatusCode;
//                            int numericStatusCode = (int)statusCode;
//                        }
//                    }
//                    seminar.Status = 3; 
//                    seminar.StatusName = "Kết thúc";
//                    await db.SaveChangesAsync();
//                }
//                catch (Exception e)
//                {
//                    throw e;
//                }
//            }
//        }
//        public static async Task<AppDomainResult> GetActive(SearchOptions search)
//        {
//            using (var db = new lmsDbContext())
//            {
//                if (search == null) return new AppDomainResult { TotalRow = 0, Data = null };
//                string sql = $"Get_ZoomActive @PageIndex = {search.PageIndex}," +
//                    $"@PageSize = {search.PageSize}";
//                var data = await db.SqlQuery<Get_ZoomActive>(sql);
//                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
//                var totalRow = data[0].TotalRow;
//                var result = data.Select(i => new tbl_ZoomRoom(i)).ToList();
//                return new AppDomainResult { TotalRow = totalRow, Data = result };
//            }
//        }
//        /// <summary>
//        /// 30p sau khi hết giờ hệ thống Tự động tắt phòng zoom nếu chưa tắt
//        /// </summary>
//        /// <returns></returns>
//        public static async Task AutoCloseRoom()
//        {
//            using (var db = new lmsDbContext())
//            {
//                DateTime time = DateTime.Now.AddMinutes(-15);
//                var seminars = await db.tbl_Seminar
//                    .Where(x => x.Enable == true && x.Status != 3 && time > x.EndTime).ToListAsync();
//                if (seminars.Any())
//                {
//                    foreach (var item in seminars)
//                        await CloseRoom(item.Id, new tbl_UserInformation { RoleId = 1, FullName = "Tự động" });
//                }
//            }
//        }
//        /// <summary>
//        /// Lấy bảng ghi buổi họp
//        /// </summary>
//        /// <param name="courseScheduleID"></param>
//        /// <returns></returns>
//        public static async Task<List<RecordingFiles>> GetRecord(int seminarId)
//        {
//            using (var db = new lmsDbContext())
//            {
//                var seminar = await db.tbl_Seminar.SingleOrDefaultAsync(x => x.Id == seminarId);
//                if (seminar == null)
//                    throw new Exception("Không tìm thấy buổi hội thảo");
//                var zoomRoom = await db.tbl_ZoomRoom.Where(x => x.SeminarId == seminar.Id).FirstOrDefaultAsync();
//                if (zoomRoom == null)
//                    return new List<RecordingFiles>();
//                var zoomConfig = db.tbl_ZoomConfig.SingleOrDefault(x => x.Id == zoomRoom.ZoomConfigId);
//                if (zoomConfig == null)
//                    return new List<RecordingFiles>();

//                List<RecordingFiles> rFile = new List<RecordingFiles>();
//                try
//                {
//                    string tokenString = "";
//                    tokenString = AccessToken(zoomConfig.APISecret, zoomConfig.APIKey);
//                    var client = new RestClient($"https://api.zoom.us/v2/meetings/{zoomRoom.RoomId}/recordings");
//                    var request = new RestRequest(Method.GET);
//                    request.AddHeader("authorization", String.Format("Bearer {0}", tokenString));
//                    IRestResponse response = client.Execute(request);
//                    HttpStatusCode statusCode = response.StatusCode;
//                    int numericStatusCode = (int)statusCode;
//                    //chỗ này hứng data nè
//                    if (numericStatusCode == 200)
//                    {
//                        var jObject = JObject.Parse(response.Content);
//                        if (jObject != null && jObject["recording_files"] != null)
//                        {
//                            rFile = JsonConvert.DeserializeObject<List<RecordingFiles>>(jObject["recording_files"].ToString());
//                        }
//                    }
//                }
//                catch { }

//                if (!rFile.Any())
//                    return new List<RecordingFiles>();
//                else
//                    return rFile;
//            }
//        }
//        public static long ToTimestamp(DateTime value)
//        {
//            long epoch = (value.Ticks - 621355968000000000) / 10000;
//            return epoch;
//        }
//        public static string AccessToken(string apiSecret, string apiKey)
//        {
//            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
//            var now = DateTime.UtcNow;
//            byte[] symmetricKey = Encoding.ASCII.GetBytes(apiSecret);

//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Issuer = apiKey,
//                Expires = now.AddSeconds(300),
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256),
//            };

//            var token = tokenHandler.CreateToken(tokenDescriptor);
//            var tokenString = tokenHandler.WriteToken(token);
//            return tokenString;
//        }
//        public static string GenerateToken(string apiKey, string apiSecret, string meetingNumber, string ts, string role)
//        {
//            string message = String.Format("{0}{1}{2}{3}", apiKey, meetingNumber, ts, role);
//            apiSecret = apiSecret ?? "";
//            var encoding = new System.Text.ASCIIEncoding();
//            byte[] keyByte = encoding.GetBytes(apiSecret);
//            byte[] messageBytesTest = encoding.GetBytes(message);
//            string msgHashPreHmac = System.Convert.ToBase64String(messageBytesTest);
//            byte[] messageBytes = encoding.GetBytes(msgHashPreHmac);
//            using (var hmacsha256 = new HMACSHA256(keyByte))
//            {
//                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
//                string msgHash = System.Convert.ToBase64String(hashmessage);
//                string token = String.Format("{0}.{1}.{2}.{3}.{4}", apiKey, meetingNumber, ts, role, msgHash);
//                var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
//                return System.Convert.ToBase64String(tokenBytes).TrimEnd(padding);
//            }
//        }
//    }
//}