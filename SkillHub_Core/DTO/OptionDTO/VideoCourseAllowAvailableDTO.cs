using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;
using SkillHub_Core.DTO.Domain;

namespace LMS_Project.DTO.OptionDTO
{
    public class VideoCourseAllowAvailableDTO : DomainOptionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public VideoCourseAllowAvailableDTO() : base() { }
        public VideoCourseAllowAvailableDTO(object model) : base(model) { }
    }
}