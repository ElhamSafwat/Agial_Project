using final_project_Api.Admin_ClassDTO;
using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace final_project_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly AgialContext _context;

        public ClassController(AgialContext context)
        {
            _context = context;
        }

        #region 1. Get All Classes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassDto>>> GetClasses()
        {
            var classes = await _context.classes
                .Include(c => c.Student_Class)
                    .ThenInclude(sc => sc.students)
                        .ThenInclude(s => s.User)
                .Include(c => c.Teacher_Class)
                    .ThenInclude(tc => tc.Teacher)
                        .ThenInclude(t => t.User)
                .Include(c => c.Teacher_Class)
                    .ThenInclude(tc => tc.Teacher.subject)
                .Select(c => new ClassDto
                {
                    ClassID = c.Class_ID,
                    ClassName = c.Class_Name,
                    Stage = c.Stage,
                    Level = c.Level,
                    StudentNames = c.Student_Class.Select(sc => sc.students.User.Full_Name).ToList(),
                    TeacherNames = c.Teacher_Class.Select(tc => tc.Teacher.User.Full_Name).ToList(),
                    SubjectNames = c.Teacher_Class.Select(tc => tc.Teacher.subject.Subject_Name).ToList()
                })
                .ToListAsync();

            return Ok(classes);
        }
        #endregion

        #region 2. Get Class by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ClassDto>> GetClass(int id)
        {
            var classEntity = await _context.classes
                .Include(c => c.Student_Class)
                    .ThenInclude(sc => sc.students)
                        .ThenInclude(s => s.User)
                .Include(c => c.Teacher_Class)
                    .ThenInclude(tc => tc.Teacher)
                        .ThenInclude(t => t.User)
                .Include(c => c.Teacher_Class)
                    .ThenInclude(tc => tc.Teacher.subject)
                .FirstOrDefaultAsync(c => c.Class_ID == id);

            if (classEntity == null)
            {
                return NotFound(new { message = "هذا الفصل غير موجود." });
            }

            var classDto = new ClassDto
            {
                ClassID = classEntity.Class_ID,
                ClassName = classEntity.Class_Name,
                Stage = classEntity.Stage,
                Level = classEntity.Level,


                StudentNames = classEntity.Student_Class.Select(sc => sc.students.User.Full_Name).ToList(),
                TeacherNames = classEntity.Teacher_Class.Select(tc => tc.Teacher.User.Full_Name).ToList(),
                SubjectNames = classEntity.Teacher_Class.Select(tc => tc.Teacher.subject.Subject_Name).ToList()
            };

            return Ok(classDto);
        }
        #endregion

       

        #region 3. Create New Class
        [HttpPost]
        public async Task<ActionResult<ClassDto>> CreateClass(ClassCreateUpdateDto classCreateDto)
        {
            // قائمة لتخزين رسائل الأخطاء
            var errorMessages = new List<string>();

            // Validate the stage
            if (!new[] { "أبتدائي", "أعدادي", "ثانوي" }.Contains(classCreateDto.Stage))
            {
                errorMessages.Add("المرحلة يجب أن تكون الابتدائية أو الاعدادية أو الثانوية.");
            }

            // Validate the level based on stage
            if (!ValidateLevel(classCreateDto.Stage, classCreateDto.Level))
            {
                errorMessages.Add("المستوى غير مناسب للمرحلة المحددة.");
            }

            // Check if the class name already exists for the same stage and level
            var existingClass = await _context.classes.FirstOrDefaultAsync(c =>
                c.Stage == classCreateDto.Stage &&
                c.Level == classCreateDto.Level &&
                c.Class_Name == classCreateDto.ClassName);

            if (existingClass != null)
            {
                errorMessages.Add("هذا الفصل مسجل بالفعل لنفس المرحلة والمستوى.");
            }

            var students = new List<Student_Class>();
            var studentIdsSet = new HashSet<string>();

            // Check and add students
            foreach (var studentId in classCreateDto.StudentIds)
            {
                if (!studentIdsSet.Add(studentId))
                {
                    errorMessages.Add($"الطالب برقم {studentId} مكرر في نفس الطلب.");
                    continue;
                }

                var student = _context.students
                    .Where(s => s.Stage == classCreateDto.Stage && s.Level == classCreateDto.Level && s.UserId == studentId)
                    .FirstOrDefault();

                if (student == null)
                {
                    errorMessages.Add($"الطالب برقم {studentId} ليس في نفس المرحلة أو المستوى.");
                    continue;
                }

                var existingStudentClass = await _context.student_classes
                    .Include(st => st.classs)
                    .AnyAsync(st =>
                        st.Student_ID == student.UserId &&
                        st.classs.Class_Name == classCreateDto.ClassName &&
                        st.classs.Stage == classCreateDto.Stage &&
                        st.classs.Level == classCreateDto.Level);

                if (existingStudentClass)
                {
                    errorMessages.Add($"الطالب برقم {studentId} مسجل بالفعل في هذا الفصل.");
                    continue;
                }

                students.Add(new Student_Class { Student_ID = student.UserId });
            }

            var teacherIdsSet = new HashSet<string>();
            var subjectIdsSet = new HashSet<int>(); // لتتبع المواد المعطاة لكل مدرس

            // جلب جميع المعلمين بناءً على TeacherIds الموجودة في الطلب
            var teachers = await _context.teachers
                .Where(t => classCreateDto.TeacherIds.Contains(t.UserId))
                .ToListAsync();

            if (teachers.Count == 0)
            {
                errorMessages.Add("لا يوجد معلمين مطابقين.");
            }

            // التحقق من المعلمين وتكرار المواد
            foreach (var teacher in teachers)
            {
                if (!teacherIdsSet.Add(teacher.UserId))
                {
                    errorMessages.Add($"المعلم برقم {teacher.UserId} مكرر في نفس الطلب.");
                }


                // التحقق من تكرار المادة
                if (!subjectIdsSet.Add(teacher.Subject_ID))
                {
                    errorMessages.Add($"المعلم برقم {teacher.UserId} لديه نفس المادة التي تم تعيينها بالفعل.");
                }
            }

            // إذا كانت هناك أخطاء، ارجع الأخطاء بصيغة new { message = "" }
            if (errorMessages.Count > 0)
            {
                return BadRequest(new { message = string.Join("\n", errorMessages) });
            }

            // إنشاء الفصل الجديد
            var newClass = new Class
            {
                Stage = classCreateDto.Stage,
                Class_Name = classCreateDto.ClassName,
                Level = classCreateDto.Level
            };

            _context.classes.Add(newClass);
            await _context.SaveChangesAsync();

            // إضافة الطلاب إلى الفصل الجديد
            foreach (var student in students)
            {
                student.Class_ID = newClass.Class_ID;
                _context.student_classes.Add(student);
            }

            // إضافة المعلمين إلى الفصل الجديد
            foreach (var teacher in teachers)
            {
                var teacherClass = new Teacher_Class
                {
                    Teacher_ID = teacher.UserId,
                    Class_ID = newClass.Class_ID
                };
                await _context.teacher_Classes.AddAsync(teacherClass);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClass), new { id = newClass.Class_ID }, new ClassDto
            {
                ClassID = newClass.Class_ID,
                Stage = newClass.Stage,
                ClassName = newClass.Class_Name,
                Level = newClass.Level
            });
        }
        #endregion

        #region 4. Update Class
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(int id, ClassCreateUpdateDto classUpdateDto)
        {
            var errorMessages = new List<string>();

            // Fetch existing class
            var classEntity = await _context.classes.FindAsync(id);
            if (classEntity == null)
            {
                return NotFound(new { message = "هذا الفصل غير موجود بالفعل." });
            }

            // Validate the stage
            if (!new[] { "أبتدائي", "أعدادي", "ثانوي" }.Contains(classUpdateDto.Stage))
            {
                errorMessages.Add("المرحلة يجب أن تكون أبتدائي أو أعدادي أو ثانوي.");
            }

            // Validate the level based on stage
            if (!ValidateLevel(classUpdateDto.Stage, classUpdateDto.Level))
            {
                errorMessages.Add("المستوى غير مناسب للمرحلة المحددة.");
            }

            // Check for duplicate class name for the same stage and level
            var existingClass = await _context.classes.FirstOrDefaultAsync(c =>
                c.Stage == classUpdateDto.Stage &&
                c.Level == classUpdateDto.Level &&
                c.Class_Name == classUpdateDto.ClassName &&
                c.Class_ID != id); // Exclude the current class

            if (existingClass != null)
            {
                errorMessages.Add("هذا الفصل مسجل بالفعل لنفس المرحلة والمستوى.");
            }

            var students = new List<Student_Class>();
            var studentIdsSet = new HashSet<string>();

            // Validate and add students
            foreach (var studentId in classUpdateDto.StudentIds)
            {
                if (!studentIdsSet.Add(studentId))
                {
                    errorMessages.Add($"الطالب برقم {studentId} مكرر في نفس الطلب.");
                    continue;
                }

                var student = await _context.students.FindAsync(studentId);
                if (student == null || student.Stage != classUpdateDto.Stage || student.Level != classUpdateDto.Level)
                {
                    errorMessages.Add($"الطالب برقم {studentId} ليس في نفس المرحلة أو المستوى.");
                    continue;
                }

                var existingStudentClass = await _context.student_classes
                    .AnyAsync(sc => sc.Student_ID == student.UserId &&
                                    sc.Class_ID != classEntity.Class_ID);
                if (existingStudentClass)
                {
                    errorMessages.Add($"الطالب برقم {studentId} مسجل بالفعل في فصل آخر.");
                    continue;
                }

                students.Add(new Student_Class { Student_ID = student.UserId });
            }

            var teacherIdsSet = new HashSet<string>();
            var subjectIdsSet = new HashSet<int>(); // لتتبع المواد المعطاة لكل مدرس

            // جلب جميع المعلمين بناءً على TeacherIds الموجودة في الطلب
            var teachers = await _context.teachers
                .Where(t => classUpdateDto.TeacherIds.Contains(t.UserId))
                .ToListAsync();

            if (teachers.Count == 0)
            {
                errorMessages.Add("لا يوجد معلمين مطابقين.");
            }

            // التحقق من المعلمين وتكرار المواد
            foreach (var teacher in teachers)
            {
                if (!teacherIdsSet.Add(teacher.UserId))
                {
                    errorMessages.Add($"المعلم برقم {teacher.UserId} مكرر في نفس الطلب.");
                }


                // التحقق من تكرار المادة
                if (!subjectIdsSet.Add(teacher.Subject_ID))
                {
                    errorMessages.Add($"المعلم برقم {teacher.UserId} لديه نفس المادة التي تم تعيينها بالفعل.");
                }
            }

            // إذا كانت هناك أخطاء، ارجع الأخطاء بصيغة new { message = "" }
            if (errorMessages.Count > 0)
            {
                return BadRequest(new { message = string.Join("\n", errorMessages) });
            }

            // If there are errors, return the errors as JSON
            if (errorMessages.Count > 0)
            {
                return BadRequest(new { message = string.Join("\n", errorMessages) });
            }

            // Update class entity properties
            classEntity.Class_Name = classUpdateDto.ClassName;
            classEntity.Stage = classUpdateDto.Stage;
            classEntity.Level = classUpdateDto.Level;

            // Clear existing students and teachers
            _context.student_classes.RemoveRange(_context.student_classes.Where(sc => sc.Class_ID == id));
            _context.teacher_Classes.RemoveRange(_context.teacher_Classes.Where(tc => tc.Class_ID == id));

            // Add validated students to the class
            foreach (var student in students)
            {
                student.Class_ID = classEntity.Class_ID;
                _context.student_classes.Add(student);
            }

            // Add validated teachers to the class
            foreach (var teacher in teachers)
            {
                var teacherClass = new Teacher_Class
                {
                    Teacher_ID = teacher.UserId,
                    Class_ID = classEntity.Class_ID
                };
                _context.teacher_Classes.Add(teacherClass);
            }



            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم التعديل بنجاح" });
        }
        #endregion

        #region 5. Delete Class
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var classEntity = await _context.classes
                .Include(c => c.Student_Class)
                .Include(c => c.Teacher_Class)
                .FirstOrDefaultAsync(c => c.Class_ID == id);

            if (classEntity == null)
            {
                return NotFound(new { message = "هذا الفصل غير موجود ." });
            }

            // Remove related student_classes
            _context.student_classes.RemoveRange(classEntity.Student_Class);
            // remove related session 
            if (classEntity.Teacher_Class != null)
            {
                List<int> tc_ids = classEntity.Teacher_Class.Select(t => t.TC_ID).ToList();
                List<Session> delete_session = new List<Session>();
                foreach (var item in tc_ids)
                {
                    var sessions = await _context.sessions.Where(s => s.TC_ID == item).ToListAsync();
                    if (sessions != null)
                    {
                        delete_session.AddRange(sessions);
                    }
                }
                if (delete_session.Count > 0)
                {
                    _context.sessions.RemoveRange(delete_session);
                }

            }


            // Remove related teacher_classes
            _context.teacher_Classes.RemoveRange(classEntity.Teacher_Class);

            // Finally, remove the class
            _context.classes.Remove(classEntity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم المسح بنجاح" });
        }

        #endregion

        // Helper function to validate level based on stage
        private bool ValidateLevel(string stage, int level)
        {
            switch (stage)
            {
                case "أبتدائي":
                    return level >= 1 && level <= 6;
                case "أعدادي":
                    return level >= 1 && level <= 3;
                case "ثانوي":
                    return level >= 1 && level <= 3;
                default:
                    return false;
            }
        }
    }

}
