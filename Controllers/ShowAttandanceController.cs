using final_project_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowAttandanceController : ControllerBase
    {

        private readonly AgialContext context;

        public ShowAttandanceController(AgialContext _context)
        {
            context = _context;
        }

        #region get attendence
        [HttpGet]

        public async Task<IActionResult> Show()
        {
            var query = from s in context.sessions
                        join t_c in context.teacher_Classes on s.TC_ID equals t_c.TC_ID
                        join t in context.teachers on t_c.Teacher_ID equals t.UserId
                        join c in context.classes on t_c.Class_ID equals c.Class_ID
                        join s_s in context.Session_Students on s.Session_ID equals s_s.Session_ID
                        join Stud in context.students on s_s.Student_ID equals Stud.UserId
                        where (s.Date.Year == DateTime.Now.Year || s.Date.Year == (DateTime.Now.Year - 1))&&(s.Date.Day <= DateTime.Now.Day)
                        select new

                        {
                            teachername = t.User.Full_Name,
                            className = c.Class_Name,
                            date= DateOnly.FromDateTime(s.Date),
                            session_name=s.Material_Name,
                            studentname= Stud.User.Full_Name,
                            attendance = s_s.Attendance ? "حاضر" : "غائب"


                        };

            var results = await query.ToListAsync(); 

            return Ok(results);
        }



        #endregion



        #region search  class name and stage leval data
        [HttpGet("{classname}/{stage}/{level}/{date}")]
        public async Task<IActionResult> showbyclassname(string classname, string stage,int level, DateTime date )
        {
            var query = from s in context.sessions
                        join t_c in context.teacher_Classes on s.TC_ID equals t_c.TC_ID
                        join t in context.teachers on t_c.Teacher_ID equals t.UserId
                        join c in context.classes on t_c.Class_ID equals c.Class_ID
                        join s_s in context.Session_Students on s.Session_ID equals s_s.Session_ID
                        join Stud in context.students on s_s.Student_ID equals Stud.UserId
                        where DateOnly.FromDateTime(s.Date)== DateOnly.FromDateTime(date) && c.Level==level && c.Class_Name==classname && c.Stage==stage
                        select new

                        {
                            teachername = t.User.Full_Name,
                            className = c.Class_Name,
                            date = DateOnly.FromDateTime(s.Date),
                            session_name = s.Material_Name,
                            studentname = Stud.User.Full_Name,
                            attendance = s_s.Attendance ? "حاضر" : "غائب"


                        };

            var results = await query.ToListAsync();

            return Ok(results);
        }
        #endregion

        #region search  class name and stage leval data
        [HttpGet("{datefrom}/{dateto}")]
        public async Task<IActionResult> showbycdate( DateTime datefrom, DateTime dateto)
        {
            var query = from s in context.sessions
                        join t_c in context.teacher_Classes on s.TC_ID equals t_c.TC_ID
                        join t in context.teachers on t_c.Teacher_ID equals t.UserId
                        join c in context.classes on t_c.Class_ID equals c.Class_ID
                        join s_s in context.Session_Students on s.Session_ID equals s_s.Session_ID
                        join Stud in context.students on s_s.Student_ID equals Stud.UserId
                        where DateOnly.FromDateTime(s.Date) >= DateOnly.FromDateTime(datefrom) && DateOnly.FromDateTime(s.Date) <= DateOnly.FromDateTime(dateto)
                        select new

                        {
                            teachername = t.User.Full_Name,
                            className = c.Class_Name,
                            date = DateOnly.FromDateTime(s.Date),
                            session_name = s.Material_Name,
                            studentname = Stud.User.Full_Name,
                            attendance = s_s.Attendance ? "حاضر" : "غائب"

                        };

            var results = await query.ToListAsync();

            return Ok(results);
        }
        #endregion

        #region show assignmnet by student id
        [HttpGet("{studentId}")]
        public async Task<IActionResult> Showassignment(string studentId)
        {
            var query = from s in context.sessions
                        join t_c in context.teacher_Classes on s.TC_ID equals t_c.TC_ID
                        join t in context.teachers on t_c.Teacher_ID equals t.UserId
                        join c in context.classes on t_c.Class_ID equals c.Class_ID
                        join s_s in context.Session_Students on s.Session_ID equals s_s.Session_ID
                        join Stud in context.students on s_s.Student_ID equals Stud.UserId
                        where (s.Date.Year == DateTime.Now.Year || s.Date.Year == (DateTime.Now.Year - 1)) && (s.Date.Day <= DateTime.Now.Day)
                              && s_s.Student_ID == studentId // تصفية بناءً على studentId المعطى
                        select new
                        {
                            teachername = t.User.Full_Name,
                            className = c.Class_Name,
                            date = DateOnly.FromDateTime(s.Date),
                            session_name = s.Material_Name,
                            studentname = Stud.User.Full_Name,
                            attendance = s_s.Attendance ? "حاضر" : "غائب"
                        };

            var results = await query.ToListAsync();

            return Ok(results);
        }

        #endregion

    }
}
