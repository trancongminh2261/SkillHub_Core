using LMSCore.DTO.BranchDTO;
using LMSCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UAParser;
using static LMSCore.DTO.StudentDeviceDTO.StudentDeviceDTO;

namespace LMSCore.Utilities
{
    public class GetDataConfig
    {
        public static async Task<List<GetBranchesDTO>> GetBranches(string branchIds)
        {
            using (var db = new lmsDbContext())
            {
                var result = new List<GetBranchesDTO>();
                if (string.IsNullOrEmpty(branchIds))
                    return result;
                var listBranchId = branchIds.Split(',').ToList();
                result = await db.tbl_Branch.Where(x => listBranchId.Contains(x.Id.ToString())).
                    Select(x => new GetBranchesDTO
                    {
                        Id = x.Id,
                        Name = x.Name
                    }).ToListAsync();
                return result;
            }
        }
        public static async Task<double> GetFileSizeInMBAsync(string fileUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                // Gửi yêu cầu GET để tải xuống tệp từ URL
                HttpResponseMessage response = await client.GetAsync(fileUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Đọc dữ liệu tệp dưới dạng byte array
                    byte[] fileData = await response.Content.ReadAsByteArrayAsync();

                    // Chuyển đổi dung lượng từ byte sang MB
                    double fileSizeInMB = Math.Round(fileData.Length / (1024.0 * 1024.0), 2);

                    // Trả về dung lượng của tệp dưới dạng MB
                    return fileSizeInMB;
                }
                else
                {
                    return 0.0;
                }
            }
        }

        public static async Task<DeviceModel> GetDeviceName(HttpContext context)
        {
            // Lấy User-Agent từ request headers
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            // Sử dụng UAParser để phân tích User-Agent
            var uaParser = Parser.GetDefault();
            ClientInfo clientInfo = uaParser.Parse(userAgent);

            // Lấy tên thiết bị
            string deviceName = clientInfo.Device.Family;
            //string deviceName = Environment.MachineName;
            string os = clientInfo.OS.Family;
            if (os.Contains("Windows"))
                deviceName = "Windows PC";
            if(os.Contains("Macintosh"))
                deviceName = "Mac";

            string browser = clientInfo.UA.Family;
            // deviceName = $"{deviceName} ({os} - {browser})";
            var result = new DeviceModel
            {
                DeviceName = deviceName,
                OS = os,
                Browser = browser
            };
            return result;
        }

        public static string GetFileName(string linkFile)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(linkFile))
                return result;
            var arr = linkFile.Split("/").ToArray();
            var name = arr[arr.Length - 1].Split("-")[0];
            var ext = arr[arr.Length - 1].Split("-")[1].Split(".")[1];
            result = name + "." + ext;
            return result;
        }
    }
}
