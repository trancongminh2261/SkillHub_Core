﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using System.Xml;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using LMSCore;
using Microsoft.Extensions.Configuration;
using LMSCore.Utilities;
using QRCoder;
using System.Drawing.Imaging;
using RestSharp;

namespace LMSCore.LMS
{
    public class AssetCRM
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        public static string fromAddress = configuration.GetSection("MySettings:FromAddress").Value.ToString();
        public static string mailPassword = configuration.GetSection("MySettings:MailPassword").Value.ToString();

        /// <summary>
        /// Tạo chuỗi ký tự gồm số
        /// </summary>
        /// <param name="numberrandom">ĐỘ dài kí tự</param>
        /// <returns></returns>
        public static string RandomString(int numberrandom)
        {
            //var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var chars = "0123456789";
            var stringChars = new char[numberrandom];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
        public static string RandomStringWithText(int numberrandom)
        {
            //var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[numberrandom];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
        /// <summary>
        /// Kiểm tra định dạng email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        //public static bool checkFotmatEmail(string email)
        //{
        //    string fotmat = "@gmail.com";
        //    if(email.Contains(fotmat) == true) 
        //    return true;
        //    return false;
        //}
        /// <summary>
        /// Kiểm tra định dạnh số điện thoại
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool checkFotmatUserNumber(string number)
        {
            try
            {
                if (number.Count() < 10 || number.Count() > 12)
                    return false;
                double check = double.Parse(number);
                return true;
            }
            catch { return false; }
        }
        public static string UrlBeauty(string title)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            title = rgx.Replace(title, "-");
            return title;
        }
        public static void SendMailAmazone(string strTo, string title, string content)
        {
            try
            {


                string fromAddressAmazon = "AKIAQAFF5ORWHYZB7EMW";
                string mailPasswordAmazon = "BIY2OIqJAPEPIdOaDbiiB/AgNC/rgXId9+GcGx9gXb1i";
                // Replace sender@example.com with your "From" address. 
                // This address must be verified with Amazon SES.
                String FROM = "admin@zim.vn";
                String FROMNAME = "app.zim.vn";

                // Replace smtp_username with your Amazon SES SMTP user name.
                String SMTP_USERNAME = fromAddressAmazon;

                // Replace smtp_password with your Amazon SES SMTP password.
                String SMTP_PASSWORD = mailPasswordAmazon;

                // (Optional) the name of a configuration set to use for this message.
                // If you comment out this line, you also need to remove or comment out
                // the "X-SES-CONFIGURATION-SET" header below.
                //String CONFIGSET = "ConfigSet";

                // If you're using Amazon SES in a region other than US West (Oregon), 
                // replace email-smtp.us-west-2.amazonaws.com with the Amazon SES SMTP  
                // endpoint in the appropriate AWS Region.
                String HOST = "email-smtp.us-east-2.amazonaws.com";

                // The port you will connect to on the Amazon SES SMTP endpoint. We
                // are choosing port 587 because we will use STARTTLS to encrypt
                // the connection.
                int PORT = 587;

                // The subject line of the email
                String SUBJECT = title;

                // The body of the email
                String BODY = content;

                // Create and build a new MailMessage object
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                message.From = new MailAddress(FROM, FROMNAME);
                message.To.Add(new MailAddress(strTo));

                message.Subject = SUBJECT;
                message.Body = BODY;
                // Comment or delete the next line if you are not using a configuration set
                //message.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);

                using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
                {
                    // Pass SMTP credentials
                    client.Credentials =
                        new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);
                    // Enable SSL encryption
                    client.EnableSsl = true;
                    client.Send(message);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void SendMail(string strTo, string title, string content, string filePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(mailPassword))
                    return;

                // Create smtp connection.
                SmtpClient client = new SmtpClient();
                client.Port = 587;//outgoing port for the mail.
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Timeout = 200000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(fromAddress, mailPassword);

                // Fill the mail form.
                var send_mail = new MailMessage();
                send_mail.IsBodyHtml = true;
                //address from where mail will be sent.
                send_mail.From = new MailAddress(fromAddress);
                //address to which mail will be sent.           
                send_mail.To.Add(new MailAddress(strTo));
                //subject of the mail.
                send_mail.Subject = title;
                send_mail.Body = content;
                // Nếu có đính kèm file Path của file bất kì thì sẽ gửi luôn file cùng eamil.
                if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                {
                    var attachment = new Attachment(filePath);
                    send_mail.Attachments.Add(attachment);
                }
                client.Send(send_mail);
            }
            catch
            {
                return;
            }
        }
        /// <summary>
        /// tạo qr code
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CreateQRCode(string data, string name)
        {
            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
            //System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
            //imgBarCode.Height = 150;
            //imgBarCode.Width = 150;
            //using (Bitmap bitMap = qrCode.GetGraphic(20))
            //{
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //        byte[] byteImage = ms.ToArray();
            //        imgBarCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);

            //        Convert Base64 Encoded string to Byte Array.
            //        byte[] imageBytes = Convert.FromBase64String(Convert.ToBase64String(byteImage));
            //        string filePath = System.Web.HttpContext.Current.Server.MapPath("~/Upload/qrcode/" + name + ".jpg");
            //        File.WriteAllBytes(filePath, imageBytes);
            //    }
            //    return "/Upload/qrcode/" + name + ".jpg";
            //}
            return string.Empty;
        }

        public static string CreateQRCodeV2(string data, string nameFile, string folderToSave, string baseUrl)
        {
            var appRootPath = WebHostEnvironment.Environment.ContentRootPath;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
            QRCode qrCode = new QRCode(qrCodeData);

            using (Bitmap bitMap = qrCode.GetGraphic(20))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitMap.Save(ms, ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();
                    string base64String = Convert.ToBase64String(byteImage);

                    // Convert Base64 Encoded string to Byte Array.
                    byte[] imageBytes = Convert.FromBase64String(base64String);
                    string folderPath = Path.Combine(appRootPath, "Upload", folderToSave);
                    // Nếu chưa có folder thì tạo folder để lưu
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    // Lưu ảnh với định dạng jpg
                    var path = Path.Combine(appRootPath, "Upload", folderToSave, nameFile + ".jpg");
                    File.WriteAllBytes(Path.Combine(path), imageBytes);

                    // Trả về link ảnh QRCode
                    string link = baseUrl + "/Upload/" + folderToSave + "/" + nameFile + ".jpg";
                    return link.IndexOf("https") == -1 ? link.Replace("http", "https") : link;
                }
            }
        }

        /// <summary>
        /// Chuyển số tiền sang chữ
        /// </summary>
        /// <param name="total">Số tiền cần đọc</param>
        /// <returns></returns>
        public static string MoneyToText(int total)
        {
            try
            {
                string rs = "";
                string[] ch = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
                string[] rch = { "lẻ", "mốt", "", "", "", "lăm" };
                string[] u = { "", "mươi", "trăm", "ngàn", "", "", "triệu", "", "", "tỷ", "", "", "ngàn", "", "", "triệu" };
                string nstr = total.ToString();

                int[] n = new int[nstr.Length];
                int len = n.Length;
                for (int i = 0; i < len; i++)
                {
                    n[len - 1 - i] = Convert.ToInt32(nstr.Substring(i, 1));
                }

                for (int i = len - 1; i >= 0; i--)
                {
                    if (i % 3 == 2)// số 0 ở hàng trăm
                    {
                        if (n[i] == 0 && n[i - 1] == 0 && n[i - 2] == 0) continue;//nếu cả 3 số là 0 thì bỏ qua không đọc
                    }
                    else if (i % 3 == 1) // số ở hàng chục
                    {
                        if (n[i] == 0)
                        {
                            if (n[i - 1] == 0) { continue; }// nếu hàng chục và hàng đơn vị đều là 0 thì bỏ qua.
                            else
                            {
                                rs += " " + rch[n[i]]; continue;// hàng chục là 0 thì bỏ qua, đọc số hàng đơn vị
                            }
                        }
                        if (n[i] == 1)//nếu số hàng chục là 1 thì đọc là mười
                        {
                            rs += " mười"; continue;
                        }
                    }
                    else if (i != len - 1)// số ở hàng đơn vị (không phải là số đầu tiên)
                    {
                        if (n[i] == 0)// số hàng đơn vị là 0 thì chỉ đọc đơn vị
                        {
                            if (i + 2 <= len - 1 && n[i + 2] == 0 && n[i + 1] == 0) continue;
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 1)// nếu là 1 thì tùy vào số hàng chục mà đọc: 0,1: một / còn lại: mốt
                        {
                            rs += " " + ((n[i + 1] == 1 || n[i + 1] == 0) ? ch[n[i]] : rch[n[i]]);
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 5) // cách đọc số 5
                        {
                            if (n[i + 1] != 0) //nếu số hàng chục khác 0 thì đọc số 5 là lăm
                            {
                                rs += " " + rch[n[i]];// đọc số 
                                rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                                continue;
                            }
                        }
                    }
                    rs += (rs == "" ? " " : ", ") + ch[n[i]];// đọc số
                    rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                }
                if (rs[rs.Length - 1] != ' ')
                    rs += " đồng";
                else
                    rs += "đồng";

                if (rs.Length > 2)
                {
                    string rs1 = rs.Substring(0, 2);
                    rs1 = rs1.ToUpper();
                    rs = rs.Substring(2);
                    rs = rs1 + rs;
                }
                //return rs.Trim().Replace("lẻ,", "lẻ").Replace("mươi,", "mươi").Replace("trăm,", "trăm").Replace("mười,", "mười");
                return rs.Trim().Replace(",", "");
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Tạo mã màu random
        /// </summary>
        /// <returns></returns>
        public static string RandomColor()
        {
            var random = new Random();
            var CustomerColor = String.Format("#{0:X6}", random.Next(0x1000000));
            string color = CustomerColor.ToString();
            return color;
        }

        public static string RemoveHTMLTags(string content)
        {
            var cleaned = string.Empty;
            try
            {
                StringBuilder textOnly = new StringBuilder();
                using (var reader = XmlNodeReader.Create(new System.IO.StringReader("<xml>" + content + "</xml>")))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Text)
                            textOnly.Append(reader.ReadContentAsString());
                    }
                }
                cleaned = textOnly.ToString();
            }
            catch
            {
                //A tag is probably not closed. fallback to regex string clean.
                string textOnly = string.Empty;
                Regex tagRemove = new Regex(@"<[^>]*(>|$)");
                Regex compressSpaces = new Regex(@"[\s\r\n]+");
                textOnly = tagRemove.Replace(content, string.Empty);
                textOnly = compressSpaces.Replace(textOnly, " ");
                cleaned = textOnly;
            }

            return cleaned;
        }
        /// <summary>
        /// Xóa dấu tiếng việt
        /// </summary>
        /// <param name="text">chuỗi cần xóa</param>
        /// <returns></returns>
        public static string RemoveUnicode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
            "í","ì","ỉ","ĩ","ị",
            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
            "ý","ỳ","ỷ","ỹ","ỵ",};

            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
            "d",
            "e","e","e","e","e","e","e","e","e","e","e",
            "i","i","i","i","i",
            "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
            "u","u","u","u","u","u","u","u","u","u","u",
            "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }
        public static string RemoveUnicodeAndSpace(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
            "í","ì","ỉ","ĩ","ị",
            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
            "ý","ỳ","ỷ","ỹ","ỵ",};

            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
            "d",
            "e","e","e","e","e","e","e","e","e","e","e",
            "i","i","i","i","i",
            "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
            "u","u","u","u","u","u","u","u","u","u","u",
            "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(" ", "");
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }
        public static bool CheckUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
            "í","ì","ỉ","ĩ","ị",
            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
            "ý","ỳ","ỷ","ỹ","ỵ",};
            bool check = false;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (text.Contains(arr1[i]))
                    check = true;
            }
            return check;
        }
        public static bool IsValidDocument(string flType)
        {
            bool isvalid = false;
            if (flType == ".jpg" || flType == ".jpeg" || flType == ".png" || flType == ".bmp" || flType == ".mp4"
                || flType == ".flv" || flType == ".mpeg" || flType == ".mov" || flType == ".mp3" || flType == ".doc"
                || flType == ".docx" || flType == ".pdf" || flType == ".csv" || flType == ".xlsx" || flType == ".xls"
                || flType == ".ppt" || flType == ".pptx" || flType == ".zip" || flType == ".rar" || flType == ".wav" 
                || flType == ".mkv" || flType == ".txt")
            {
                isvalid = true;
            }
            return isvalid;
        }
        public static bool IsValidImage(string flType)
        {
            bool isvalid = false;
            if (flType == ".jpg" || flType == ".jpeg" || flType == ".png")
            {
                isvalid = true;
            }
            return isvalid;
        }

        public static bool IsValidAudio(string flType) => flType == ".aac " || flType == ".flac " || flType == ".wav " || flType == ".wma" || flType == ".mp3" || flType == ".m4a";

        //Push noti desktop
        public class jsonNoti
        {
            public string title { get; set; }
            public string message { get; set; }
            public string icon { get; set; }
            public string link { get; set; }
        }
        public static string vapIdPublicKey = "BLV67mH2vJ089lrdChQhSzSwJgWXvpKBdwgZ-AzuDpmlKGlZPtCbH_AD28gDnd7u42srBlEQLmbRYf46thgGIzI";
        public static string vapIdPrivateKey = "RTQLMmzY3Ey72ELoJpW-_gDJbC-v_sG7d8r9JKalM0c";
        #region Kiểm tra có phải là hình hay ko
        private enum ImageFileExtension
        {
            none = 0,
            jpg = 1,
            jpeg = 2,
            bmp = 3,
            gif = 4,
            png = 5
        }
        public enum FileType
        {
            Image = '1',
            VIdeo = '2',
            PDF = '3',
            Text = '4',
            DOC = '5',
            DOCX = '6',
            PPT = '7',
        }

        public class ClientWriteLogModel
        {
            public string API { get; set; }
            public int UserId { get; set; }
            public string Error { get; set; }
        }
        public static string ClientWriteLog(ClientWriteLogModel item)
        {
            string name = DateTime.Now.ToString("dd-MM-yyyy");
            string content = "API: " + item.API + "    UserId: " + item.UserId + "    MessageError: " + item.Error + "    " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "" + Environment.NewLine;
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "/FileLog/ClientLog" + name + ".txt";
            File.AppendAllText(filePath, content);
            return content;
        }
        public static bool isValIdFile(byte[] bytFile, string flType, String FileContentType)
        {
            bool isvalId = false;
            if (flType == ".jpg" || flType == ".jpeg" || flType == ".png")
            {
                isvalId = isValIdImageFile(bytFile, FileContentType);//we are going call this method
            }
            //else if (flType == FileType.VIdeo)
            //{
            //    isvalId = isValIdVIdeoFile(bytFile, FileContentType);
            //}
            //else if (flType == FileType.PDF)
            //{
            //    isvalId = isValIdPDFFile(bytFile, FileContentType);
            //}
            return isvalId;
        }
        public static bool isValIdFileCustom(string flType)
        {
            bool isvalId = false;
            if (flType == ".jpg" || flType == ".jpeg" || flType == ".png" || flType == ".bmp")
            {
                isvalId = true;
            }
            return isvalId;
        }
        public static bool isValIdImageAndVIdeo(string flType)
        {
            bool isvalId = false;
            if (flType == ".jpg" || flType == ".jpeg" || flType == ".png" || flType == ".bmp" || flType == ".mp4" || flType == ".flv" || flType == ".mpeg" || flType == ".mov")
            {
                isvalId = true;
            }
            return isvalId;
        }
        public static bool isValIdImageAndVIdeoAndAudio(string flType)
        {
            bool isvalId = false;
            if (flType == ".jpg"
                || flType == ".jpeg"
                || flType == ".png"
                || flType == ".bmp"
                || flType == ".mp4"
                || flType == ".flv"
                || flType == ".mpeg"
                || flType == ".mov"
                || flType == ".wmv"
                || flType == ".wav"
                || flType == ".mp3")
            {
                isvalId = true;
            }
            return isvalId;
        }
        public static bool isValIdDocument(string flType)
        {
            bool isvalId = false;
            if (flType == ".jpg" || flType == ".jpeg" || flType == ".png" || flType == ".bmp" || flType == ".mp4"
                || flType == ".flv" || flType == ".mpeg" || flType == ".mov" || flType == ".mp3" || flType == ".doc"
                || flType == ".docx" || flType == ".pdf" || flType == ".csv" || flType == ".xlsx" || flType == ".xls"
                || flType == ".ppt" || flType == ".pptx" || flType == ".zip" || flType == ".rar")
            {
                isvalId = true;
            }
            return isvalId;
        }
        public static bool isValIdExcel(string flType)
        {
            bool isvalId = false;
            if (flType == ".xlsx" || flType == ".xls")
            {
                isvalId = true;
            }
            return isvalId;
        }
        public static bool isValIdZip(string flType)
        {
            bool isvalId = false;
            if (flType == ".zip")
            {
                isvalId = true;
            }
            return isvalId;
        }
        public static bool isValIdImageFile(byte[] bytFile, String FileContentType)
        {
            bool isvalId = false;

            byte[] chkBytejpg = { 255, 216, 255, 224 };
            byte[] chkBytebmp = { 66, 77 };
            byte[] chkBytegif = { 71, 73, 70, 56 };
            byte[] chkBytepng = { 137, 80, 78, 71 };

            ImageFileExtension imgfileExtn = ImageFileExtension.none;

            if (FileContentType.Contains("jpg") | FileContentType.Contains("jpeg"))
            {
                imgfileExtn = ImageFileExtension.jpg;
            }
            else if (FileContentType.Contains("png"))
            {
                imgfileExtn = ImageFileExtension.png;
            }
            else if (FileContentType.Contains("bmp"))
            {
                imgfileExtn = ImageFileExtension.bmp;
            }
            else if (FileContentType.Contains("gif"))
            {
                imgfileExtn = ImageFileExtension.gif;
            }

            if (imgfileExtn == ImageFileExtension.jpg || imgfileExtn == ImageFileExtension.jpeg)
            {
                if (bytFile.Length >= 4)
                {
                    int j = 0;
                    for (Int32 i = 0; i <= 3; i++)
                    {
                        if (bytFile[i] == chkBytejpg[i])
                        {
                            j = j + 1;
                            if (j == 3)
                                isvalId = true;
                        }
                    }
                }
            }
            if (imgfileExtn == ImageFileExtension.png)
            {
                if (bytFile.Length >= 4)
                {
                    int j = 0;
                    for (Int32 i = 0; i <= 3; i++)
                    {
                        if (bytFile[i] == chkBytepng[i])
                        {
                            j = j + 1;
                            if (j == 3)
                                isvalId = true;
                        }
                    }
                }
            }
            if (imgfileExtn == ImageFileExtension.bmp)
            {
                if (bytFile.Length >= 4)
                {
                    int j = 0;
                    for (Int32 i = 0; i <= 1; i++)
                    {
                        if (bytFile[i] == chkBytebmp[i])
                        {
                            j = j + 1;
                            if (j == 2)
                                isvalId = true;
                        }
                    }
                }
            }

            if (imgfileExtn == ImageFileExtension.gif)
            {
                if (bytFile.Length >= 4)
                {
                    int j = 0;
                    for (Int32 i = 0; i <= 1; i++)
                    {
                        if (bytFile[i] == chkBytegif[i])
                        {
                            j = j + 1;
                            if (j == 3)
                                isvalId = true;
                        }
                    }
                }
            }
            return isvalId;
        }

        #endregion

        /// <summary>
        /// Ghi file log lỗi hằng ngày
        /// </summary>
        /// <param name="page">Trag lỗi, controller</param>
        /// <param name="function">Hàm, view lỗi</param>
        /// <param name="loginUId">Id đăng đăng nhập</param>
        /// <param name="contenterror">Thông báo lỗi</param>
        public static void Writelog(string page, string function, int loginUId, string contenterror)
        {
            string name = GetDateTime.Now.ToString("dd-MM-yyyy");
            string content = "PageError: " + page + "    FunctionError: " + function + "    loginUId: " + loginUId + "    MessageError: " + contenterror + "    " + GetDateTime.Now.ToString("dd/MM/yyyy HH:mm") + "" + Environment.NewLine;
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "/FileLog/log" + name + ".txt";
            File.AppendAllText(filePath, content);
        }
        //public static int GetVIdeoDuration(string filePath)
        //{
        //    //var player = new WindowsMediaPlayer();
        //    //var clip = player.newMedia(filePath);
        //    //TimeSpan span = TimeSpan.FromSeconds(clip.duration);
        //    //int totalMinutes = Math.Round(span.TotalMinutes, 0).ToString().ToInt(0);
        //    //return totalMinutes;

        //    //// Microsoft.WindowsAPICodePack - Shell NuGet package
        //    //using (var shell = ShellObject.FromParsingName(filePath))
        //    //{
        //    //    IShellProperty prop = shell.Properties.System.Media.Duration;
        //    //    var t = (ulong)prop.ValueAsObject;
        //    //    TimeSpan span = TimeSpan.FromTicks((long)t);
        //    //    int totalMinutes = Math.Round(span.TotalMinutes, 0).ToString().ToInt(0);
        //    //    return totalMinutes;
        //    //}

        //    TagLib.File file = TagLib.File.Create(filePath);
        //    int s_time = (int)file.Properties.Duration.TotalSeconds;
        //    int s_minutes = s_time / 60;
        //    int s_seconds = s_time % 60;
        //    if (s_seconds > 20)
        //        s_minutes += 1;
        //    return s_minutes;
        //}
        public static void OneSignalPushNotifications(OneSignalRequest oneSignalRequest)
        {
            try
            {
                if (!string.IsNullOrEmpty(oneSignalRequest.PlayerId))
                {
                    oneSignalRequest.Content = oneSignalRequest.Content.Replace("&nbsp;", " ");
                    //string onesignalAppId = ConfigurationManager.AppSettings["OnesignalAppId"].ToString();
                    //string onesignalRestId = ConfigurationManager.AppSettings["OnesignalRestId"].ToString();
                    string onesignalAppId = "";
                    string onesignalRestId = "";

                    var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

                    request.KeepAlive = true;
                    request.Method = "POST";
                    request.ContentType = "application/json; charset=utf-8";

                    request.Headers.Add("authorization", "Basic " + onesignalRestId);

                    //var serializer = new JavaScriptSerializer();
                    var obj = new
                    {
                        app_Id = onesignalAppId,
                        headings = new { en = oneSignalRequest.Headings },
                        contents = new { en = oneSignalRequest.Content },
                        channel_for_external_user_Ids = "push",
                        url = oneSignalRequest.Url,
                        include_player_Ids = new string[] { oneSignalRequest.PlayerId }//Gửi cho user đc chỉ định
                                                                                       //included_segments = new string[] { "Subscribed Users" } //Gửi cho tất cả user nào đăng ký
                    };
                    //var param = serializer.Serialize(obj);
                    var param = JsonConvert.SerializeObject(obj);
                    byte[] byteArray = Encoding.UTF8.GetBytes(param);

                    string responseContent = null;

                    try
                    {
                        using (var writer = request.GetRequestStream())
                        {
                            writer.Write(byteArray, 0, byteArray.Length);
                        }

                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseContent = reader.ReadToEnd();
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
                    }
                    System.Diagnostics.Debug.WriteLine(responseContent);
                }
            }
            catch { }
        }
        public static async Task WriteLog(LogModel logModel)
        {
            await Task.Run(() =>
            {
                string name = GetDateTime.Now.ToString("dd-MM-yyyy");
                string content = "PageError: " + logModel.page + "    FunctionError: " + logModel.function + "    loginUId: " + logModel.currentUserId + "    ContentError: " + logModel.contentError + "    Exception: " + logModel.exception + "    " + GetDateTime.Now.ToString("dd/MM/yyyy HH:mm") + "" + Environment.NewLine;
                string filePath = AppDomain.CurrentDomain.BaseDirectory + "/FileLog/log" + name + ".txt";
                File.AppendAllText(filePath, content);
            });
        }
        public class LogModel
        {
            public string page { get; set; }
            public string function { get; set; }
            public int currentUserId { get; set; }
            public string contentError { get; set; }
            public Exception exception { get; set; }
        }
        public static async Task SendMailAsync(string strTo, string title, string content)
        {
            try
            {
                await Task.Run(() =>
                {
                    //string bd = "PCFET0NUWVBFIGh0bWw+CjxodG1sIGxhbmc9InZpIj4KICA8aGVhZD4KICAgIdxtZXRhIGNoYXJzZXQ9IlVURi04IiAvPgogICAgPG1ldGEKICAgICAgbmFtZT0Idmlld3BvcnQiCiAgICAgIGNvbnRlbnQ9IndpZHRoPWRldmljZS13aWR0aCwgaW5pdGlhbC1zY2FsZT0xLCBtYXhpbXVtLXNjYWxlPTEiCiAgICAvPgogICAgPHRpdGxlPktJV0k8L3RpdGxlPgogIdwvaGVhZD4KCiAgPGJvZHk+CiAgICA8dGFibGUKICAgICAgd2lkdGg9IjEwMCUiCiAgICAgIHN0eWxlPSJtYXJnaW46MCBhdXRvO21heC13aWR0aDo2MDBweCA7Zm9udC1zaXplOjE0cHg7IGJvcmRlci1jb2xsYXBzZTpjb2xsYXBzZTtmb250LWZhbWlseTogQXJpYWwsIEhlbHZldGljYSwgc2Fucy1zZXJpZiI7PgogICAgICA8dGhlYWQ+CiAgICAgICAgPHRyIHN0eWxlPSJiYWNrZ3JvdW5kLWNvbG9yOiMxYTJlNDYiPgogICAgICAgICAgPHRoPgogICAgICAgICAgICA8aW1nIHNyYz0iaHR0cHM6Ly9raXdpLm1vbmFtZWRpYS5uZXQvQXBwX1RoZW1lcy9LSVdJL2ltYWdla2l3aS9raXdpbm90aS5wbmciIHdpZHRoPSI4MCIvPgogICAgICAgICAgPC90aD4KICAgICAgICAgIdx0aD48aDMgc3R5bGU9ImNvbG9yOiNmY2RiMzM7Ij5LSMOUSSBQSOG7pEMgTeG6rFQgS0jhuqhVPC9oMz48L3RoPgogICAgICAgIdwvdHI+CiAgICAgIdwvdGhlYWQ+CiAgICAgIdx0Ym9keSBzdHlsZT0iYmFja2dyb3VuZC1jb2xvcjojZWZlZmVmOyI+CiAgICAgICAgPHRyPgogICAgICAgICAgPHRkIGNvbHNwYW49IjIiIHN0eWxlPSJwYWRkaW5nOjAgMTVweDsiPgogICAgICAgICAgICA8cD5QYXNzd29yZDogTUFUS0hBVU1PSTwvcD4KCQkJPHA+VnVpIGzDsm5nIHRydXkgY+G6rXAgdOG6oWkgPGEgaHJlZj0iaHR0cHM6Ly9hcHAubWFhc2VkdS5jb20vIiBzdHlsZT0IdGV4dC1kZWNvcmF0aW9uOiBub25lOyBjb2xvcjogYmx1ZTsiIHRhcmdldD0iX2JsYW5rIj5hcHAubWFhc2VkdS5jb208L2E+IMSR4buDIMSR4buVaSBt4bqtdCBraOG6qXUgbeG7m2kuPC9wPgogICAgICAgICAgPC90ZD4KICAgICAgICA8L3RyPgogICAgICAgIAogICAgICAgIdx0cj4KICAgICAgICAgIdx0ZCBjb2xzcGFuPSIyIiBzdHlsZT0IdGV4dC1hbGlnbjogY2VudGVyOyBwYWRkaW5nOjA7Ij4KICAgICAgICAgICAgPGgzPkPDoW0gxqFuIHF1w70ga2jDoWNoIMSRw6Mgc+G7rSBk4bulbmcgZOG7i2NoIHbhu6UgY+G7p2EgY2jDum5nIHTDtGkuPC9oMz4KICAgICAgICAgIdwvdGQ+CiAgICAgICAgPC90cj4KICAgICAgICA8dHIgc3R5bGU9ImJhY2tncm91bmQtY29sb3I6IzFhMmU0NjsgY29sb3I6d2hpdGUiPgogICAgICAgICAgPHRkIGNvbHNwYW49IjIiIHN0eWxlPSJwYWRkaW5nOjE1cHg7Ij4KICAgICAgICAgICAgPGRpdj4KICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6IGJvbGQiPkhlYWQgT2ZmaWNlOjwvc3Bhbj48c3Bhbj4gVmluaG9tZXMgQ2VudHJhbCBQYXJrLCAyMDggTmd1eWVuIEh1dSBDYW5oLCBXYXJkIdIyLCBEaXN0IEJpbmggVGhhbmgsIEhDTUMgfCBUZWw6IdA5NzkgNDIyIdM5Mzwvc3Bhbj4KICAgICAgICAgICAgPC9kaXY+CiAgICAgICAgICAgIdxkaXYgc3R5bGU9Im1hcmdpbi10b3A6NXB4OyI+CiAgICAgICAgICAgICAgV2Vic2l0ZToKICAgICAgICAgICAgICA8YSBocmVmPSJodHRwczovL21hYXNlZHUuY29tLyIgdGFyZ2V0PSJfYmxhbmsiIHN0eWxlPSJ0ZXh0LWRlY29yYXRpb246IG5vbmU7Y29sb3I6IGFxdWE7Ij5tYWFzZWR1LmNvbTwvYT4JCQkgIAogICAgICAgICAgICAgIHwgSG90bGluZSAyNC83OiA8YSBocmVmPSJ0ZWw6Kzg0OTc5NDIyMzkzIiBzdHlsZT0IdGV4dC1kZWNvcmF0aW9uOiBub25lO2NvbG9yOiBhcXVhOyI+MDk3OSA0MjIgMzkzPC9hPgogICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgIdwvdGQ+CiAgICAgICAgPC90cj4KICAgICAgPC90Ym9keT4KICAgIdwvdGFibGU+CiAgPC9ib2R5Pgo8L2h0bWw+";
                    //var base64EncodedBytes = System.Convert.FromBase64String(bd);
                    //string content = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                    //content = content.Replace("MATKHAUMOI", MATKHAUMOI);
                    // Create smtp connection.
                    SmtpClient client = new SmtpClient();
                    client.Port = 587;//outgoing port for the mail.
                    client.Host = "smtp.gmail.com";
                    client.EnableSsl = true;
                    client.Timeout = 1000000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(fromAddress, mailPassword);

                    // Fill the mail form.
                    //var send_mail = new MailMessage();
                    //send_mail.IsBodyHtml = true;
                    ////address from where mail will be sent.
                    //send_mail.From = new MailAddress(fromAddress);
                    ////address to which mail will be sent.           
                    //send_mail.To.Add(new MailAddress(strTo));
                    ////subject of the mail.
                    //send_mail.Subject = title;
                    //send_mail.Body = content;
                    //client.Send(send_mail);
                });
            }
            catch (Exception exception)
            {
                await AssetCRM.WriteLog(new AssetCRM.LogModel() { function = "SendMailAsync", exception = exception });
                throw exception;
            }
        }
        public static string InitCode(string baseCode, DateTime time, int count)
        {
            string result = baseCode;
            string year = time.Year.ToString().Substring(2, 2);
            string month = time.Month < 10 ? $"0{time.Month}" : time.Month.ToString();
            string day = time.Day < 10 ? $"0{time.Day}" : time.Day.ToString();
            string stt = count < 10 ? $"000{count}"
                            : count < 100 ? $"00{count}"
                            : count < 1000 ? $"0{count}"
                            : count.ToString();
            return result + year + month + day + stt;
        }
    }
}