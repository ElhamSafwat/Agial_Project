using final_project_Api.Admin_ClassDTO;
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
                    .ThenInclude(tc => tc.Subjects)
                .Select(c => new ClassDto
                {
                    ClassID = c.Class_ID,
                    ClassName = c.Class_Name,
                    Stage = c.Stage,
                    Level = c.Level,
                    StudentNames = c.Student_Class.Select(sc => sc.students.User.Full_Name).ToList(),
                    TeacherNames = c.Teacher_Class.Select(tc => tc.Teacher.User.Full_Name).ToList(),
                    SubjectNames = c.Teacher_Class.Select(tc => tc.Subjects.Subject_Name).ToList()
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
                    .ThenInclude(tc => tc.Subjects)
                .FirstOrDefaultAsync(c => c.Class_ID == id);

            if (classEntity == null)
            {
                return NotFound("هذا الفصل غير موجود .");
            }

            var classDto = new ClassDto
            {
                ClassID = classEntity.Class_ID,
                ClassName = classEntity.Class_Name,
                Stage = classEntity.Stage,
                Level = classEntity.Level,
                StudentNames = classEntity.Student_Class.Select(sc => sc.students.User.Full_Name).ToList(),
                TeacherNames = classEntity.Teacher_Class.Select(tc => tc.Teacher.User.Full_Name).ToList(),
                // Ensuring distinct subject names are returned
                SubjectNames = classEntity.Teacher_Class.Select(tc => tc.Subjects.Subject_Name).ToList()
            };

            return Ok(classDto);
        }
        #endregion

        #region 3. Create New Class
        [HttpPost]
        public async Task<ActionResult<ClassDto>> CreateClass(ClassCreateUpdateDto classCreateDto)
        {
            // Validate the stage
            if (!new[] { "الابتدائية", "الاعدادية", "الثانوية" }.Contains(classCreateDto.Stage))
            {
                return BadRequest("المرحلة يجب أن تكون الابتدائية أو الاعدادية أو الثانوية.");
            }

            // Validate the level based on stage
            if (!ValidateLevel(classCreateDto.Stage, classCreateDto.Level))
            {
                return BadRequest("المستوى غير مناسب للمرحلة المحددة.");
            }

            // Check if the class name already exists for the same stage and level
            var existingClass = await _context.classes.FirstOrDefaultAsync(c =>
                c.Stage == classCreateDto.Stage &&
                c.Level == classCreateDto.Level &&
                c.Class_Name == classCreateDto.ClassName);

            if (existingClass != null)
            {
                return BadRequest("هذا الفصل مسجل بالفعل لنفس المرحلة والمستوى.");
            }

            var students = new List<Student_Class>();
            var studentIdsSet = new HashSet<string>();

            // Check and add students
            foreach (var studentId in classCreateDto.StudentIds)
            {
                if (!studentIdsSet.Add(studentId))
                {
                    return BadRequest($"الطالب برقم {studentId} مكرر في نفس الطلب.");
                }

                var student = await _context.students.FindAsync(studentId);
                if (student == null)
                {
                    return BadRequest($"الطالب برقم {studentId} غير موجود.");
                }

                var existingStudentClass = await _context.teacher_Classes.AnyAsync(tc =>
                tc.Teacher_ID == student.UserId &&
                _context.classes.Any(c => c.Class_Name == classCreateDto.ClassName && c.Class_ID == tc.Class_ID));

                if (existingStudentClass)
                {
                    return BadRequest($"الطالب برقم {studentId} مسجل بالفعل في هذا الفصل.");
                }

                students.Add(new Student_Class { Student_ID = student.UserId });
            }


            var teachers = new List<Teacher_Class>();
            var teacherIdsSet = new HashSet<string>();
            var subjectIdsSet = new HashSet<string>();

            foreach (var teacherId in classCreateDto.TeacherIds)
            {
                if (!teacherIdsSet.Add(teacherId))
                {
                    return BadRequest($"المعلم برقم {teacherId} مكرر في نفس الطلب.");
                }

                var teacher = await _context.teachers.FindAsync(teacherId);
                if (teacher == null)
                {
                    return BadRequest($"المعلم برقم {teacherId} غير موجود.");
                }

                foreach (var subjectId in classCreateDto.SubjectIds)
                {
                    if (!subjectIdsSet.Add($"{teacherId}-{subjectId}")) // Check for duplicate teacher-subject pair
                    {
                        return BadRequest($"المعلم برقم {teacherId} لديه مادة برقم {subjectId} مكررة في نفس الفصل.");
                    }
                    // Ensure the subject exists
                    var subject = await _context.subjects.FindAsync(subjectId);
                    if (subject == null)
                    {
                        return BadRequest($"المادة برقم {subjectId} غير موجودة.");
                    }

                    // Add teacher and subject to the class
                    teachers.Add(new Teacher_Class
                    {
                        Teacher_ID = teacher.UserId,
                        Subject_ID = subject.Subject_ID
                    });
                }
            }


            var newClass = new Class
            {
                Stage = classCreateDto.Stage,
                Class_Name = classCreateDto.ClassName,
                Level = classCreateDto.Level
            };

            _context.classes.Add(newClass);
            await _context.SaveChangesAsync();

            foreach (var student in students)
            {
                student.Class_ID = newClass.Class_ID;
                _context.student_classes.Add(student);
            }

            foreach (var teacher in teachers)
            {
                teacher.Class_ID = newClass.Class_ID;
                _context.teacher_Classes.Add(teacher);
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
            var classEntity = await _context.classes.FindAsync(id);
            if (classEntity == null)
            {
                return NotFound("هذا الفصل غير موجود بالفعل.");
            }

            // Validate the stage
            if (!new[] { "الابتدائية", "الاعدادية", "الثانوية" }.Contains(classUpdateDto.Stage))
            {
                return BadRequest("المرحلة يجب أن تكون الابتدائية أو الاعدادية أو الثانوية.");
            }

            // Validate the level based on stage
            if (!ValidateLevel(classUpdateDto.Stage, classUpdateDto.Level))
            {
                return BadRequest("المستوى غير مناسب للمرحلة المحددة.");
            }

            classEntity.Class_Name = classUpdateDto.ClassName;
            classEntity.Stage = classUpdateDto.Stage;
            classEntity.Level = classUpdateDto.Level;

            // Clear existing students and teachers
            _context.student_classes.RemoveRange(_context.student_classes.Where(sc => sc.Class_ID == id));
            _context.teacher_Classes.RemoveRange(_context.teacher_Classes.Where(tc => tc.Class_ID == id));

            var students = new List<Student_Class>();
            var studentIdsSet = new HashSet<string>();

            // Check and add students
            foreach (var studentId in classUpdateDto.StudentIds)
            {
                if (!studentIdsSet.Add(studentId))
                {
                    return BadRequest($"الطالب برقم {studentId} مكرر في نفس الطلب.");
                }

                var student = await _context.students.FindAsync(studentId);
                if (student == null)
                {
                    return BadRequest($"الطالب برقم {studentId} غير موجود.");
                }

                students.Add(new Student_Class { Student_ID = student.UserId });
            }

            var teachers = new List<Teacher_Class>();
            var teacherIdsSet = new HashSet<string>();
            var subjectIdsSet = new HashSet<string>();

            // Check and add teachers
            foreach (var teacherId in classUpdateDto.TeacherIds)
            {
                if (!teacherIdsSet.Add(teacherId))
                {
                    return BadRequest($"المعلم برقم {teacherId} مكرر في نفس الطلب.");
                }

                var teacher = await _context.teachers.FindAsync(teacherId);
                if (teacher == null)
                {
                    return BadRequest($"المعلم برقم {teacherId} غير موجود.");
                }

                // Check for duplicate Subject IDs associated with this teacher
                foreach (var subjectId in classUpdateDto.SubjectIds)
                {
                    if (!subjectIdsSet.Add($"{subjectId}"))
                    {
                        return BadRequest($"المعلم برقم {teacherId} لديه مادة برقم {subjectId} مكررة في نفس الفصل.");
                    }
                }

                teachers.Add(new Teacher_Class { Teacher_ID = teacher.UserId });
            }

            foreach (var student in students)
            {
                student.Class_ID = classEntity.Class_ID;
                _context.student_classes.Add(student);
            }

            foreach (var teacher in teachers)
            {
                teacher.Class_ID = classEntity.Class_ID;
                _context.teacher_Classes.Add(teacher);
            }

            _context.Entry(classEntity).State = EntityState.Modified;
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
                return NotFound("هذا الفصل غير موجود .");
            }

            // Remove related student_classes
            _context.student_classes.RemoveRange(classEntity.Student_Class);
            // remove related session 
            if (classEntity.Teacher_Class != null) 
            {
               List<int> tc_ids = classEntity.Teacher_Class.Select(t => t.TC_ID).ToList();
                List<Session>delete_session = new List<Session>();
                foreach (var item in tc_ids)
                {
                    var sessions = await _context.sessions.Where(s => s.TC_ID ==item).ToListAsync();
                    if (sessions != null)
                    {
                        delete_session.AddRange(sessions);
                    }
                }
                if(delete_session.Count > 0)
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
                case "الابتدائية":
                    return level >= 1 && level <= 6;
                case "الاعدادية":
                    return level >= 1 && level <= 3;
                case "الثانوية":
                    return level >= 1 && level <= 3;
                default:
                    return false;
            }
        }
    }

}
