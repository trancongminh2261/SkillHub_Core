using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_CertificateTemplate : DomainEntity
    {
        /// <summary>
        /// Tên mẫu chứng chỉ
        /// </summary>
        public string Name { get; set; }
        /*/// <summary>
        /// Logo
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// Tên người cấp chứng chỉ
        /// </summary>
        public string Issuer { get; set; }*/
        /// <summary>
        /// Nội dung
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Hình nền
        /// </summary>
        public string Background { get; set; }
        /// <summary>
        /// Mặt sau
        /// </summary>
        public string Backside { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public tbl_CertificateTemplate() : base() { }
        public tbl_CertificateTemplate(object model) : base(model) { }
        public static List<CertificateTemplateGuide> GetGuide()
        {
            return new List<CertificateTemplateGuide> {
               new CertificateTemplateGuide { Key = "{MaHocVien}", Value = "Mã học viên"},
               new CertificateTemplateGuide { Key = "{TenHocVien}", Value = "Tên học viên"},
               new CertificateTemplateGuide { Key = "{NgayCap}", Value = "Ngày cấp"},
               new CertificateTemplateGuide { Key = "{ThangCap}", Value = "Tháng cấp"},
               new CertificateTemplateGuide { Key = "{NamCap}", Value = "Năm cấp"},
               new CertificateTemplateGuide { Key = "{Lop}", Value = "Lớp"},
               new CertificateTemplateGuide { Key = "{ChuongTrinh}", Value = "Chương trình"},
               new CertificateTemplateGuide { Key = "{ChuyenMon}", Value = "Chuyên môn"},
            };
        }
    }
    public class CertificateTemplateGuide
    { 
        public string Key { get; set; }
        public string Value { get; set; }
    }
}