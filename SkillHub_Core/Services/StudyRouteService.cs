using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using LMSCore.DTO.StudyRoute;

namespace LMSCore.Services
{
    public class StudyRouteService
    {
        public static async Task<List<tbl_StudyRoute>> Insert(StudyRouteCreate request, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                tbl_UserInformation student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == request.StudentId && x.Enable == true);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var programs = await db.tbl_Program.Where(x => ("," + request.ProgramIds + ",").Contains("," + x.Id + ",") && x.Enable == true).ToListAsync();
                if (!programs.Any())
                    throw new Exception("Chương trình không tồn tại");
                int index = 1;
                List<tbl_StudyRoute> programOfStudent = await db.tbl_StudyRoute.Where(x => x.StudentId == request.StudentId && x.ProgramId == x.ProgramId && x.Enable == true).ToListAsync();
                if (programOfStudent.Any())
                {
                    index = programOfStudent.OrderByDescending(x => x.Index).Select(x => x.Index).FirstOrDefault() + 1;
                }
                tbl_StudyRoute entity = new tbl_StudyRoute(request);
                entity.CreatedBy = user.FullName;
                entity.CreatedBy = user.FullName;
                entity.Status = (int)StudyRouteStatus.ChuaHoc;
                entity.StatusName = ListStudyRouteStatus().Where(x => x.Key == entity.Status).FirstOrDefault().Value;
                var addList = new List<tbl_StudyRoute>();
                foreach (var program in programs.OrderBy(x => x.Index))
                {
                    entity.Id = 0;
                    entity.Index = index;
                    entity.ProgramId = program.Id;
                    db.tbl_StudyRoute.Add(entity);
                    await db.SaveChangesAsync();
                    addList.Add(entity);
                    index++;
                }
                return addList;
            }
        }
        public static async Task<tbl_StudyRoute> Update(StudyRouteUpdate request, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                tbl_StudyRoute studyRoute = await db.tbl_StudyRoute.SingleOrDefaultAsync(x => x.Id == request.Id);
                if (studyRoute == null)
                    throw new Exception("Lộ trình không tồn tại");
                tbl_UserInformation student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == request.StudentId && x.Enable == true);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                tbl_Program program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == request.ProgramId && x.Enable == true);
                if (program == null)
                    throw new Exception("Chương trình không tồn tại");
                studyRoute.StudentId = request.StudentId ?? studyRoute.StudentId;
                studyRoute.ProgramId = request.ProgramId ?? request.ProgramId.Value;
                studyRoute.Status = request.Status ?? studyRoute.Status;
                studyRoute.Note = request.Note ?? studyRoute.Note;
                studyRoute.StatusName = ListStudyRouteStatus().Where(x => x.Key == studyRoute.Status).FirstOrDefault().Value;
                studyRoute.ModifiedBy = user.FullName;
                await db.SaveChangesAsync();
                return studyRoute;
            }
        }
        public static async Task<bool> UpdateIndex(int idUp, int idDown, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var studyRouteUp = await db.tbl_StudyRoute.SingleOrDefaultAsync(x => x.Id == idUp);
                if (studyRouteUp == null)
                    throw new Exception("Lộ trình không tồn tại");
                var studyRouteDown = await db.tbl_StudyRoute.SingleOrDefaultAsync(x => x.Id == idDown);
                if (studyRouteDown == null)
                    throw new Exception("Lộ trình không tồn tại");
                if (studyRouteUp.StudentId != studyRouteDown.StudentId)
                    throw new Exception("Lộ trình không thuộc cùng một học viên");
                int tempIndex = (int)studyRouteUp.Index;
                studyRouteUp.Index = studyRouteDown.Index;
                studyRouteDown.Index = tempIndex;
                studyRouteUp.ModifiedBy = user.FullName;
                studyRouteDown.ModifiedBy = user.FullName;
                db.SaveChanges();
                return true;
            }
        }
        public static async Task<bool> Delete(int id, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                tbl_StudyRoute studyRoute = await db.tbl_StudyRoute.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (studyRoute == null)
                {
                    throw new Exception("Lộ trình không tồn tại");
                }
                studyRoute.Enable = false;
                studyRoute.ModifiedOn = DateTime.Now;
                studyRoute.ModifiedBy = user.FullName;
                await db.SaveChangesAsync();
                return true;
            }
        }
        public static async Task<List<Get_StudyRoute>> GetStudyRouteOfStudent(string studentIds, string parentIds)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_StudyRoute @StudentIds='{studentIds ?? ""}', @ParentIds='{parentIds ?? ""}'";
                var data = await db.SqlQuery<Get_StudyRoute>(sql);

                return data;
            }
        }
        public static async Task<List<Get_SuggestStudyRoute>> SuggestStudyRoute(int studentId)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_SuggestStudyRoute @StudentId='{studentId}'";
                var data = await db.SqlQuery<Get_SuggestStudyRoute>(sql);

                return data;
            }
        }
    }
}