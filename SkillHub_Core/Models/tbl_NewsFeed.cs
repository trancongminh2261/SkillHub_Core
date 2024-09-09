namespace LMSCore.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using System.IO;
    using System.Linq;
    using static LMSCore.Models.lmsEnum;

    public class tbl_NewsFeed : DomainEntity
    {
        /// <summary>
        /// Nội dung Bản tin
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Màu Bản tin
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// Cờ nền
        /// </summary>
        public bool? IsBackGround { get; set; }
        /// <summary>
        /// Nền Bản tin
        /// </summary>
        public string BackGroundUrl { get; set; }
        /// <summary>
        /// Nhóm
        /// </summary>
        public int? NewsFeedGroupId { get; set; }
        /// <summary>
        /// Pin bài viết
        /// </summary>
        public bool Pin { get; set; }
        public DateTime? PinDate { get; set; }
        /// <summary>
        /// Tên nhóm
        /// </summary>
        [NotMapped]
        public string GroupName { get; set; }
        /// <summary>
        /// Danh sách chi nhánh dạng chuỗi
        /// </summary>
        [JsonIgnore]
        public string BranchIds { get; set; }
        [NotMapped]
        public List<int> BranchIdList { get; set; }

        [NotMapped]
        public List<string> BranchNameList { get; set; }
        /// <summary>
        /// Id người tạo
        /// </summary>
        public int? CreatedIdBy { get; set; }
        /// <summary>
        /// Avatar người tạo
        /// </summary>
        [NotMapped]
        public string Avatar { get; set; }

        /// <summary>
        /// Role người tạo
        /// </summary>
        [NotMapped]
        public string RoleName { get; set; }

        /// <summary>
        /// Tên chức vụ trong nhóm
        /// </summary>
        [NotMapped]
        public string TypeNameGroup { get; set; }

        /// <summary>
        /// Cờ kiểm tra account login có like bài tin không  để hiển thị like
        /// </summary>
        [NotMapped]
        public int? IsLike { get; set; }
        /// <summary>
        /// Số lượng bình luận
        /// </summary>
        public int? TotalComment { get; set; }
        /// <summary>
        /// Số lượt yêu thích
        /// </summary>
        public int? TotalLike { get; set; }          
        /// <summary>
        /// Danh sách file dạng chuỗi
        /// </summary>
        [JsonIgnore]
        public string Files { get; set; }
        /// <summary>
        /// Số lượng file
        /// </summary>
        public int? TotalFile { get; set; }
        [NotMapped]
        public List<NewsFeedFileModel> FileList
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<NewsFeedFileModel>>(Files ?? string.Empty);
                }
                catch
                {
                    return null;
                }
            }
        }

        public tbl_NewsFeed() : base() { }
        public tbl_NewsFeed(object model) : base(model) { }
    }

    public class Get_NewsFeed : DomainEntity
    {
        /// <summary>
        /// Nội dung Bản tin
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Màu Bản tin
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// Cờ nền
        /// </summary>
        public bool? IsBackGround { get; set; }
        /// <summary>
        /// Nền Bản tin
        /// </summary>
        public string BackGroundUrl { get; set; }
        /// <summary>
        /// Nhóm
        /// </summary>
        public int? NewsFeedGroupId { get; set; }
        /// <summary>
        /// Tên nhóm
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// Danh sách chi nhánh dạng chuỗi
        /// </summary>
        [JsonIgnore]
        public string BranchIds { get; set; }
        public List<int> BranchIdList
        {
            get
            {
                if (!string.IsNullOrEmpty(BranchIds))
                    return BranchIds.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                else
                    return null;
            }
        }
        [JsonIgnore]
        public string BranchNames { get; set; }
        public List<string> BranchNameList
        {
            get
            {
                if (!string.IsNullOrEmpty(BranchNames) || BranchNames != null)
                    return BranchNames.Split(',').ToList();
                else
                    return null;
            }
        }
        /// <summary>
        /// Id người tạo
        /// </summary>
        public int? CreatedIdBy { get; set; }
        /// <summary>
        /// Avatar người tạo
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// Role người tạo
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Tên chức vụ trong nhóm
        /// </summary>
        public string TypeNameGroup { get; set; }

        /// <summary>
        /// Cờ kiểm tra account login có like bài tin không  để hiển thị like
        /// </summary>
        public int? IsLike { get; set; }
        /// <summary>
        /// Số lượng bình luận
        /// </summary>
        public int? TotalComment { get; set; }
        /// <summary>
        /// Số lượt yêu thích
        /// </summary>
        public int? TotalLike { get; set; }
        /// <summary>
        /// Danh sách file dạng chuỗi
        /// </summary>
        [JsonIgnore]
        public string Files { get; set; }
        /// <summary>
        /// Số lượng file
        /// </summary>
        public int? TotalFile { get; set; }       
        public bool? Pin { get; set; }
        public DateTime? PinDate { get; set; }
        public int? TotalRow { get; set; }
    }

    public class NewsFeedFileModel
    {
        /// <summary>
        /// Đường dẫn file
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// Tên file
        /// </summary>
        public string FileName
        {
            get
            {
                try
                {
                    return Path.GetFileName(FileUrl);
                }
                catch { return null; }

            }
        }
        /// <summary>
        /// Kiểu file
        /// </summary>
        public string FileType
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(FileUrl))
                        return "";
                    var values = FileUrl.Split('.');
                    if (values.Length == 0)
                        return "";
                    return values[values.Length - 1].ToString();
                }
                catch { return null; }

            }
        }
    }
}