﻿using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;

using LMS_Project.Models;
using Newtonsoft.Json;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.Configuration;

using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LMSCore.Users;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using static LMSCore.Models.lmsEnum;
using LMSCore.LMS;
using Microsoft.Extensions.Configuration;


namespace LMS_Project.Services
{
    public class CertificateService : DomainService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public CertificateService(lmsDbContext dbContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(dbContext)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor; 
        }
        public async Task<tbl_Certificate> GetById(int id)
        {
            return await dbContext.tbl_Certificate.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<AppDomainResult> GetAll(CertificateSearch search, tbl_UserInformation user)
        {
            if (search == null) return new AppDomainResult { TotalRow = 0, Data = null };
            if (user.RoleId == ((int)RoleEnum.student))
                search.UserId = user.UserInformationId;
            string sql = $"Get_Certificate @PageIndex = {search.PageIndex}," +
                $"@PageSize = {search.PageSize}," +
                $"@VideoCourseId = {search.VideoCourseId}," +
                $"@UserId = {search.UserId}";
            var data = await dbContext.SqlQuery<Get_Certificate>(sql);
            var myCourses = await dbContext.tbl_VideoCourseStudent
                .Where(x => x.UserId == user.UserInformationId && x.Enable == true).Select(x => x.VideoCourseId).ToListAsync();
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            var result = data.Select(i => new tbl_Certificate(i)).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        public  async Task<tbl_Certificate> Update(CertificateUpdate model, tbl_UserInformation user)
        {
            var entity = await dbContext.tbl_Certificate.SingleOrDefaultAsync(x => x.Id == model.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy chứng chỉ");
                entity.Content = model.Content ?? entity.Content;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await dbContext.SaveChangesAsync();
                return entity;
        }
        //public static async Task CreateCertificate(tbl_UserInformation user)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        try
        //        {
        //            var entity = await dbContext.tbl_Certificate.AnyAsync(x => x.UserId == user.UserInformationId);
        //            if (entity)
        //                throw new Exception("Bạn đã được cấp chứng chỉ");
        //            var config = await dbContext.tbl_CertificateConfig.FirstOrDefaultAsync();
        //            if (config == null)
        //                throw new Exception("Chưa tạo mẫu chứng chỉ, vui lòng liên hệ người quản trị");

        //            var videoCoures = await dbContext.tbl_VideoCourse.Where(x => x.Enable == true && x.Active == true)
        //                .Select(x => new { x.Id, x.Name }).ToListAsync();
        //            if (!videoCoures.Any())
        //                throw new Exception("Không tìm thấy khoá học");
        //            foreach (var item in videoCoures)
        //            {
        //                var lastSection = await dbContext.tbl_Section.Where(x => x.VideoCourseId == item.Id && x.Enable == true)
        //                    .OrderByDescending(x => x.Index).Select(x => x.Id).FirstOrDefaultAsync();
        //                if (lastSection != 0)
        //                {
        //                    var completed = await dbContext.tbl_SectionCompleted.AnyAsync(x => x.SectionId == lastSection && x.Enable == true && x.UserId == user.UserInformationId);
        //                    if (!completed)
        //                        throw new Exception($"bạn chưa hoàn thành khoá học {item.Name}");
        //                }
        //            }
        //            var model = new tbl_Certificate
        //            {
        //                Content = CertificateConfigService.ReplaceContent(config.Content, user),
        //                CreatedBy = user.FullName,
        //                CreatedOn = DateTime.Now,
        //                Enable = true,
        //                ModifiedBy = user.FullName,
        //                ModifiedOn = DateTime.Now,
        //                UserId = user.UserInformationId
        //            };
        //            dbContext.tbl_Certificate.Add(model);
        //            await dbContext.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            throw e;
        //        }
        //    }
        //}
        public async Task CreateCertificate(int videoCourseId, int studentId)
        {
            try
            {
                var entityExists = await dbContext.tbl_Certificate.AnyAsync(x => x.UserId == studentId && x.VideoCourseId == videoCourseId && x.Enable == true);
                if (!entityExists)
                {
                    var videoCourse = await dbContext.tbl_VideoCourse.FirstOrDefaultAsync(x => x.Enable == true && x.Active == true && x.Id == videoCourseId);
                    if (videoCourse == null)
                        return;

                    var config = await dbContext.tbl_CertificateConfig
                        .SingleOrDefaultAsync(x => x.Id == videoCourse.CertificateConfigId);
                    if (config == null)
                        return;

                    var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentId);
                    var model = await dbContext.tbl_Certificate.FirstOrDefaultAsync(x => x.UserId == studentId && x.VideoCourseId == videoCourseId && x.Enable == true);
                    if (model == null)
                    {
                        model = new tbl_Certificate
                        {
                            Content = CertificateConfigService.ReplaceContent(config.Content, videoCourse.Name, student),
                            CreatedBy = student.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true,
                            ModifiedBy = student.FullName,
                            ModifiedOn = DateTime.Now,
                            UserId = student.UserInformationId,
                            Background = config.Background,
                            VideoCourseId = videoCourse.Id,
                            Backside = config.Backside
                        };
                        dbContext.tbl_Certificate.Add(model);
                        await dbContext.SaveChangesAsync();

                        // Gửi mail đến người dùng
                        var request = _httpContextAccessor.HttpContext.Request;
                        string baseUrl = $"{request.Scheme}://{request.Host}";
                        var uploadPath = $"{baseUrl}/Upload";
                        var viewsPath = $"{baseUrl}/Views";

                        string content = System.IO.File.ReadAllText($"{viewsPath}/Home/ExportCertificate.cshtml");
                        content = content.Replace("{background}", model.Background);
                        content = content.Replace("{content}", model.Content);
                        model = await ExportPDF(model.Id, content, uploadPath, baseUrl);

                        string projectName = _configuration["MySettings:ProjectName"];

                        string contentSendMail = System.IO.File.ReadAllText($"{viewsPath}/Home/SendMailCertificate.cshtml");
                        contentSendMail = contentSendMail.Replace("{TenHocVien}", student.FullName);
                        contentSendMail = contentSendMail.Replace("{TenChungChi}", videoCourse.Name);
                        contentSendMail = contentSendMail.Replace("{TenToChuc}", projectName);
                        contentSendMail = contentSendMail.Replace("{PDFUrl}", model.PDFUrl);
                        contentSendMail = contentSendMail.Replace("{DomainAPI}", baseUrl);

                        Thread sendMail = new Thread(() =>
                        {
                            AssetCRM.SendMail(student.Email, $"Cấp chứng chỉ khóa học {videoCourse.Name}", contentSendMail);
                        });
                        sendMail.Start();
                    }
                    else
                    {
                        model.Content = CertificateConfigService.ReplaceContent(config.Content, videoCourse.Name, student);
                        model.Background = config.Background;
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception e)
            {
                throw e; // Consider logging the exception instead of throwing it again
            }
        }


        public async Task<tbl_Certificate> ExportPDF(int id, string content, string path, string domain)
        {
            var entity = await dbContext.tbl_Certificate.SingleOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new Exception("Không tìm thấy chứng chỉ");
            var user = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.UserId);

            string savePath = $"{path}/Certificate_{user.UserCode}.pdf";

            if (!string.IsNullOrEmpty(entity.PDFUrl))
                try
                {
                    File.Delete(savePath);
                }
                catch { }

            using (MemoryStream stream = new MemoryStream())
            {
                var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
                {
                    Path = path
                });
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = browserFetcher.RevisionInfo(BrowserFetcher.DefaultRevision).ExecutablePath
                });
                var page = await browser.NewPageAsync();
                await page.EmulateMediaTypeAsync(MediaType.Screen);
                await page.SetContentAsync(content);
                var pdfContent = await page.PdfStreamAsync(new PdfOptions
                {
                    Width = 794,
                    Height = 1123,
                    //Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new PuppeteerSharp.Media.MarginOptions() { Top = "0px" },
                    //PageRanges = "2",
                });
                await pdfContent.CopyToAsync(stream);

                byte[] bytes = stream.ToArray();
                stream.Close();
                await browser.CloseAsync();

                var newStream = new MemoryStream(bytes);

                using (var fileStream = System.IO.File.Create(savePath))
                {
                    newStream.WriteTo(fileStream);
                }
            }
            entity.PDFUrl = $"{domain}/Upload/Certificate/Certificate_{user.UserCode}.pdf";
            await dbContext.SaveChangesAsync();
            return entity;
        }

    }
}