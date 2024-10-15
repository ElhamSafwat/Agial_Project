using final_project_Api.Admin_ClassDTO;
using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        public AgialContext context;
        public SessionController(AgialContext _context)
        {
            context= _context;
        }
        #region add new session
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> post(List<Create_Session> sess)
        {
            //create list from class to add session
            List<Session> sessionsadd = new List<Session>();
            //list for add error to display for admin
   
            List<object> errors = new List<object>();
            try
            {
                if (sess == null || sess.Count == 0)
                {
                    return BadRequest(new { message = "لا توجد حصص تم اضافتها" });
                }
                foreach (var item in sess)
                {
                    //for check data is valid for his model
                    if (ModelState.IsValid)
                    {
                        Session session1 = new Session();
                        session1.Date = item.Date;

                        session1.End_Time = item.End_Time;
                        session1.Start_Time = item.Start_Time;
                        session1.period = item.period;
                        session1.Room = item.Room;
                        #region to reach to id session with id Teacher_Class and nknow name subject
                        Teacher_Class? teacher_Class = context.teacher_Classes.FirstOrDefault(tc => tc.Teacher_ID == item.Teacher_ID
                        && tc.Class_ID == item.Class_ID);
                        if (teacher_Class == null)
                        {
                            
                            errors.Add(new { S = item, Error = $"لم يتم العثور على معلم أو فصل صالح للمعلم {item.Teacher_ID}" });

                            //to complate data
                            continue;

                        }
                        session1.TC_ID = teacher_Class.TC_ID;
                        var teacher = context.teachers.Include(t => t.subject).FirstOrDefault(it => it.UserId == (item.Teacher_ID));
                        if (teacher == null)
                        {
                            
                            errors.Add(new { S = item, Error = $"لم يتم العثور على المعلم {item.Teacher_ID}" });

                            continue;

                        }
                        session1.Material_Name = teacher.subject.Subject_Name;
                        #endregion

                        //Before adding the session class, make sure that the class is empty at that time 
                        //and check in the same list this session not exist 
                        bool existsInDb = (await context.sessions.Include(s => s.Teacher_Class).AnyAsync(s =>
                            //s.TC_ID == session1.TC_ID &&
                            s.Teacher_Class.Class_ID == item.Class_ID &&
                            s.period == session1.period &&
                            s.Start_Time == session1.Start_Time &&
                            s.Date == session1.Date
                            ));
                        /*++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/


                        //if room not online check the room is empty or not
                        bool empty_room = false;
                         if (session1.Room != "أونلاين")
                        {
                            empty_room = (await context.sessions.AnyAsync(s =>
                            //s.TC_ID == session1.TC_ID &&   // s.Teacher_Class.Class_ID == item.Class_ID &&
                            s.period == session1.period && s.Start_Time == session1.Start_Time &&
                            s.Date == session1.Date && s.Room == session1.Room
                            ));
                        }
                        /*****************************************************************************/
                        bool existsInList = (sessionsadd.Any(s =>
                           s.TC_ID == session1.TC_ID &&
                              s.period == session1.period &&
                                s.Start_Time == session1.Start_Time && s.Date == session1.Date));
                        /*****************************************************************************/

                        if (existsInDb || existsInList)
                        {
                             errors.Add(new { S = item, Error = $"لقد تم اختيار الوقت {session1.Start_Time} لنفس المجموعة في نفس اليوم {session1.Date.ToShortDateString()}" });

                            continue;
                        }
                        if (empty_room)
                        {
                            errors.Add(new { S = item, Error = $"لقد تم اختيار الوقت {session1.Start_Time} :لنفس القاعه{session1.Room} في نفس التاريخ {session1.Date.ToShortDateString()}" });

                            continue;
                        }
                        bool test_endtime = (session1.End_Time > 0 && session1.End_Time <= 12);
                        bool test_starttime = (session1.Start_Time > 0 && session1.Start_Time <= 12);
                        if (DateOnly.FromDateTime(session1.Date) >= DateOnly.FromDateTime(DateTime.Now )&& test_endtime
                            && test_starttime)
                        {
                            sessionsadd.Add(session1);

                        }
                        else
                        {
                             if (test_starttime == false && test_endtime == false)
                            {
                                errors.Add(new { S = item, Error = " من فضلك ادخل وقت ابتداءوانتهاء الحصه  بين رفمين 1:12" });

                                continue;
                            }
                            else if(test_endtime == false)
                            {
                                errors.Add(new { S = item, Error = " من فضلك ادخل وقت انتهاء الحصه  بين رفمين 1:12" });

                                continue;
                            }
                            else if(test_starttime == false) {
                                errors.Add(new { S = item, Error = " من فضلك ادخل وقت ابتداء الحصه  بين رفمين 1:12" });

                                continue;
                            }
                            else 
                            {
                                errors.Add(new { S = item, Error = " من فضلك ادخل تاريخ الحصه صحيحا لانك قمت بختيار تاريخ سابق لتاريخ اليوم" });

                                continue;

                            }
                            

                        }

                    }

                }
                if (sessionsadd.Count > 0)
                {

                    context.sessions.AddRange(sessionsadd);
                    await context.SaveChangesAsync();
                    //for display error for admin
                    if (errors.Count > 0)
                    {
                       
                        return StatusCode(201, new { Message = "تم إنشاء بعض الجلسات مع وجود أخطاء في أخرى", Errors = errors });

                    }
                    

                    return StatusCode(201, new { Message = "تم إضافة جميع الحصص بدون أخطاء" });


                }

                return BadRequest(errors);


            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }


        }


        #endregion

        /*****************************************************************************************/

        #region get all
        //get all sesion 
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> getAll()
        {
            List<Session> DataFromDB = await context.sessions.Include(s=>s.Teacher_Class).ToListAsync();
            //create list from session in DTO to display data without error
            List<Get_Sessions> DataForDispliy = new List<Get_Sessions>();

            try
            {
                if (DataFromDB != null)
                {
                    
                   
                    foreach (var item in DataFromDB)
                    {
                        //create for each new object
                        Get_Sessions sessions = new Get_Sessions();
                        sessions.Session_Title = item.Material_Name;
                        sessions.Session_ID = item.Session_ID;
                        sessions.Date = DateOnly.FromDateTime(item.Date);
                        sessions.Start_Time = item.Start_Time;
                        sessions.End_Time = item.End_Time;
                        sessions.Room = item.Room;
                        sessions.period = item.period;
                        var teacher_class = item.Teacher_Class;
                        if (teacher_class != null)
                        {
                            //sessions.TC_ID= teacher_class.TC_ID;
                            int class_id = (int)teacher_class.Class_ID;
                            //sessions.Class_ID = class_id;
                            var C_N = context.classes.FirstOrDefault(c => c.Class_ID == class_id);
                            sessions.Class_Name = C_N.Class_Name;
                            string tech = teacher_class.Teacher_ID;
                            //sessions.Teacher_ID = tech;
                            var teacher = context.teachers.Include(t => t.User).FirstOrDefault(t => t.UserId == tech);
                            string teach_Name = teacher.User.Full_Name;
                            sessions.Teacher_Name = teach_Name;
                        }
                        DataForDispliy.Add(sessions);

                    }

                    
                }
                 return Ok(DataForDispliy);
            }


            catch (Exception ex) {

                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");

            }
            }
        #endregion


        /**************************************************************************************/
        #region Get Session By ID 
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> getsessionbyid(int id)
        {
            try
            {
                var session = await context.sessions.Include(s=>s.Teacher_Class.Class).Include(s => s.Teacher_Class.Teacher.User).FirstOrDefaultAsync(s => s.Session_ID == id);
                if(session == null)
                {
                    return NotFound(new {message="لا يوجد حصص "});
                }
                Get_Sessions sessionsdto = new Get_Sessions 
                { 
                   Session_ID = session.Session_ID,
                   Session_Title=session.Material_Name,
                   Date = DateOnly.FromDateTime(session.Date),
                   Start_Time = session.Start_Time,
                   End_Time = session.End_Time,
                   Room = session.Room,
                   period = session.period,
                   Teacher_Name=session.Teacher_Class.Teacher.User.Full_Name,
                   Class_Name=session.Teacher_Class.Class.Class_Name
                };
                return Ok(sessionsdto);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }
        }
        #endregion

        #region get sessions table for one class
        [HttpGet("{class_name}/{stage}/{level}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> getby_class_name(string class_name,string stage,int level)
        {
            
            try 
            
            {
                int? class_id = context.classes.Where(c => c.Class_Name == class_name&&c.Level==level&&c.Stage==stage).Select(v => v.Class_ID).FirstOrDefault();

                if (class_id ==null|| class_id==0)
                {
                    return NotFound(new { message = " .من فضلك ادخل اسم مجموعه صح" });
                }
                
                   List<Session> DataFromDB = context.sessions.
                    Where(s=>s.Teacher_Class.Class_ID== class_id&& 
                    DateOnly.FromDateTime(s.Date)>= DateOnly.FromDateTime(DateTime.Now)).ToList();

                List<Get_Sessions>list = new List<Get_Sessions>();
                  if(DataFromDB == null)
                  {

                    return NotFound(new { message = $"{class_name}لا يوجد بعد حصص مجموعه " });
                  }
                //to add each object in
                foreach (var session in DataFromDB) {
                        //create object from get_Sessions
                        Get_Sessions data_display=new Get_Sessions();
                        data_display.Session_ID = session.Session_ID;
                        data_display.Session_Title=session.Material_Name;
                        data_display.End_Time= session.End_Time;
                        data_display.Start_Time= session.Start_Time;
                        data_display.Date= DateOnly.FromDateTime(session.Date);
                        data_display.Class_Name = class_name;

                        data_display.Room = session.Room;
                        data_display.period = session.period;
                    //to display teacher name
                    string? teach_name = context.teacher_Classes
                             .Where(t => t.TC_ID == session.TC_ID)
                             .Select(t => t.Teacher.User.Full_Name)
                             .FirstOrDefault();
                    data_display.Teacher_Name = teach_name ?? "غير معرف";
                       list.Add(data_display);
                    }

                   
                  
                    return Ok(list);
                

              



            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }

        }


        #endregion

        /*************************************************************************************/

        #region get session basic data 
        [HttpGet("{datefrom}/{dateto}")]
        [Authorize(Roles ="Admin")]

        public async Task<IActionResult> get_by_dates(DateTime datefrom, DateTime dateto)
        {
            //to convert datetime to only date
           var datefrom1= DateOnly.FromDateTime(datefrom);
           var dateto1= DateOnly.FromDateTime(dateto);
            if(datefrom1> dateto1)
            {
                return BadRequest(new { message = "من فضلك ادخل ترتيب التواريخ صيح يجب ان يكون تاريخ بدليه اصغر من التاريخ النهايه " });
            }
            try 
            {
                //to get list from sessions between this dates;
                List<Session> sessions_from_db = await context.sessions.Include(S=>S.Teacher_Class).
                    Where(s => DateOnly.FromDateTime(s.Date) <= dateto1 && DateOnly.FromDateTime(s.Date) >= datefrom1).ToListAsync();
                if (sessions_from_db.Count <= 0 || sessions_from_db == null) 
                { 
                    return NotFound(new { message = "لم يتم بعد انشاء جدول حصص لمجموعات في تلك الفترات " });
                
                
                }
               
                //create list from session to display
                List<Get_Sessions>list_session= new List<Get_Sessions>();
                foreach (var item in sessions_from_db) 
                {

                   
                        //create for each new object
                        Get_Sessions sessions = new Get_Sessions();
                        sessions.Session_Title = item.Material_Name;
                        sessions.Session_ID = item.Session_ID;
                        sessions.Date = DateOnly.FromDateTime(item.Date);
                        sessions.Start_Time = item.Start_Time;
                        sessions.End_Time = item.End_Time;
                        sessions.Room = item.Room;
                        sessions.period = item.period;
                        var teacher_class = item.Teacher_Class;
                        if (teacher_class != null)
                        {
                            //sessions.TC_ID= teacher_class.TC_ID;
                            int class_id = (int)teacher_class.Class_ID;
                            //sessions.Class_ID = class_id;
                            var C_N = context.classes.FirstOrDefault(c => c.Class_ID == class_id);
                            sessions.Class_Name = C_N.Class_Name;
                            string tech = teacher_class.Teacher_ID;
                            //sessions.Teacher_ID = tech;
                            var teacher = context.teachers.Include(t => t.User).FirstOrDefault(t => t.UserId == tech);
                            string teach_Name = teacher.User.Full_Name;
                            sessions.Teacher_Name = teach_Name;
                        }
                    list_session.Add(sessions);
                }

                return Ok(list_session);


            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }


        }
        #endregion



        /****************************************************************************************/

        #region delete 
        //deleteby determine date that you want delete from her to date that less then it

        [HttpDelete("{date:datetime}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> delete_by_date(DateTime date)
        {
            var dateTO=DateOnly.FromDateTime(date);
            try
            {
                //create list from database that store it session that date less from this date
                List<Session> list_deletes = await context.sessions.Include(s=>s.session_Students).Where(s => DateOnly.FromDateTime(s.Date) <= dateTO).ToListAsync();
                if(list_deletes.Count <= 0 || list_deletes == null)
                {
                    return NotFound(new { message = "تلك التاريخ لا يوجد له حصص مسجله تسبقه من اجل حذفها حتي تلك تاريخ" });
                }
                // delete list of session_student for each obj list of list_deletes
                foreach (var item in list_deletes)
                {
                    if (item.session_Students != null)
                    {
                        context.Session_Students.RemoveRange(item.session_Students);
                        context.SaveChanges();
                    }

                }
                

                context.sessions.RemoveRange(list_deletes);
                context.SaveChanges();
                return Ok(new { message = "تم الحذف بنجاح" });

            
            
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }


        }
        #endregion




        /***************************************************************************************/




        #region delete by id

        //to delete one session
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> delete_by_Id(int id)
        {

            try
            {
                // Session sess = await context.sessions.FirstOrDefaultAsync(s => s.Session_ID == id);

                Session sess = await context.sessions.Include(s=>s.session_Students).FirstOrDefaultAsync(s => s.Session_ID == id);
                if (sess == null)
                {
                    return NotFound(new { message = "من فضلك ادخل معرف صح لي حصصه التي تريد حذفها." });
                }
                // remove related session_Students
                if (sess.session_Students != null)
                {
                    context.Session_Students.RemoveRange(sess.session_Students);
                    context.SaveChanges();
                }

                context.sessions.Remove(sess);
                context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }

        }

        #endregion


        /*************************************************************************************/
        #region edit

        [HttpPut("int")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> edit(int id, Edit_Session e_session)
        {
            try
            {
                Session session = await context.sessions.FirstOrDefaultAsync(s => s.Session_ID == id);
                if (session == null)
                {
                    return NotFound(new { message = "من فضلك ادخل معرف حصه صحيح الذي تريد حذفه." });
                }

                session.Date= e_session.Date;
                session.End_Time= e_session.End_Time;
                session.Start_Time= e_session.Start_Time;
                session.period=e_session.period;
                session.Room = e_session.Room;

                context.sessions.Update(session);
                context.SaveChanges();
                //return Ok($"تم تعديل موعد حصه {session.Material_Name}  ");
                return Ok(new { message = $"تم تعديل موعد حصه {session.Material_Name}." });

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }
        }



        #endregion
        #region Get teachers for one class
        [HttpGet("{classId}/teachers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeachersByClass(int classId)
        {
            var teachers = await context.teacher_Classes
                .Where(tc => tc.Class_ID == classId)
                .Select(tc => new GetTeachersToClassDTO
                {
                    UserID = tc.Teacher_ID,
                    FullName = tc.Teacher.User.Full_Name

                })
                .ToListAsync();

            return Ok(teachers);

        }
        #endregion
    }
}
