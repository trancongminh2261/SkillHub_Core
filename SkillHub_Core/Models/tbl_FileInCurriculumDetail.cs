namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_FileInCurriculumDetail : DomainEntity
    {
        public int? CurriculumDetailId { get; set; }
        public int? Index { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public tbl_FileInCurriculumDetail() : base() { }
        public tbl_FileInCurriculumDetail(object model) : base(model) { }
    }
    public class FileInCurriculumDetailModel : DomainEntity
    {
        public int? CurriculumDetailId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileType
        {
            get
            {
                if (string.IsNullOrEmpty(FileUrl))
                    return "";
                var values = FileUrl.Split('.');
                if (values.Length == 0)
                    return "";
                return values[values.Length - 1].ToString();
            }
        }
        public string FileSize { get;set; }
        public FileInCurriculumDetailModel() : base() { }
        public FileInCurriculumDetailModel(object model) : base(model) { }
    }
}