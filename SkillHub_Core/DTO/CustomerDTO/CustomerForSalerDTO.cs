using System;

namespace LMSCore.DTO.CustomerDTO
{
    public class CustomerForSalerDTO
    {
        public int? BranchId { get; set; }
        public string BranchName { get; set; }
        public int? Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public int? CustomerStatusId { get; set; }
        public string CustomerStatusName { get; set; }
        public string SaleCode { get; set; }
        public int? SaleId { get; set; }
        public string SaleName { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        public string LearningNeedName { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public string PurposeName { get; set; }
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>
        public int? SourceId { get; set; }
        public string SourceName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public int TotalRow { get; set; }
    }
}
