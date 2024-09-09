//using LMSCore.Areas.Models.ZnsModel;
//using LMSCore.Areas.Models.ZnsModel.ZnsTemplate;
//using LMSCore.Areas.Request;
//using LMSCore.Models;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using Microsoft.EntityFrameworkCore;
//using System.Linq;
//using System.Net.Http;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using static LMSCore.Models.lmsEnum;

//namespace LMSCore.Services
//{
//    public class ZaloNotificationService : DomainService
//    {
//        public static readonly object lockObject = new object();
//        public ZaloNotificationService(lmsDbContext dbContext) : base(dbContext)
//        {
//        }
//        private ZnsAccessTokenResponse GetAccessToken()
//        {
//            ZnsAccessTokenResponse znsAccessTokenResponse = new ZnsAccessTokenResponse();
//            tbl_ZnsConfig znsConfig = dbContext.tbl_ZnsConfig.FirstOrDefault();
//            //kiểm tra access token còn hiệu lực không (24h nhưng check 20h thui :> )
//            bool checkExpired = (int)DateTime.Now.Subtract(znsConfig.TokenModifiedOn).TotalHours >= 20;
//            if (checkExpired)
//            {
//                znsAccessTokenResponse = this.CheckToken(znsConfig.RefreshToken, znsConfig.AppId, znsConfig.SecretKey).Result;
//            }
//            else
//            {
//                // xài token cũ
//                znsAccessTokenResponse.Data = new ZnsAuthResponse { access_token = znsConfig.AccessToken, refresh_token = znsConfig.RefreshToken, expires_in = znsConfig.ExpiresIn };
//                znsAccessTokenResponse.Success = true;
//            }
//            return znsAccessTokenResponse;
//        }
//        private async Task<ZnsAccessTokenResponse> CheckToken(string refreshToken, string appId, string secretKey)
//        {
//            ZnsAccessTokenResponse znsAccessTokenResponse = new ZnsAccessTokenResponse();
//            string endPoint = @"https://oauth.zaloapp.com/v4/oa/access_token";
//            var client = new HttpClient();
//            string body = $"app_id={appId}&refresh_token={refreshToken}&grant_type=refresh_token";
//            HttpContent content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
//            client.DefaultRequestHeaders.Add("secret_key", secretKey);
//            IActionResult response = await client.PostAsync(endPoint, content);
//            if (response.StatusCode == System.Net.(int)HttpStatusCode.OK)
//            {
//                string responseBody = await response.Content.ReadAsStringAsync();
//                ZnsAuthResponse znsAuthResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ZnsAuthResponse>(responseBody);
//                if (!string.IsNullOrEmpty(znsAuthResponse.access_token))
//                {
//                    znsAccessTokenResponse.Data = znsAuthResponse;
//                    znsAccessTokenResponse.Success = true;
//                }
//                else
//                {
//                    znsAccessTokenResponse.Success = false;
//                }
//            }
//            else
//            {
//                znsAccessTokenResponse.Success = false;
//            }
//            return znsAccessTokenResponse;
//        }
//        public void Noti(ZnsTemplateModel template, tbl_UserInformation user)
//        {
//            lock (ZaloNotificationService.lockObject)
//            {
//                if (!ListZnsTemplateType().Where(x => x.Key == template.Type).Any())
//                {
//                    return;
//                }
//                ZnsAccessTokenResponse znsAccessToken = this.GetAccessToken();
//                if (!znsAccessToken.Success)
//                {
//                    return;
//                }

//                tbl_ZnsTemplate znsTemplate = dbContext.tbl_ZnsTemplate.FirstOrDefault(x => x.Enable == true && x.Type == template.Type);
//                if (znsTemplate == null)
//                {
//                    return;
//                }

//                string endPoint = @"https://oauth.zaloapp.com/v4/oa/access_token";

//                ZnsRequest znsRequest = new ZnsRequest();
//                znsRequest.template_id = znsTemplate.TemplateId;
//                znsRequest.template_data = template.TemplateData;
//                znsRequest.mode = "development";
//                znsRequest.phone = user.Mobile;
//                string body = Newtonsoft.Json.JsonConvert.SerializeObject(znsRequest);
//                HttpContent content = new StringContent(body, Encoding.UTF8, "application/json");
//                var client = new HttpClient();
//                client.DefaultRequestHeaders.Add("access_token", znsAccessToken.Data.access_token);
//                IActionResult response = client.PostAsync(endPoint, content).Result;
//                response.EnsureSuccessStatusCode();
//                string responseBody = response.Content.ReadAsStringAsync().Result;
//                //kiểm trả response
//                Console.WriteLine(responseBody);
//            }
//        }
//        public async Task<List<tbl_ZnsTemplate>> GetZnsTemplate()
//        {
//            List<tbl_ZnsTemplate> znsTemplate = await dbContext.tbl_ZnsTemplate.Where(x => x.Enable == true).ToListAsync();
//            return znsTemplate;
//        }
//        public async Task<tbl_ZnsTemplate> GetZnsTemplateById(int id)
//        {
//            tbl_ZnsTemplate znsTemplate = await dbContext.tbl_ZnsTemplate.SingleOrDefaultAsync(x => x.Enable == true && x.Id == id);
//            return znsTemplate;
//        }
//        public async Task<List<Get_ZnsConfig>> GetZnsConfig()
//        {
//            List<tbl_ZnsConfig> znsConfig = await dbContext.tbl_ZnsConfig.ToListAsync();
//            var data = znsConfig.Select(x => new Get_ZnsConfig(x)).ToList();
//            return data;
//        }
//        public async Task<Get_ZnsConfig> GetZnsConfigById(int id)
//        {
//            tbl_ZnsConfig znsConfig = await dbContext.tbl_ZnsConfig.SingleOrDefaultAsync(x => x.Enable == true && x.Id == id);
//            var data = new Get_ZnsConfig(znsConfig);
//            return data;
//        }

//        public async Task<Get_ZnsConfig> ZnsConfigUpdate(ZnsConfigUpdate request, tbl_UserInformation userLog)
//        {
//            tbl_ZnsConfig znsConfig = await dbContext.tbl_ZnsConfig.SingleOrDefaultAsync(x => x.Id == request.Id && x.Enable == true);
//            if (znsConfig == null)
//            {
//                throw new Exception("Dữ liệu không tồn tại");
//            }
//            if (!string.IsNullOrEmpty(request.RefreshToken))
//            {
//                var checkToken = await this.CheckToken(request.RefreshToken, request.AppId, request.SecretKey);
//                if (!checkToken.Success)
//                {
//                    throw new Exception("Refresh Token không hợp lệ");
//                }
//                znsConfig.RefreshToken = checkToken.Data.refresh_token;
//                znsConfig.AccessToken = checkToken.Data.access_token;
//                znsConfig.ExpiresIn = checkToken.Data.expires_in;
//                znsConfig.TokenModifiedOn = DateTime.Now;
//            }

//            znsConfig.AppId = request.AppId;
//            znsConfig.SecretKey = request.SecretKey;
//            znsConfig.ModifiedBy = userLog.FullName;
//                znsConfig.ModifiedOn = DateTime.Now;

//            await dbContext.SaveChangesAsync();
//            return new Get_ZnsConfig(znsConfig);
//        }
//        public async Task<tbl_ZnsTemplate> ZnsTemplateUpdate(ZnsTemplateUpdate request, tbl_UserInformation userLog)
//        {
//            tbl_ZnsTemplate znsTemplate = await dbContext.tbl_ZnsTemplate.SingleOrDefaultAsync(x => x.Id == request.Id && x.Enable == true);
//            if (znsTemplate == null)
//            {
//                throw new Exception("Dữ liệu không tồn tại");
//            }
//            znsTemplate.TemplateId = request.TemplateId;
//            znsTemplate.ModifiedBy = userLog.FullName;
//            await dbContext.SaveChangesAsync();
//            return znsTemplate;
//        }

//        public List<ZnsTemplateSample> GetTemplateSample(int templateType)
//        {
//            List<ZnsTemplateSample> znsTemplates = new List<ZnsTemplateSample>();
//            PropertyInfo[] properties;
//            if (templateType == (int)ZnsTemplateType.ThongBaoBaiKiemTra)
//            {
//                properties = typeof(ExamNoticeTemplate).GetProperties();
//            }
//            else if (templateType == (int)ZnsTemplateType.ThongBaoHocPhi)
//            {
//                properties = typeof(TuitionNoticeTemplate).GetProperties();
//            }
//            else
//            {
//                return znsTemplates;
//            }

//            foreach (var p in properties)
//            {
//                ZnsTemplateSample item = new ZnsTemplateSample();
//                var desc = p.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false).FirstOrDefault() as DescriptionAttribute;
//                item.FieldName = $"<{p.Name}>";
//                item.Description = desc.Description;
//                znsTemplates.Add(item);
//            }

//            return znsTemplates;
//        }
//    }
//}