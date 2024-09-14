using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMSCore.Models
{
    public static class lmsEnum
    {
        public enum UploadType
        {
            Document = 1,
            Image,
            Audio,
        }
        public enum LessonFileType
        {
            FileUpload = 1,
            LinkYoutube,
            AntiDownload
        }
        public enum AnswerEssay
        {
            Exist = 1,
            NotExist
        }
        public enum ExerciseLevel
        {
            Easy = 1,
            Normal,
            Difficult
        }
        public enum ExerciseType
        {
            MultipleChoice = 1,
            DragDrop,
            FillInTheBlank,
            Essay
        }
        public enum SeminarStatus
        {
            ChuaDienRa = 1,
            DangDienRa,
            KetThuc
        }
        public enum RoleEnum
        {
            admin = 1,
            teacher,
            student,
            manager

        }
        public enum AccountStatus
        {
            active,
            inActive
        }
        public enum GenderEnum
        {
            nu = 0,
            nam = 1,
            khac = 2
        }
        public enum AllowRegister
        {
            Allow,
            UnAllow
        }
        public enum ChangeInfoStatus
        {
            Approve,
            Cancel
        }
        public enum LessonType
        {
            Video = 1,
            BaiTap
        }
    }

    public class EnumObject
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

}