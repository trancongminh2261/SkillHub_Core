using LMSCore.DTO.BranchDTO;
using LMSCore.Models;
using System;
using System.Collections.Generic;

namespace LMSCore.DTO.PopupConfigDTO
{
    public class GetPopupConfigDTO : DomainEntity
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime? STime { get; set; }
        public DateTime? ETime { get; set; }
        public double? Durating { get; set; } // SECOND
        public string Url { get; set; }
        public bool IsShow { get; set; } = false;
        public string BranchIds { get; set; }
        public List<GetBranchesDTO> Branches { get; set; }
        public int TotalRow { get; set; }
        public GetPopupConfigDTO() : base() { }
        public GetPopupConfigDTO(object model) : base(model) { }
    }
}
