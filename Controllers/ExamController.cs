using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly AgialContext context;
        public ExamController(AgialContext _context)
        {
            this.context = _context;
        }

        #region Get all Exams
        [HttpGet]
        public async Task<IActionResult> GetExam()
        {
            List<Exam> exams = await context.exam.Include(e => e.Tech).Include(e=>e.Tech.User).ToListAsync();
            List<Get_ExamDTO> examDTOs = new List<Get_ExamDTO>();
            try
            {
                foreach (Exam item in exams)
                {
                    Get_ExamDTO get_ExamDTO = new Get_ExamDTO();
                    get_ExamDTO.Exam_ID = item.Exam_ID;
                    get_ExamDTO.Exam_Date = item.Exam_Date;
                    get_ExamDTO.Start_Time = item.Start_Time;
                    get_ExamDTO.End_Time = item.End_Time;
                    get_ExamDTO.Min_Degree= item.Min_Degree;
                    get_ExamDTO.Max_Degree= item.Max_Degree;
                    get_ExamDTO.class_name= item.class_name;
                    get_ExamDTO.subject_name= item.subject_name;
                    get_ExamDTO.Teacher_Name = item.Tech.User.Full_Name;
                    examDTOs.Add(get_ExamDTO);
                }
                return Ok(examDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }
        }
        #endregion
        
        #region Get Exam By ID
        [HttpGet("/id/{id}")]
        public async Task<IActionResult> Get_Exam_By_ID(int id)
        {
            var exam = context.exam.Include(e => e.Tech).Include(e => e.Tech.User).FirstOrDefault(s => s.Exam_ID == id);
            if (exam == null)
            {
                return NotFound();
            }
            Get_ExamDTO examDTO = new Get_ExamDTO
            {
                Exam_ID =exam.Exam_ID,
                Exam_Date = exam.Exam_Date,
                Start_Time = exam.Start_Time,
                End_Time = exam.End_Time,
                Min_Degree = exam.Min_Degree,
                Max_Degree = exam.Max_Degree,
                class_name = exam.class_name,
                subject_name = exam.subject_name,
                Teacher_Name = exam.Tech.User.Full_Name
            };
            return Ok(examDTO);
        }
        #endregion

        #region Get Exam By Teacher ID
        [HttpGet("/Teacher/{Teacher_id}")]
        public async Task<IActionResult> Get_Exam_By_Teacher_ID(string Teacher_id)
        {
            List<Exam> exams=await context.exam.Include(e=>e.Tech).Where(e=>e.Teacher_ID==Teacher_id).ToListAsync();
            if (exams.Count == 0) // تحقق مما إذا كانت القائمة فارغة
            {
                return NotFound($"هذا المعلم '{Teacher_id}' لم يقم بأنشاء امتحانات بعد.");
            }
            List<Get_ExamByTechIDDTO> ExamByTechIDDTOs = new List<Get_ExamByTechIDDTO>();
            foreach (var exam in exams)
            {
                Get_ExamByTechIDDTO get_ExamByTechdto = new Get_ExamByTechIDDTO
                {
                    Exam_ID = exam.Exam_ID,
                    Exam_Date = exam.Exam_Date,
                    Start_Time = exam.Start_Time,
                    End_Time = exam.End_Time,
                    Min_Degree = exam.Min_Degree,
                    Max_Degree = exam.Max_Degree,
                    class_name = exam.class_name,
                    subject_name = exam.subject_name
                };
                ExamByTechIDDTOs.Add(get_ExamByTechdto);
            }
            return Ok(ExamByTechIDDTOs);
        }
        #endregion

        #region Get Exam By class name , stage , level 

        [HttpGet("/GetExamsbyClassName")]
        public async Task<IActionResult> GetExams(string className, string stage, int level)
        {
            var exams = await (from exam in context.exam
                               join cls in context.classes on exam.class_name equals cls.Class_Name
                               where cls.Stage == stage && cls.Level == level && cls.Class_Name == className
                               select exam).Include(e=>e.Tech.User).ToListAsync();

            if (exams.Count == 0)
            {
                return NotFound($"لا يوجد امتحانات لهذه المجموعه '{className}'");
            }

            List<Get_ExamDTO> examDTOs = new List<Get_ExamDTO>();
            foreach (Exam item in exams)
            {
                Get_ExamDTO get_ExamDTO = new Get_ExamDTO();
                get_ExamDTO.Exam_ID = item.Exam_ID;
                get_ExamDTO.Exam_Date = item.Exam_Date;
                get_ExamDTO.Start_Time = item.Start_Time;
                get_ExamDTO.End_Time = item.End_Time;
                get_ExamDTO.Min_Degree = item.Min_Degree;
                get_ExamDTO.Max_Degree = item.Max_Degree;
                get_ExamDTO.class_name = item.class_name;
                get_ExamDTO.subject_name = item.subject_name;
                get_ExamDTO.Teacher_Name = item.Tech.User.Full_Name;
                examDTOs.Add(get_ExamDTO);
            }
            return Ok(examDTOs);
        }

        #endregion

        #region Get Exam By Date
        [HttpGet("/date/{date}")]
        public async Task<IActionResult> Get_Exam_By_Date(DateTime date)
        {
            try
            {
                var datefrom = DateOnly.FromDateTime(date);
                List<Exam> exams = await context.exam.Include(e => e.Tech).Include(e => e.Tech.User).Where(e =>DateOnly.FromDateTime( e.Exam_Date) == datefrom).ToListAsync();

                if (exams.Count == 0) // تحقق مما إذا كانت القائمة فارغة
                {
                    return NotFound("لم يتم انشاء امتحان في هذا اليوم");
                }

                List<Get_ExamDTO> examDTOs = new List<Get_ExamDTO>();

                foreach (var exam in exams)
                {
                    Get_ExamDTO examDTO = new Get_ExamDTO
                    {
                        Exam_ID = exam.Exam_ID,
                        Exam_Date = exam.Exam_Date,
                        Start_Time = exam.Start_Time,
                        End_Time = exam.End_Time,
                        Min_Degree = exam.Min_Degree,
                        Max_Degree = exam.Max_Degree,
                        class_name = exam.class_name,
                        subject_name = exam.subject_name,
                        Teacher_Name = exam.Tech.User.Full_Name
                    };
                    examDTOs.Add(examDTO);
                }

                return Ok(examDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        #region Get Exams By Date Range
        [HttpGet("from/{startDate}/to/{endDate}")]
        public async Task<IActionResult> Get_Exams_By_Date_Range(DateTime startDate, DateTime endDate)
        {
            // تأكد من أن startDate أقل من endDate
            if (startDate > endDate)
            {
                return BadRequest("Start date must be less than or equal to end date.");
            }

            var datefrom = DateOnly.FromDateTime(startDate);
            var dateto = DateOnly.FromDateTime(endDate);

            List<Exam> exams = await context.exam
                .Include(e => e.Tech)
                .Include(e => e.Tech.User)
                .Where(e => DateOnly.FromDateTime(e.Exam_Date) >= datefrom && DateOnly.FromDateTime(e.Exam_Date) <= dateto)
                .ToListAsync();
           
            List<Get_ExamDTO> examDTOs = new List<Get_ExamDTO>();

            if (exams.Count == 0)
            {
                return NotFound("لم يتم انشاء امتحانات في هذه الفترة.");
            }

            foreach (var exam in exams)
            {
                Get_ExamDTO examDTO = new Get_ExamDTO
                {
                    Exam_ID = exam.Exam_ID,
                    Exam_Date = exam.Exam_Date,
                    Start_Time = exam.Start_Time,
                    End_Time = exam.End_Time,
                    Min_Degree = exam.Min_Degree,
                    Max_Degree = exam.Max_Degree,
                    class_name = exam.class_name,
                    subject_name = exam.subject_name,
                    Teacher_Name = exam.Tech.User.Full_Name
                };
                examDTOs.Add(examDTO);
            }

            return Ok(examDTOs);
        }

        #endregion
    
        #region Create Exam
        [HttpPost]
        public async Task<IActionResult> AddExam(Create_ExamDTO create_ExamDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // التحقق من أن Exam_Date هو تاريخ اليوم أو بعده
                if (create_ExamDTO.Exam_Date < DateTime.Today)
                {
                    return BadRequest("تاريخ الامتحان لا يجب ان يكون في الماضي");
                }

                // التحقق من أن Min_Degree أقل من Max_Degree
                if (create_ExamDTO.Min_Degree >= create_ExamDTO.Max_Degree)
                {
                    return BadRequest("Min_Degree must be less than Max_Degree.");
                }

                // التحقق من أن Start_Time أقل من End_Time
                if ((create_ExamDTO.Start_Time <= 0 || create_ExamDTO.Start_Time > 12) ||
                    (create_ExamDTO.End_Time <= 0 || create_ExamDTO.End_Time > 12))
                {
                    return BadRequest("من فضلك ادخل الوقت الصحيح.");
                }
                // Validate class_name against Teacher_Class table
                var teacherClass = await context.teacher_Classes
                    .FirstOrDefaultAsync(tc => tc.Teacher_ID == create_ExamDTO.Teacher_ID &&
                                                 tc.Class.Class_Name == create_ExamDTO.class_name);

                if (teacherClass == null)
                {
                    return BadRequest("هذا المعلم لم يدرس لهذا الفصل ");
                }
                //if (teacherClass==null || teacherClass.Teacher.subject.Subject_Name != create_ExamDTO.subject_name)
                //{
                //    return BadRequest("المادة المحددة لا يدرسها المدرس");
                //}
                //var subject = teacherClass.Teacher.Subject_ID;
                var teacher_sub = context.teachers.Where(t => t.UserId == create_ExamDTO.Teacher_ID).Select(t => t.subject.Subject_Name).FirstOrDefault();
                var exam = new Exam
                {
                    Exam_Date = create_ExamDTO.Exam_Date,
                    Start_Time = create_ExamDTO.Start_Time,
                    End_Time = create_ExamDTO.End_Time,
                    Min_Degree = create_ExamDTO.Min_Degree,
                    Max_Degree = create_ExamDTO.Max_Degree,
                    class_name = create_ExamDTO.class_name,
                    subject_name = teacher_sub,
                    Teacher_ID = create_ExamDTO.Teacher_ID
                };

                context.exam.Add(exam);
                await context.SaveChangesAsync();
                return Ok(new { message = $"تم أضافه الامتحان ." });

                //return Ok(exam);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }
        }
        #endregion

        #region Edit Exam
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit_Exam(int id,Edit_ExamDTO edit_ExamDTO)
        {
            try
            {
                var exam = await context.exam.Include(e => e.Tech).Include(e => e.Tech.User).FirstOrDefaultAsync(e => e.Exam_ID == id);
                if(exam == null)
                {
                    return NotFound("!من فضلك ادخل رقم الامتحان الصحيح");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                // التحقق من أن Exam_Date هو تاريخ اليوم أو بعده
                if (edit_ExamDTO.Exam_Date < DateTime.Today)
                {
                    return BadRequest("Exam_Date cannot be in the past.");
                }

                // التحقق من أن Min_Degree أقل من Max_Degree
                if (edit_ExamDTO.Min_Degree >= edit_ExamDTO.Max_Degree)
                {
                    return BadRequest("Min_Degree must be less than Max_Degree.");
                }

                // التحقق من أن Start_Time أقل من End_Time
                if ((edit_ExamDTO.Start_Time <= 0 || edit_ExamDTO.Start_Time > 12) ||
                    (edit_ExamDTO.End_Time <= 0 || edit_ExamDTO.End_Time > 12))
                {
                    return BadRequest("من فضلك ادخل الوقت الصحيح.");
                }

                exam.Exam_Date = edit_ExamDTO.Exam_Date;
                exam.Start_Time=edit_ExamDTO.Start_Time;
                exam.End_Time=edit_ExamDTO.End_Time;
                exam.Min_Degree=edit_ExamDTO.Min_Degree;
                exam.Max_Degree=edit_ExamDTO.Max_Degree;
                exam.class_name=edit_ExamDTO.class_name;
                context.exam.Update(exam);
                context.SaveChanges();
                return Ok("تم تعديل بيانات الامتحان");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }
        }
        #endregion

        #region Delete Exam
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            try
            {
                var exam= await context.exam.Include(e=>e.Tech).Include(e=>e.Student_Exam).FirstOrDefaultAsync(e=>e.Exam_ID==id);
                if (exam == null)
                {
                    return NotFound("من فضلك ادخل رقم الامتحان الصحيح");
                }
                if(exam.Student_Exam != null)
                {
                    context.student_Exams.RemoveRange(exam.Student_Exam);
                    context.SaveChanges();
                }
                context.exam.Remove(exam);
                context.SaveChanges();
                return Ok(new { message = "تم حذف الامتحان" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }
        }
        #endregion
    }
}


