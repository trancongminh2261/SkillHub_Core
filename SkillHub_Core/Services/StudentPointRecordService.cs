using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;


using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LMSCore.Services.TranscriptService;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace LMSCore.Services
{
    public class StudentPointRecordService
    {
        public static async Task<tbl_StudentPointRecord> GetById(int StudentPointRecordId)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_StudentPointRecord.SingleOrDefaultAsync(x => x.Id == StudentPointRecordId
                && x.Enable == true);
                return data;
            }
        }
        public static async Task<AppDomainResult> GetAll(StudentPointRecordSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    return new AppDomainResult { TotalRow = 0, Data = null };

                string sql = $"Get_StudentPointRecord @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Search = N'{baseSearch.Search ?? ""}'," +
                    $"@ClassId = {baseSearch.ClassId}," +
                    $"@Year = {baseSearch.Year}," +
                    $"@Month = {baseSearch.Month}";
                var data = await db.SqlQuery<Get_StudentPointRecord>(sql);

                int countNonCreatedRecord = 0;
                int countListTranscript = 0;
                // lấy ds đợt thi
                var listTranscript = await GetListTermByClassIdAndYearMonth(baseSearch.ClassId, baseSearch.Year, baseSearch.Month);
                {
                    // lọc từng đợt thi
                    foreach (var transcript in listTranscript)
                    {
                        // ds bảng điểm theo từng đợt thi
                        var x = await db.tbl_Point.Where(xx => xx.TranscriptId == transcript.Id).ToListAsync();
                        countListTranscript = countListTranscript + x.Count();
                    }
                }
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true, ResultMessage = countNonCreatedRecord + " bảng điểm chưa tạo" };

                int totalRow = data[0].TotalRow;
                countNonCreatedRecord = countListTranscript - totalRow;
                var result = data.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true, ResultMessage = countNonCreatedRecord + " bảng điểm chưa tạo" };
            }
        }

        // Tạo record cho n học viên theo, lớp, năm, tháng --> trả về ds n record
        public static async Task<List<tbl_StudentPointRecord>> Insert(StudentPointRecordCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                // tạo biến lưu List trả về
                var listData = new List<tbl_StudentPointRecord>();
                // lấy ds đợt thi
                var listTranscript = await GetListTermByClassIdAndYearMonth(itemModel.ClassId, itemModel.Year, itemModel.Month);
                // ds các bảng điểm của các đợt thi,
                var listStudentPoint = new List<List<PointModel>>();
                // ds hv trong lớp đó
                var listStudent = await db.tbl_StudentInClass.Where(x => x.ClassId == itemModel.ClassId && x.Enable == true).ToListAsync();
                ////
                if (listTranscript != null)
                {
                    // lọc từng đợt thi
                    foreach (var transcript in listTranscript)
                    {
                        // ds bảng điểm theo từng đợt thi,( chứa bảng điểm nhiều sv)
                        var x = await GetListTranscriptionByTermId(transcript.Id, user);
                        listStudentPoint.Add(x);
                    }
                }
                // lọc từng bảng, lọc từng bảng điểm, lấy từng thông tin điểm mỗi sv
                foreach (var studentTranscript in listStudent)
                {
                    var isExisted = await IsExistStudentRecord(itemModel.ClassId, itemModel.Year, itemModel.Month, studentTranscript.StudentId);
                    //tồn tại rồi thì bỏ qua
                    if (isExisted == true) continue;
                    // chưa có thì tạo
                    List<PointModel> listStudentPointModel = new List<PointModel>();
                    //n đợt thi, mỗi đợt thi --> n bảng điểm, mỗi bảng --> n thông tin điểm hv, lấy 1 thông tin điểm hv
                    foreach (var listPointModel in listStudentPoint)
                    {
                        // item đây là thông tin từng hv
                        foreach (var item in listPointModel)
                        {
                            if (studentTranscript.StudentId == item.StudentId)
                            {
                                listStudentPointModel.Add(item);
                                break;
                            }
                        }
                    }

                    // tính Attendance 
                    Attendance attendance = await CalculateStudentAttendance(itemModel.ClassId, itemModel.Year, itemModel.Month, studentTranscript.StudentId);
                    // tạo record cho 1 hv
                    PointRecordForOneStudentCreate pointRecordForOneStudentCreate = new PointRecordForOneStudentCreate();
                    pointRecordForOneStudentCreate.ClassId = itemModel.ClassId;
                    pointRecordForOneStudentCreate.Year = itemModel.Year;
                    pointRecordForOneStudentCreate.Month = itemModel.Month;
                    pointRecordForOneStudentCreate.StudentId = studentTranscript.StudentId;
                    pointRecordForOneStudentCreate.Attend = attendance.Attend;
                    pointRecordForOneStudentCreate.TotalLessons = attendance.TotalLessons;
                    pointRecordForOneStudentCreate.Unexcused = attendance.Unexcused;
                    pointRecordForOneStudentCreate.Transcript = listStudentPointModel;
                    var OneStudentPointRecord = await CreatePointRecordForOneStudent(pointRecordForOneStudentCreate, user);
                    listData.Add(OneStudentPointRecord);
                }

                return listData;
            }
        }

        // sửa 1 record của 1 hv
        public static async Task<tbl_StudentPointRecord> Update(StudentPointRecordUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudentPointRecord.SingleOrDefaultAsync(x => x.Id == itemModel.Id
                && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy bảng điểm học viên");

                entity.Behaviour = itemModel.Behaviour ?? entity.Behaviour;
                entity.AcademicPerformance = itemModel.AcademicPerformance ?? entity.AcademicPerformance;
                entity.Note = itemModel.Note ?? entity.Note;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int recordId)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudentPointRecord.SingleOrDefaultAsync(x => x.Id == recordId
                 && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy bảng điểm học viên");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }

        // kiểm tra xem lớp đó, theo đợt (năm tháng) đã tạo record chưa

        // trả về ds đợt thi theo lớp học, năm và tháng
        public static async Task<List<tbl_Transcript>> GetListTermByClassIdAndYearMonth(int classId, int year, int month)
        {
            using (var db = new lmsDbContext())
            {
                DateTime datetimeStart = new DateTime(year, month, 1);
                DateTime datetimeEnd = new DateTime(year, month + 1, 1);

                return await db.tbl_Transcript.Where(x => x.ClassId == classId
                && x.CreatedOn >= datetimeStart
                && x.CreatedOn < datetimeEnd
                && x.Enable == true).OrderByDescending(x => x.Id).ToListAsync();


            }
        }
        // trả về ds tất cả bảng điểm theo đợt thi
        public static async Task<List<PointModel>> GetListTranscriptionByTermId(int termId, tbl_UserInformation user)
        {
            var data = await TranscriptService.GetPoint(termId, user);
            return data;
        }
        // tạo record cho một sv 
        public static async Task<tbl_StudentPointRecord> CreatePointRecordForOneStudent(PointRecordForOneStudentCreate model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    //Tạo record cho 1 sv
                    var data = new tbl_StudentPointRecord
                    {
                        ClassId = model.ClassId,
                        Year = model.Year,
                        Month = model.Month,
                        StudentId = model.StudentId,

                        Attend = model.Attend,
                        TotalLessons = model.TotalLessons,
                        Unexcused = model.Unexcused,

                        Transcript = Newtonsoft.Json.JsonConvert.SerializeObject(model.Transcript),
                        Enable = true,
                        CreatedBy = user.FullName,
                        ModifiedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now
                    };
                    db.tbl_StudentPointRecord.Add(data);
                    await db.SaveChangesAsync();
                    return data;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static async Task<Attendance> CalculateStudentAttendance(int classId, int year, int month, int? studentId)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    Attendance attendance = new Attendance();
                    DateTime datetimeStart = new DateTime(year, month, 1);
                    DateTime datetimeEnd = new DateTime(year, month + 1, 1);
                    var listTotalLessons = await db.tbl_Schedule.Where(x => x.ClassId == classId
                                                        && x.StartTime >= datetimeStart
                                                        && x.EndTime < datetimeEnd
                                                        && x.Enable == true).ToListAsync();
                    int TotalLessons = listTotalLessons.Count();
                    int Attend = 0;
                    int Unexcused = 0;
                    foreach (var item in listTotalLessons)
                    {
                        var a = await db.tbl_RollUp.Where(x => x.ClassId == classId
                                                         && x.StudentId == studentId
                                                         && x.ScheduleId == item.Id
                                                         && x.Enable == true
                                                         && x.Status == 1).ToListAsync();
                        Attend = Attend + a.Count();
                        var b = await db.tbl_RollUp.Where(x => x.ClassId == classId
                                                         && x.StudentId == studentId
                                                         && x.ScheduleId == item.Id
                                                         && x.Enable == true
                                                         && x.Status == 3).ToListAsync();
                        Unexcused = Unexcused + b.Count();
                    }
                    attendance.TotalLessons = TotalLessons;
                    attendance.Attend = Attend;
                    attendance.Unexcused = Unexcused;
                    return attendance;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static async Task<bool> IsExistStudentRecord(int classId, int year, int month, int? studentId)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    bool res = false;
                    res = await db.tbl_StudentPointRecord.AnyAsync(x => x.ClassId == classId
                                        && x.Year == year
                                        && x.Month == month
                                        && x.StudentId == studentId
                                        && x.Enable == true);
                    return res;
                }
                catch (Exception e) { throw e; }
            }
        }
        public static async Task<tbl_StudentPointRecord> StudentReportHtmlToPdf(int StudentPointRecordId, string path, string pathViews, string domain)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    // lấy ra record 
                    var item = await db.tbl_StudentPointRecord.SingleOrDefaultAsync(x => x.Id == StudentPointRecordId && x.Enable == true);

                    if (item == null)
                        throw new Exception("Không tìm thấy bảng điểm học viên");
                    ///

                    var student = db.tbl_UserInformation.FirstOrDefault(x => x.UserInformationId == item.StudentId);
                    var studentClass = db.tbl_Class.FirstOrDefault(x => x.Id == item.ClassId);

                    //đường dẫn tới html report
                    string content = System.IO.File.ReadAllText($"{pathViews}/Base/StudentsReportBase.cshtml");
                    // nơi in ra record 
                    var dateTimeNow = DateTime.Now.ToString("ddMMyyyHHmmss");
                    var nameFile = "student" + AssetCRM.RemoveUnicode(student.FullName).Replace(" ", "") + dateTimeNow + "_report.pdf";
                    string savePath = $"{path}/Report/StudentsReportBase/{nameFile}";

                    content = content.Replace("{month}", item.Month.ToString());
                    content = content.Replace("{year}", item.Year.ToString());

                    content = content.Replace("{fullname}", student.FullName);
                    content = content.Replace("{class}", studentClass == null ? "" : studentClass.Name);

                    content = content.Replace("{Attend}", item.Attend.ToString());
                    content = content.Replace("{TotalLessons}", item.TotalLessons.ToString());
                    content = content.Replace("{Unexcused}", item.Unexcused.ToString());
                    var a = new StringBuilder();
                    bool aa = true;
                    var listTranscript = JsonConvert.DeserializeObject<List<PointModel>>(item.Transcript);
                    foreach (var transcript in listTranscript)
                    {
                        var term = await db.tbl_Transcript.FirstOrDefaultAsync(x => x.Id == transcript.TermId && x.Enable == true);
                        var date = term.CreatedOn?.ToString("dd/MM/yyyy");
                        var reading = string.IsNullOrEmpty(transcript.Reading) ? "0" : transcript.Reading;
                        var listening = string.IsNullOrEmpty(transcript.Listening) ? "0" : transcript.Listening;
                        var writing = string.IsNullOrEmpty(transcript.Writing) ? "0" : transcript.Writing;
                        var grammar = string.IsNullOrEmpty(transcript.Grammar) ? "0" : transcript.Grammar;
                        var speaking = string.IsNullOrEmpty(transcript.Speaking) ? "0" : transcript.Speaking;
                        var medium = string.IsNullOrEmpty(transcript.Medium) ? "0" : transcript.Medium;
                        var passFail = transcript.PassOrFail != null ? (transcript.PassOrFail == false ? "Trượt" : "Đậu") : "...";
                        if (aa == false)
                        {
                            a.Append($"<tr>");
                            a.Append($"<td class=\"text-center text-bold\">{date}</td>");
                            a.Append($" <td class=\"text-center text-bold\">{term.Name}</td>");
                            a.Append($"<td class=\"text-center MarkFail\">{reading}</td>");
                            a.Append($"<td class=\"text-center MarkFail\">{listening}</td>");
                            a.Append($"<td class=\"text-center MarkFail\">{writing}</td>");
                            a.Append($"<td class=\"text-center MarkFail\">{grammar}</td>");
                            a.Append($"<td class=\"text-center MarkFail\">{speaking}</td>");
                            a.Append($"<td class=\"text-center text-bold MarkFail\">{medium}/100-{passFail}</td>");
                            a.Append($"</tr>");
                        }
                        else
                        {
                            content = content.Replace("{Date}", date);
                            content = content.Replace("{TestName}", term.Name);
                            //điểm
                            content = content.Replace("{ReadMark}", reading);
                            content = content.Replace("{ListenMark}", listening);
                            content = content.Replace("{WriteMark}", writing);
                            content = content.Replace("{GrammarMark}", grammar);
                            content = content.Replace("{SpeakMark}", speaking);
                            content = content.Replace("{Mark}", medium);
                            content = content.Replace("{PassOrFail}", passFail);
                            aa = false;
                        }
                    }
                    content = content.Replace("{RL}", a.ToString());
                    //Comments from the homeroom teacher
                    content = content.Replace("{Behaviour}", item.Behaviour);
                    content = content.Replace("{AcademicPerformance}", item.AcademicPerformance);
                    content = content.Replace("{Note}", item.Note);

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
                            Format = PaperFormat.A4,
                            PrintBackground = true
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
                    item.PDFUrl = $"{domain}/Upload/Report/StudentsReportBase/{nameFile}";
                    await db.SaveChangesAsync();


                    return item;
                }

                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}