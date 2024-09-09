using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using LMSCore.Utilities;
using LMSCore.DTO.ClassDTO;
using OfficeOpenXml.Style;

namespace LMSCore.Services
{
    public class ExcelExportService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExcelExportService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        #region export excel V2 - hàm dùng chung đối với những file chỉ đổ dữ liệu không xử lý gì thêm
        /// <summary>
        /// màu chủ đạo của hệ thống
        /// </summary>
        public static string colorCode = configuration.GetSection("MySettings:ColorCode").Value.ToString();
        public static void SetEffectTitle(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml(colorCode));
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            // Tô viền cho ô
            /*cell.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
            cell.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;*/
            cell.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
            cell.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
        }
        public static string ExportV2<T>(string templateName, string folderToSave, List<T> items, List<string> listTitle, string fileExportName, string baseUrl)
        {
            byte[] bytes;
            var env = WebHostEnvironment.Environment;
            var templatePath = Path.Combine(env.ContentRootPath, "Template", templateName);
            using (var stream = new MemoryStream())
            {
                FetchV2(stream, templatePath, items, listTitle);
                bytes = stream.ToArray();
            }

            // Xóa tất cả các tệp trong thư mục trước khi lưu tệp mới
            var downloadFolderPath = Path.Combine(env.ContentRootPath, "Download", folderToSave);
            foreach (string filePath in Directory.GetFiles(downloadFolderPath))
            {
                File.Delete(filePath);
            }
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string nameFileExport = $"{fileExportName}_{timestamp}.xlsx";
            var path = Path.Combine(env.ContentRootPath, "Download", folderToSave, nameFileExport);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllBytes(Path.Combine(path), bytes);

            string link = baseUrl + "/Download/" + folderToSave + "/" + nameFileExport;
            return link.IndexOf("https") == -1 ? link.Replace("http", "https") : link;
        }

        public static void FetchV2<T>(Stream stream, string templateName, List<T> items, List<string> listTitle)
        {
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentNullException(nameof(templateName));

            using (var templateDocumentStream = File.OpenRead(templateName))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var xlPackage = new ExcelPackage(templateDocumentStream))
                {
                    var sheet = xlPackage.Workbook.Worksheets[0];
                    var rowIndex = 1;
                    var column = 1; // Bắt đầu từ cột đầu tiên
                    for (int i = 0; i < listTitle.Count; i++)
                    {
                        sheet.Cells[rowIndex, column + i].Value = listTitle[i];
                        SetEffectTitle(sheet.Cells[rowIndex, column + i]);
                    }
                    rowIndex++;

                    foreach (var model in items)
                    {
                        foreach (PropertyInfo item in model.GetType().GetProperties())
                        {
                            var value = item.GetValue(model);
                            // Kiểm tra nếu giá trị là kiểu DateTime thì định dạng lại
                            if (item.PropertyType == typeof(DateTime))
                            {
                                value = ((DateTime)value).ToString("dd/MM/yyyy");
                            }
                            // Kiểm tra nếu giá trị là kiểu bool và là true thì value = x
                            else if (item.PropertyType == typeof(bool))
                            {
                                if ((bool)value == true)
                                {
                                    value = "x";
                                }
                                else
                                {
                                    value = ""; // Nếu là false thì gán giá trị rỗng
                                }
                            }
                            // Các cột khác: Giá trị
                            sheet.Cells[rowIndex, column].Value = value;
                            column++;
                        }
                        rowIndex++;
                        column = 1;
                    }
                    xlPackage.SaveAs(stream);
                }
            }
        }
        #endregion
        public static void Fetch<T>(Stream stream, string templateName, List<T> items)
        {
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentNullException(nameof(templateName));
            using (var templateDocumentStream = File.OpenRead(templateName))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var xlPackage = new ExcelPackage(templateDocumentStream))
                {
                    var sheet = xlPackage.Workbook.Worksheets[0];
                    var rowIndex = 3;
                    var column = 1;
                    const int index = 2;
                    foreach (var model in items)
                    {
                        sheet.Cells[rowIndex, column].Value = rowIndex - index;
                        column++;
                        foreach (PropertyInfo item in model.GetType().GetProperties())
                        {
                            var value = item.GetValue(model);
                            sheet.Cells[rowIndex, column].Value = value;
                            column++;
                        }
                        rowIndex++;
                        column = 1;
                    }
                    xlPackage.SaveAs(stream);
                }
            }
        }

        // Export xử dụng tên radom ngẫu nhiên
        public static string Export<T>(List<T> items, string templateName, string folderToSave)
        {
            byte[] bytes;
            var env = WebHostEnvironment.Environment;
            var templatePath = Path.Combine(env.ContentRootPath, "Template", templateName);
            using (var stream = new MemoryStream())
            {
                Fetch(stream, templatePath, items);
                bytes = stream.ToArray();
            }
            string nameFileExport = $"{Guid.NewGuid()}.xlsx";
            var path = Path.Combine(env.ContentRootPath, "Download", folderToSave, nameFileExport);
            File.WriteAllBytes(path, bytes);
            return path;
        }

        // Export xử dụng tên custom --fileNameToSave--
        public static string ExportV3<T>(List<T> items, string templateName, string fileNameToSave, string folderToSave, string baseUrl)
        {
            byte[] bytes;
            var env = WebHostEnvironment.Environment;
            var templatePath = Path.Combine(env.ContentRootPath, "Template", templateName);
            using (var stream = new MemoryStream())
            {
                Fetch(stream, templatePath, items);
                bytes = stream.ToArray();
            }
            // Xóa tất cả các tệp trong thư mục trước khi lưu tệp mới
            var downloadFolderPath = Path.Combine(env.ContentRootPath, "Download", folderToSave);
            foreach (string filePath in Directory.GetFiles(downloadFolderPath))
            {
                File.Delete(filePath);
            }
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string nameFileExport = $"{fileNameToSave}_{timestamp}.xlsx";
            var path = Path.Combine(env.ContentRootPath, "Download", folderToSave, nameFileExport);
            File.WriteAllBytes(Path.Combine(path), bytes);

            string link = baseUrl + "/Download/" + folderToSave + "/" + nameFileExport;
            return link.IndexOf("https") == -1 ? link.Replace("http", "https") : link;
        }

        public static void FetchTemplate(Stream stream, string templateName, List<ListDropDown> dropdown)
        {
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentNullException(nameof(templateName));
            using (var templateDocumentStream = File.OpenRead(templateName))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var xlPackage = new ExcelPackage(templateDocumentStream))
                {
                    var sheet = xlPackage.Workbook.Worksheets[0];
                    if (dropdown.Any())
                    {
                        foreach (var dropdownItem in dropdown)
                        {
                            string colName = "ValidationList" + dropdownItem.columnName;
                            // Tạo danh sách giá trị cho dropdown cho cột
                            var validationList = sheet.Workbook.Worksheets.Add(colName);
                            for (int i = 0; i < dropdownItem.dataDropDown.Count; i++)
                            {
                                validationList.Cells[i + 1, 1].Value = dropdownItem.dataDropDown[i].name;
                            }
                            var range = validationList.Cells["A1:A" + dropdownItem.dataDropDown.Count];
                            var validation = sheet.DataValidations.AddListValidation($"{dropdownItem.columnName}:{dropdownItem.columnName}");
                            validation.ShowErrorMessage = true;
                            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                            validation.ErrorTitle = "Invalid Value";
                            validation.Error = "Please select a valid value from the list.";
                            validation.Formula.ExcelFormula = $"='{colName}'!$A$1:$A${dropdownItem.dataDropDown.Count}";
                            validationList.Hidden = eWorkSheetHidden.Hidden;
                        }
                    }
                    xlPackage.SaveAs(stream);
                }
            }
        }
        public static string ExportTemplate(string folderToSave, string templateName, string fileNameToSave, List<ListDropDown> dropdown, string baseUrl)
        {
            byte[] bytes;
            var env = WebHostEnvironment.Environment;
            var templatePath = Path.Combine(env.ContentRootPath, "Template", templateName);
            using (var stream = new MemoryStream())
            {
                FetchTemplate(stream, templatePath, dropdown);
                bytes = stream.ToArray();
            }
            // Xóa tất cả các tệp trong thư mục trước khi lưu tệp mới
            var downloadFolderPath = Path.Combine(env.ContentRootPath, "Download", folderToSave);
            foreach (string filePath in Directory.GetFiles(downloadFolderPath))
            {
                File.Delete(filePath);
            }
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string nameFileExport = $"{fileNameToSave}_{timestamp}.xlsx";
            var path = Path.Combine(env.ContentRootPath, "Download", folderToSave, nameFileExport);

            File.WriteAllBytes(Path.Combine(path), bytes);

            string link = baseUrl + "/Download/" + folderToSave + "/" + nameFileExport;
            return link.IndexOf("https") == -1 ? link.Replace("http", "https") : link;
        }

        #region Xuất excel báo cáo kết quả học tập của học sinh cho phụ huynh

        public static async Task FetchNotification(Stream stream, string templateName, StudentLearningOutcomesDTO items)
        {
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentNullException(nameof(templateName));
            using (var templateDocumentStream = File.OpenRead(templateName))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var xlPackage = new ExcelPackage(templateDocumentStream))
                {
                    // Đổ data ở Sheet 1
                    var sheet1 = xlPackage.Workbook.Worksheets[0];
                    sheet1.Cells[1, 2].Value = items.ParentName;
                    sheet1.Cells[2, 2].Value = items.StudentName;
                    sheet1.Cells[3, 2].Value = items.ClassName;
                    sheet1.Cells[4, 2].Value = items.Attendance;

                    var rowIndex_sh1 = 8;
                    var column_sh1 = 1;
                    if (items.HomeworkInClassModel.Count != 0)
                    {
                        foreach (var homwork in items.HomeworkInClassModel)
                        {
                            foreach (PropertyInfo item in homwork.GetType().GetProperties())
                            {
                                var value = item.GetValue(homwork);
                                sheet1.Cells[rowIndex_sh1, column_sh1].Value = value;
                                column_sh1++;
                            }
                            rowIndex_sh1++;
                            column_sh1 = 1;
                        }
                    }

                    // Đổ data ở Sheed 2
                    var sheet2 = xlPackage.Workbook.Worksheets[1];
                    var rowIndex_sh2 = 3; // Bắt đầu từ hàng thứ 3 của sheet 2
                    var column_sh2 = 1;

                    if (items.ScoreInClassModel.Count != 0)
                    {
                        foreach (var score in items.ScoreInClassModel)
                        {
                            // Ghi tên đợt thi vào cột đầu tiên của dòng hiện tại
                            var cell = sheet2.Cells[rowIndex_sh2, 1];
                            cell.Value = score.Name;

                            // Thay đổi định dạng của ô tên đợt thi
                            cell.Style.Font.Size = 14; // Kích thước chữ
                            cell.Style.Font.Bold = true; // In đậm
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid; // Đặt PatternType trước khi đặt màu nền
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#BDD7EE")); // Màu nền

                            // Merge các ô để tên đợt thi chiếm đủ số cột tương ứng với số cột điểm
                            int mergeColumns = score.ClassTranscriptDetailModel.Count; // Mỗi cột điểm sẽ có 1 ô cho tên và 1 ô cho giá trị
                            sheet2.Cells[rowIndex_sh2, 1, rowIndex_sh2, mergeColumns].Merge = true;

                            rowIndex_sh2++; // Xuống dòng mới để ghi chi tiết các cột điểm

                            // Ghi tên các cột điểm
                            foreach (var detail in score.ClassTranscriptDetailModel)
                            {
                                var columnCell = sheet2.Cells[rowIndex_sh2, column_sh2];
                                columnCell.Value = detail.Name;

                                // Thay đổi định dạng của ô tên cột điểm
                                columnCell.Style.Fill.PatternType = ExcelFillStyle.Solid; // Đặt PatternType trước khi đặt màu nền
                                columnCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DDEBF7")); // Màu nền

                                column_sh2++;
                            }

                            rowIndex_sh2++; // Xuống dòng để ghi giá trị của cột điểm
                            column_sh2 = 1; // Reset lại cột để ghi giá trị

                            // Ghi các giá trị điểm tương ứng
                            foreach (var detail in score.ClassTranscriptDetailModel)
                            {
                                var valueCell = sheet2.Cells[rowIndex_sh2, column_sh2];
                                valueCell.Value = detail.Value;

                                column_sh2++;
                            }

                            rowIndex_sh2++; // Xuống dòng sau khi ghi xong các cột điểm
                            rowIndex_sh2++; // Tăng thêm 1 dòng để cách nhau giữa các bảng điểm
                            column_sh2 = 1; // Reset cột về lại cột đầu tiên
                        }
                    }
                    xlPackage.SaveAs(stream);
                }
            }
        }

        public static async Task<string> ExportExcelNotification(StudentLearningOutcomesDTO items, string templateName, string fileNameToSave, string folderToSave)
        {
            byte[] bytes;
            var env = WebHostEnvironment.Environment;
            var templatePath = Path.Combine(env.ContentRootPath, "Template", templateName);
            using (var stream = new MemoryStream())
            {
                await FetchNotification(stream, templatePath, items);
                bytes = stream.ToArray();
            }
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string guid = Guid.NewGuid().ToString();
            string nameFileExport = $"{fileNameToSave}_{timestamp}_{guid}.xlsx";
            var path = Path.Combine(env.ContentRootPath, "Download", folderToSave, nameFileExport);
            File.WriteAllBytes(Path.Combine(path), bytes);
            return path;
        }
        #endregion
    }
}