using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacheCalendarController : ControllerBase
    {
        private readonly AgialContext context;

        public TeacheCalendarController(AgialContext _context)
        {
            context = _context;
        }
        //get all for teacher
        [HttpGet("{id}/{month}/{year}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> teacher_session(string id,int month,int year)
        {
            var T_c=await context.teacher_Classes.Where(tc=>tc.Teacher_ID==id).
                Select(tc=>new { id =tc.TC_ID  ,classname=tc.Class.Class_Name}).ToListAsync();
            List< CalenderTeacherDTO >list=new List< CalenderTeacherDTO >();
            try
            {
                if (T_c.Count() > 0)
                {
                    foreach (var item in T_c)
                    {
                        List<Session> sessions = context.sessions.Where(s => s.TC_ID == item.id && s.Date.Month == month && s.Date.Year == year).ToList();
                        foreach (var session in sessions)
                        {
                            CalenderTeacherDTO v = new CalenderTeacherDTO();
                            v.end = session.End_Time;
                            v.room = session.Room;
                            v.class_name = item.classname;
                            v.date = DateOnly.FromDateTime(session.Date);
                            v.start = session.Start_Time;
                            v.period = session.period;
                            v.session_id = session.Session_ID;

                            list.Add(v);




                        }



                    }

                    return Ok(list);

                }
                else
                {
                    return Ok(new { message = "لم يتم بعد تحديد حصص لتلك معلم" });
                }
            }
            catch (Exception ex) 
            { 
               
                return BadRequest(ex);
            
            }


        }


        #region calendar for student
        //get all for teacher
        [HttpGet("student/{id}/{month}/{year}")]
        [Authorize(Roles = "Student,Parent")]
        public async Task<IActionResult> student_session(string id, int month, int year)
        {
            var class_id=await context.student_classes.Where(s=>s.Student_ID==id).Select(s=>s.Class_ID).ToListAsync();
            List<int> T_C = new List<int>();
            if (class_id.Count != 0)
            {
               
                foreach (var item in class_id)
                {
                    var session_class_id = await context.teacher_Classes.Where(tc => tc.Class_ID ==item).Select(tc => tc.TC_ID).ToListAsync();


                    T_C.AddRange(session_class_id);

                }
                
            }


            List<StudentCalendar> list = new List<StudentCalendar>();
            try
            {
                if (T_C.Count != 0)
                {
                    foreach (var item in T_C)
                    {

                        var session_list = await context.sessions.Where(s => s.TC_ID == item&& s.Date.Month == month && s.Date.Year == year).ToListAsync();
                        foreach (var item1 in session_list)
                        {
                            StudentCalendar session = new StudentCalendar();
                            session.start = item1.Start_Time;
                            session.session_name = item1.Material_Name;
                            session.room = item1.Room;
                            session.date = DateOnly.FromDateTime(item1.Date);
                            session.period = item1.period;
                            session.session_id = item1.Session_ID;
                            session.end=item1.End_Time;
                            list.Add(session);

                        }

                    }

                  
                }
                return Ok(list);
            }
            catch (Exception ex)
            {

                return BadRequest(ex);

            }


        }

        #endregion



        #region by parent id view get his student

        [HttpGet("parent/{parent_id}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> has_son(string parent_id)
        {
            var list_student=await context.students.Where(s=>s.Parent_ID == parent_id)
                .Select(s => new {userId=s.UserId,fallname=s.User.Full_Name})
                .ToListAsync();
            try
            {
              if (list_student.Count != 0)
              {
                    return Ok(list_student);
              }
                else
                {
                    return Ok(new {message="لا يمتلك ابناء بعد"});
                }
            }
            catch (Exception ex)
            { 
              return BadRequest(ex.Message);
            
            }

        }


        #endregion












    }
}
