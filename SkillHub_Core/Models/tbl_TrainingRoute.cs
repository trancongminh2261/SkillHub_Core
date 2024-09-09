using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static LMSCore.Models.lmsEnum;
namespace LMSCore.Models
{
    public class tbl_TrainingRoute : DomainEntity
    {
        public string Name { get; set; }
        public string CurrentLevel { get; set; }
        public string TargetLevel { get; set; }
        public int Age { get; set; }
        
        public tbl_TrainingRoute() : base() { }
        public tbl_TrainingRoute(object model) : base(model) { }
    }


    public class Get_TrainingRoute : DomainEntity
    {
        public string Name { get; set; }
        public string CurrentLevel { get; set; }
        public string TargetLevel { get; set; }
        public int Age { get; set; }
        public int TotalRow { get; set; }
    }
    public class Get_TrainingRoute_Detail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public int? TrainingRouteDetailId { get; set; }
        public string TrainingRouteDetailSkill { get; set; }
        public int? TrainingRouteDetailLevel { get; set; }
        public int? TrainingRouteDetailIndex { get; set; }
        public bool? TrainingRouteDetailEnable { get; set; }
        public int? IeltsExamId { get; set; }
        public string IeltsExamName { get; set; }
    }
    public class TrainingRouteFormDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public List<TrainingRouteDetailDTO> Details { get; set; }
        public bool Completed {
            get
            {
                if (Details.Count > 0)
                {
                    if (Details.Find(x => x.Completed == false) != null)
                        return false;
                }
                return true;
            }
        }
    }
    public class TrainingRouteDetailDTO
    {
        public int? Id { get; set; }
        public string Skill { get; set; }
        public int? Level { get; set; }
        public int? Index { get; set; }
        public int? IeltsExamId { get; set; }
        public string IeltsExamName { get; set; }
        public bool Completed { get; set; }
    }
}