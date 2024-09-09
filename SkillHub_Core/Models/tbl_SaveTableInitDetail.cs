namespace LMSCore.Models
{
    public class tbl_SaveTableInitDetail : DomainEntity
    {
        public int SaveTableInitId { get; set; }
        public string FieldName { get;set; }
        public string FieldType { get;set; }
        public int Index { get;set; }
        public bool IsForeignKey { get;set; }
        public string ForeignTable { get; set; }
        /// <summary>
        /// ý field này là ví dụ nó lưu là Name thì excel khách nhập thông tin vô sẽ đi so sánh với field có tên là Name trong ForeignTable
        /// </summary>
        public string ForeignFieldName { get; set; }
        public tbl_SaveTableInitDetail() : base() { }
        public tbl_SaveTableInitDetail(object model) : base(model) { }
    }
}
