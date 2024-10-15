using final_project_Api.DTOs;
using final_project_Api.Models;
using final_project_Api.SubjectDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly AgialContext _context;

        public SubjectController(AgialContext context)
        {
            _context = context;
        }


        // GET: api/subject
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<getsubjectDto>>> GetSubjects()
        {
            try
            {
                var subjects = await _context.subjects
                    .Include(s => s.teachers)
                    .ThenInclude(t => t.User)
                    .ToListAsync();

                if (subjects == null || !subjects.Any())
                {
                    return NotFound("لم يتم العثور على مواد دراسية.");
                }

                var subjectDtos = subjects.Select(s => new getsubjectDto
                {
                    id = s.Subject_ID,
                    Subject_Name = s.Subject_Name,
                    Description = s.Description,
                    TeacherNames = s.teachers != null && s.teachers.Any()
                        ? s.teachers.Select(t => t.User.Full_Name).ToList()
                        : new List<string> { "لا يوجد معلمين لهذه المادة" }
                }).ToList();

                return Ok(subjectDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ غير متوقع: {ex.Message}");
            }
        }




        // GET: api/subject/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<getsubjectDto>> GetSubject(int id)
        {
            try
            {
                var subject = await _context.subjects
                                            .Include(s => s.teachers)
                                            .ThenInclude(t => t.User)
                                            .FirstOrDefaultAsync(s => s.Subject_ID == id);

                if (subject == null)
                {
                    return NotFound("المادة غير موجودة.");
                }

                var subjectDto = new getsubjectDto
                {
                    id = subject.Subject_ID,
                    Description = subject.Description,
                    Subject_Name = subject.Subject_Name,
                    TeacherNames = subject.teachers.Select(t => t.User.Full_Name).ToList()
                };

                return Ok(subjectDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ غير متوقع: {ex.Message}");
            }
        }



        // POST: api/subjects
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Subject>> PostSubject(putpostSubjectDtos putpostDtos)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(putpostDtos.Subject_Name))
                {
                    return BadRequest("اسم المادة مطلوب.");
                }

                var existingSubject = await _context.subjects
                    .FirstOrDefaultAsync(s => s.Subject_Name == putpostDtos.Subject_Name);

                if (existingSubject != null)
                {
                    return BadRequest("المادة موجودة بالفعل.");
                }

                var subject = new Subject
                {
                    Description = putpostDtos.Description,
                    Subject_Name = putpostDtos.Subject_Name,
                };

                _context.subjects.Add(subject);
                await _context.SaveChangesAsync();

                return Ok(subject);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"حدث خطأ أثناء حفظ البيانات: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ غير متوقع: {ex.Message}");
            }
        }






        // PUT: api/subject/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutSubject(int id, putpostSubjectDtos putDtos)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(putDtos.Subject_Name))
                {
                    return BadRequest("اسم المادة مطلوب.");
                }

                var subject = await _context.subjects.FirstOrDefaultAsync(s => s.Subject_ID == id);
                if (subject == null)
                {
                    return NotFound("المادة غير موجودة.");
                }

                subject.Description = putDtos.Description;
                subject.Subject_Name = putDtos.Subject_Name;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"حدث خطأ أثناء تحديث البيانات: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ غير متوقع: {ex.Message}");
            }
        }





    }
}
