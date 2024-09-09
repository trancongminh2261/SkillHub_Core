namespace LMSCore.Models
{
    /// <summary>
    /// Lưu thông tin các bảng sẽ dùng trong import khởi tạo dữ liệu
    /// </summary>
    public class tbl_SaveTableInit : DomainEntity
    {              
        /// <summary>
        /// nơi chưa table
        /// </summary>
        public string NameSpace { get; set; }
        /// <summary>
        /// thông tin tên bảng ví dụ tbl_Area
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// mô tả ví dụ tbl_Area => mô tả là tỉnh/ thành phố
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// bảng này có cần dùng trong import hay không
        /// </summary>
        public bool IsImport { get; set; }
        /// <summary>
        /// vị trị table này trong field import
        /// </summary>
        public int Index { get;set; }
        public tbl_SaveTableInit() : base() { }
        public tbl_SaveTableInit(object model) : base(model) { }
    }
}
