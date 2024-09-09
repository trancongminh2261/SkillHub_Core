namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_ZoomRoom : DomainEntity
    {
        public int? SeminarId { get; set; }
        [NotMapped]
        public string SeminarName { get; set; }
        public int? LeaderId { get; set; }
        [NotMapped]
        public string LeaderName { get; set; }
        public string RoomId { get; set; }
        public string RoomPass { get; set; }
        public string SignatureTeacher { get; set; }
        public string SignatureStudent { get; set; }
        public int? ZoomConfigId { get; set; }
        public tbl_ZoomRoom() : base() { }
        public tbl_ZoomRoom(object model) : base(model) { }
    }
    public class Get_ZoomActive : DomainEntity
    {
        public int? SeminarId { get; set; }
        public string SeminarName { get; set; }
        public int? LeaderId { get; set; }
        public string LeaderName { get; set; }
        public string RoomId { get; set; }
        public string RoomPass { get; set; }
        public string SignatureTeacher { get; set; }
        public string SignatureStudent { get; set; }
        public int? ZoomConfigId { get; set; }
        public int TotalRow { get; set; }
    }
}