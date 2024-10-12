using final_project_Api.Admin_ClassDTO;
using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static final_project_Api.DTO.Get_for_editClass;

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

                var student = _context.students.Include(s=>s.User)
                    .Where(s => s.Stage == classCreateDto.Stage && s.Level == classCreateDto.Level && s.UserId == studentId)
                    .FirstOrDefault();

                if (student == null)
                {
                    errorMessages.Add($"الطالب برقم {student.User.Full_Name} ليس في نفس المرحلة أو المستوى.");
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
                    errorMessages.Add($"الطالب  {student.User.Full_Name} مسجل بالفعل في هذا الفصل.");
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
        
        
        #region get student by stage and level
        [HttpGet("students/{stage}/{level}")]
        public async Task<IActionResult>getstudentsforclass(string stage ,int level)
        {
            var students = await _context.students.Where(s => s.Stage == stage && s.Level == level).Select(s => new { student_id = s.UserId, student_name = s.User.Full_Name }).ToListAsync(); ;

           

            return Ok(students);
        }
        #endregion

        #region get teacher
        [HttpGet("teachers/{stage}")]
        public async Task<IActionResult> getteacherforclass(string stage)
        {
           
            if (stage == "ثانوي")
            {
                 var teachers = await _context.teachers.Where(s => s.teacher_Stages.Any(t=>t.Stage == stage)  ).Select(s => new { teacher_id = s.UserId, teacher_name = s.User.Full_Name,stubject_id=s.Subject_ID }).ToListAsync(); ;
                return Ok(teachers);
            }
            else
            {
                var teachers = await _context.teachers.Select(s => new { teacher_id = s.UserId, teacher_name = s.User.Full_Name, stubject_id = s.Subject_ID }).ToListAsync();
                return Ok(teachers);
            }

           
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


        #region git for edit

        [HttpGet("GETClass/{id}")]
        public async Task<ActionResult<Get_for_editClass>> GetClasses(int id)
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

            // بناء كائن ClassDto مع معلومات الطلاب والمدرسين
            var classDto = new Get_for_editClass
            {
                ClassID = classEntity.Class_ID,
                ClassName = classEntity.Class_Name,
                Stage = classEntity.Stage,
                Level = classEntity.Level,
                Students = classEntity.Student_Class
                    .Select(sc => new StudentgetDto
                    {
                        StudentID = sc.students.UserId, // استخدام UserId كمعرف للطالب
                        FullName = sc.students.User.Full_Name
                    })
                    .ToList(),
                Teachers = classEntity.Teacher_Class
                    .Select(tc => new TeachergetDto
                    {
                        TeacherID = tc.Teacher.UserId, // استخدام UserId كمعرف للمدرس
                        FullName = tc.Teacher.User.Full_Name
                    })
                    .ToList(),
                SubjectNames = classEntity.Teacher_Class
                    .Select(tc => tc.Teacher.subject.Subject_Name)
                    .Distinct() 
                    .ToList()
            };

            return Ok(classDto);
        }

        #endregion




        #region edit
        [HttpPut("EditClass/{id}")]


        public async Task<IActionResult> Edit(int id, ClassCreateUpdateDto update)
        {
            try
            {
                // جلب الطلاب المرتبطين بالفصل الحالي
                var class_entity = await _context.student_classes.Where(c => c.Class_ID == id).ToListAsync();

                // حذف الطلاب الموجودين في القائمة الجديدة (الذين يجب حذفهم)
                foreach (var existingStudent in class_entity)
                {
                    if (update.StudentIds.Contains(existingStudent.Student_ID))
                    {
                        _context.student_classes.Remove(existingStudent);
                    }
                }

                // إضافة الطلاب الجدد الذين لم يكونوا موجودين في الفصل
                foreach (var newStudentId in update.StudentIds)
                {
                    if (!class_entity.Any(sc => sc.Student_ID == newStudentId))
                    {
                        var newStudent = new Student_Class
                        {
                            Student_ID = newStudentId,
                            Class_ID = id
                        };
                        _context.student_classes.Add(newStudent);
                    }
                }

                
                var teacher_entity = await _context.teacher_Classes.Where(c => c.Class_ID == id).ToListAsync();

                
                foreach (var existingTeacher in teacher_entity)
                {
                    if (update.TeacherIds.Contains(existingTeacher.Teacher_ID))
                    {
                        _context.teacher_Classes.Remove(existingTeacher);
                    }
                }

               
                foreach (var newTeacherId in update.TeacherIds)
                {
                    if (!teacher_entity.Any(tc => tc.Teacher_ID == newTeacherId))
                    {
                        var newTeacher = new Teacher_Class
                        {
                            Teacher_ID = newTeacherId,
                            Class_ID = id
                        };
                        _context.teacher_Classes.Add(newTeacher);
                    }
                }

                
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم التعديل بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

      


    }
    #endregion






}


