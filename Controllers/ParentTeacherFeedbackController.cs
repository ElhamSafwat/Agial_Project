using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentTeacherFeedbackController : ControllerBase
    {
        public AgialContext context;
        public ParentTeacherFeedbackController(AgialContext _context)
        {
            context = _context;
        }
        #region add feedback from parent to teacher


        [HttpPost]
        public async Task<IActionResult> AddFeedback(Create_ParentTeacherFeedback feedbackDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacher = await context.teachers.Include(t=>t.teacher_Classes).FirstOrDefaultAsync(t => t.UserId ==feedbackDto.Teacher_ID);
            var parent = await context.parent.FindAsync(feedbackDto.Parent_ID);
            var student = await context.students.Include(s=>s.Student_Classes).FirstOrDefaultAsync(s=>s.UserId==feedbackDto.Student_ID);

            if (teacher == null || parent == null || student == null)
            {
                return NotFound(new { message = "Teacher, Parent, or Student not found." });
            }
            //check this parent is parent this student
            bool isparent = student.Parent_ID == feedbackDto.Parent_ID;
            if (isparent==false) 
            { 
              return BadRequest(new { massege = "هذا ليس ولدك" });
            }
            //check this teacher is teaching this student
            bool is_same_class = false;
            if (teacher.teacher_Classes !=null && student.Student_Classes != null)
            {
                
                foreach (var item in teacher.teacher_Classes)
                {
                    foreach (var item1 in student.Student_Classes)
                    {
                        if (item1.Class_ID == item.Class_ID)
                        {
                            is_same_class = true;
                            break;
                        }
                    }

                }
            }
            if (is_same_class)
            {
                var feedback = new Parent_Teacher_Feedback
                {
                    Teacher_ID = feedbackDto.Teacher_ID,
                    Parent_ID = feedbackDto.Parent_ID,
                    Student_ID = feedbackDto.Student_ID,
                    FeedBack = feedbackDto.FeedBack,
                    From = "parent",
                    date = feedbackDto.FeedbackDate
                };
                context.parent_Teacher_Feedbacks.Add(feedback);
                await context.SaveChangesAsync();

                return Ok(new { massege = "تم اضافه التعليق بنجاح" });
            }
            else
            {
                return BadRequest(new { massege = "تلك معلم لا يدرس لولدك هن فضلك اختار مدرس الصح "});
                }
        }
        #endregion



        #region edit
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, Edit_ParentTeacherFeedback feedbackDto)
        {
            var entity_feed = await context.parent_Teacher_Feedbacks.FirstOrDefaultAsync(f => f.Parent_Teacher_Feedback_Id == id);
            try
            {
                if (entity_feed == null)
                {

                    return NotFound(new { message = "من فضلك ادخل معرف صحيح " });
                }

                entity_feed.FeedBack = feedbackDto.FeedBack;

                entity_feed.date = DateTime.Now;
                context.parent_Teacher_Feedbacks.Update(entity_feed);
                await context.SaveChangesAsync();
                return Ok(new { message = "تم تعديل بنجاح." });
            }

            catch (DbUpdateConcurrencyException)
            {

                return BadRequest(new { message = "من فضلك ادخل داتا صحيحه " });

            }

           
        }


        #endregion

        #region get all for parent
        //Parents presented the comments he wrote for admin
        //display for only admin
        [HttpGet]
        public async Task<IActionResult> Get_all()
        {
            List<Parent_Teacher_Feedback> parent_Teacher_Feedbacks = await context.parent_Teacher_Feedbacks.
                Include(f => f.Teacher.User).Include(f => f.Parent.User).Include(f => f.Student.User).
                Where(p => p.From == "parent").ToListAsync();
            List<Get_Parent_Teacher_Feedback> lists = new List<Get_Parent_Teacher_Feedback>();
            try
            {
                if (parent_Teacher_Feedbacks != null)
                {
                    foreach (var item in parent_Teacher_Feedbacks)
                    {
                        Get_Parent_Teacher_Feedback p_t_f = new Get_Parent_Teacher_Feedback();
                        p_t_f.Id=item.Parent_Teacher_Feedback_Id;
                        p_t_f.Teacher_Name = item.Teacher.User.Full_Name;
                        p_t_f.Student_Name = item.Student.User.Full_Name;
                        p_t_f.parent_Name = item.Parent.User.Full_Name;
                        p_t_f.date = DateOnly.FromDateTime(item.date);
                        p_t_f.Feedback = item.FeedBack;


                        lists.Add(p_t_f);



                    }

                }
               
                    return Ok(lists);
                
                
            }
            catch (DbUpdateConcurrencyException)
            {

                return NotFound(new {message= "من فضلك ادخل داتا صحيحه" });

            }



        }

        #endregion



        //can delete commint parent and admin
        #region delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> delete(int id)
        {
            Parent_Teacher_Feedback p = await context.parent_Teacher_Feedbacks.FirstOrDefaultAsync(p => p.Parent_Teacher_Feedback_Id == id);
            if (p != null)
            {
                context.parent_Teacher_Feedbacks.Remove(p);
                context.SaveChanges();
                return NoContent();

            }
            else
            {
                return BadRequest(new {message= "من فضلك اختار صح" });
            }

        }
        #endregion


        #region get by teacher_id
        //display commit in page teacher that come parent
        //display only for teacher
        [HttpGet("Teacher/{id}")]
        public async Task<IActionResult> get_teach_id(string id)
        {
            
                List<Parent_Teacher_Feedback> parent_Teacher_Feedbacks = await context.parent_Teacher_Feedbacks.
                    Include(f => f.Teacher.User).Include(f => f.Parent.User).Include(f => f.Student.User).
                    Where(p => p.From == "parent" && p.Teacher_ID == id).ToListAsync();
                List<Get_feedback__to__teacher_from_parent> lists = new List<Get_feedback__to__teacher_from_parent>();
            try
            {
                if (parent_Teacher_Feedbacks != null)
                {
                    foreach (var item in parent_Teacher_Feedbacks)
                    {
                        Get_feedback__to__teacher_from_parent p_t_f = new Get_feedback__to__teacher_from_parent();
                        p_t_f.Id = item.Parent_Teacher_Feedback_Id;

                        p_t_f.Student_Name = item.Student.User.Full_Name;
                        p_t_f.parent_Name = item.Parent.User.Full_Name;
                        p_t_f.date = DateOnly.FromDateTime(item.date);
                        p_t_f.Feedback = item.FeedBack;
                        lists.Add(p_t_f);
                    }

                }
                if (lists.Count > 0)
                {
                    return Ok(lists);
                }
                else
                {
                    return BadRequest(new { message = "من فضلك ادخل معرف صحيح" });
                }
            }
            catch (Exception e)
            {

                return NotFound(new { message = "من فضلك ادخل معرف صحيح" });

            }


        }

        #endregion






        #region dispay all comment for parent
        // display for parent his comments
        //display only parent
        [HttpGet("parent/{parent_id}")]
        public async Task<IActionResult> get_for_parent(string parent_id)
        {
            List<Parent_Teacher_Feedback> get_feedback=await context.parent_Teacher_Feedbacks.
                Include(f => f.Teacher.User).Include(f => f.Student.User).Include(f => f.Parent.User).
                Where(p => p.From == "Teacher"&&p.Parent_ID== parent_id).ToListAsync();
            List<Get_feedback_by_parent> lists = new List<Get_feedback_by_parent>();
            try
            {
                if (get_feedback != null)
                {
                    foreach (var item in get_feedback)
                    {
                        Get_feedback_by_parent p_t_f = new Get_feedback_by_parent();
                        p_t_f.Id=item.Parent_Teacher_Feedback_Id;
                        
                        p_t_f.Teacher_Name = item.Teacher.User.Full_Name;
                        p_t_f.Student_Name = item.Student.User.Full_Name;
                        p_t_f.date = DateOnly.FromDateTime(item.date);
                        p_t_f.Feedback = item.FeedBack;


                        lists.Add(p_t_f);



                    }

                }
                
                    return Ok(lists);
                
            }
            catch (Exception e)
            {

                return NotFound(new {message= "من فضلك ادخل معرف اب صحيح " });

            }

        }
        #endregion


        #region data of dropdown teacher
        /*
        //to get date will put in drop down to display teacher that realate student for each parent
       
        
        

        [HttpGet("ByParent/{parentId}")]
        public async Task<IActionResult> GetTeachersByParent(string parentId)
        {
            //reach for student realate his parent
            var students = await context.students
                                         .Where(s => s.Parent_ID == parentId)
                                         .Select(s => s.UserId)
                                         .ToListAsync();

            if (students == null || students.Count == 0)
            {
                return NotFound("تلك الوالد  ليس لديه ابناء بعد .");
            }
            //reach for class realate his studens
            var groups = await context.student_classes
                                       .Where(sg => students.Contains(sg.Student_ID))
                                       .Select(sg => sg.Class_ID)
                                       .Distinct()
                                       .ToListAsync();

            if (groups == null || groups.Count == 0)
            {
                return NotFound("تلك ابناء لم يتم تسجيلها بعد في مجموعات.");
            }

            // reach for teacher realate this class
            var teachers = await context.teacher_Classes
                                         .Where(tg => groups.Contains(tg.Class_ID))
                                         .Select(tg => new {
                                             teacher_id = tg.Teacher_ID,
                                             teacher_name = tg.Teacher.User.Full_Name
                                         })
                                         .Distinct()
                                         .ToListAsync();

            if (teachers == null || teachers.Count == 0)
            {
                return NotFound("تلك مجموعات لم تحصل بعد علي معلمين .");
            }

            return Ok(teachers);
        }

        

        */
        #endregion



        #region search by class name ,stage and level
        //only admin

        [HttpGet("{class_name}/{stage}/{level}/{from}")]
        public async Task<IActionResult> search_by_name(string class_name,string stage,int level,string from)
        {
            var get_class= await context.classes.Include(c=>c.Teacher_Class).Where(c=>c.Class_Name==class_name&&c.Stage==stage&&c.Level== level).FirstOrDefaultAsync(); ;

            if (get_class != null) 
            {


                List<Parent_Teacher_Feedback> get_feedback = await context.parent_Teacher_Feedbacks.
                    Include(f => f.Teacher.User).Include(f => f.Student.User).Include(f => f.Parent.User).
                    Include(f => f.Teacher.teacher_Classes).
                    Where(p => p.From == from && p.Teacher.teacher_Classes.Any(t => t.Class_ID == get_class.Class_ID)).ToListAsync();

                //using get_parent_teacher_feedback to all display all feedback
                List<Get_Parent_Teacher_Feedback> all_feedback = new List<Get_Parent_Teacher_Feedback>();
                if (get_feedback != null)
                {
                    foreach (var item in get_feedback)
                    {
                        Get_Parent_Teacher_Feedback feed=new Get_Parent_Teacher_Feedback();
                        feed.date=DateOnly.FromDateTime(item.date);
                        feed.Id = item.Parent_Teacher_Feedback_Id;
                        feed.parent_Name = item.Parent.User.Full_Name;
                        feed.Student_Name = item.Student.User.Full_Name;
                        feed.Teacher_Name = item.Teacher.User.Full_Name;
                        feed.Feedback = item.FeedBack;
                        all_feedback.Add(feed);
                    }

                }
                else
                {
                    return BadRequest(new {message= "تلك مجموعه لا تمتلك فيدباك بعد" });
                    }

                return Ok(all_feedback);
            }
            else
            {
                return BadRequest(new { message = "من فضلك  ادخل اسم  مجموعه وقيمه  مرحله والمستوي داتا صحيحه حتي نسطيع ارجع الاراء لتلك مجموعه" });
            }


        }

        #endregion



    }

}

