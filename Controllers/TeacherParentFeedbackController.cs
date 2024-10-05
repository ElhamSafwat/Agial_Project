﻿using final_project_Api.DTO;
using final_project_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace final_project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherParentFeedbackController : ControllerBase
    {
        private readonly AgialContext _context;

        public TeacherParentFeedbackController(AgialContext context)
        {
            _context = context;
        }
    

        #region getAll
        // GET: api/ParentTeacherFeedback
        [HttpGet]
        public async Task<ActionResult<IEnumerable<teacherparentfeedbackgetall>>> GetFeedbacks()
        {
            var feedbacks = await _context.parent_Teacher_Feedbacks
                .Include(f => f.Teacher)
                .Include(f => f.Parent)
                .Include(f => f.Student)
                .Where(f => f.From == "Teacher") 
                .Select(feedback => new teacherparentfeedbackgetall
                {
                    TeacherName = feedback.Teacher.User.Full_Name,
                    ParentName = feedback.Parent.User.Full_Name,
                    StudentName = feedback.Student.User.Full_Name,
                    Date = feedback.date,
                    FeedBack = feedback.FeedBack,
                    From = feedback.From   
                })
                .ToListAsync();

            return Ok(feedbacks);
        }
        #endregion

        #region getparentid

        // GET: api/ParentTeacherFeedback/parent/{parentId}
        [HttpGet("parent/{parentId}")]
        public async Task<ActionResult<IEnumerable<getbyparentidfeedback>>> GetFeedbacksByParentId(string parentId)
        {
            var feedbacks = await _context.parent_Teacher_Feedbacks
                .Include(f => f.Teacher)
                .Include(f => f.Student)
                .Where(f => f.Parent_ID == parentId && f.From == "Teacher") 
                .Select(feedback => new getbyparentidfeedback
                {
                    TeacherName = feedback.Teacher.User.Full_Name,
                    StudentName = feedback.Student.User.Full_Name,
                    Date = feedback.date,
                    FeedBack = feedback.FeedBack,
                })
                .ToListAsync();

            if (feedbacks == null || !feedbacks.Any())
            {
                return NotFound("لا توجد ملاحظات لهذا الوالد.");
            }

            return Ok(feedbacks);
        }
        #endregion

        #region getbyteacherid

        // GET: api/ParentTeacherFeedback/teacher/{teacherId}
        [HttpGet("teacher/{teacherId}")]
        public async Task<ActionResult<IEnumerable<getbyteacheridfeedback>>> GetFeedbacksByTeacherId(string teacherId)
        {
            var feedbacks = await _context.parent_Teacher_Feedbacks
                .Include(f => f.Student)
                .Include(f => f.Parent)
                .Where(f => f.Teacher_ID == teacherId && f.From == "Teacher") 
                .Select(feedback => new getbyteacheridfeedback
                {
                    ParentName = feedback.Parent.User.Full_Name,
                    StudentName = feedback.Student.User.Full_Name,
                    Date = feedback.date,
                    FeedBack = feedback.FeedBack,
                })
                .ToListAsync();

            if (feedbacks == null || !feedbacks.Any())
            {
                return NotFound("لا توجد ملاحظات لهذا المعلم.");
            }

            return Ok(feedbacks);
        }
        #endregion

        #region GETByid

        // GET: api/ParentTeacherFeedback/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<teacherparentgetbyid>> GetFeedbackById(int id)
        {
            var feedback = await _context.parent_Teacher_Feedbacks
                .Include(f => f.Teacher)
                .Include(f => f.Parent)
                .Include(f => f.Student)
                .Where(f => f.Parent_Teacher_Feedback_Id== id)  
                .Select(f => new teacherparentgetbyid
                {
                    TeacherName = f.Teacher.User.Full_Name,  
                    ParentName = f.Parent.User.Full_Name,   
                    StudentName = f.Student.User.Full_Name, 
                    Date = f.date,
                    FeedBack = f.FeedBack,
                })
                .FirstOrDefaultAsync();

            if (feedback == null)
            {
                return NotFound($"لا توجد ملاحظة بهذا المعرف: {id}");
            }

            return Ok(feedback);
        }

        #endregion

        #region add feedback from parent to teacher 


        [HttpPost]
        public async Task<IActionResult> AddFeedback(Create_TeacherparentFeedback feedbackDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacher = await _context.teachers.Include(t => t.teacher_Classes).FirstOrDefaultAsync(t => t.UserId == feedbackDto.Teacher_ID);
            var parent = await _context.parent.FindAsync(feedbackDto.Parent_ID);
            var student = await _context.students.Include(s => s.Student_Classes).FirstOrDefaultAsync(s => s.UserId == feedbackDto.Student_ID);

            if (teacher == null || parent == null || student == null)
            {
                return NotFound("Teacher, Parent, or Student not found.");
            }
            //check this parent is parent this student 
            bool isparent = student.Parent_ID == feedbackDto.Parent_ID;
            if (isparent == false)
            {
                return BadRequest("هذا ليس ولدك");
            }
            //check this teacher is teaching this student 
            bool is_same_class = false;
            if (teacher.teacher_Classes != null && student.Student_Classes != null)
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
                    From = "Teacher",
                    date = feedbackDto.FeedbackDate
                };
                _context.parent_Teacher_Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                return Ok("تم اضافه التعليق بنجاح");
            }
            else
            {
                return BadRequest("تلك معلم لا يدرس لولدك هن فضلك اختار مدرس الصح ");
            }
        }
        #endregion

        #region Delete

        // DELETE: api/ParentTeacherFeedback/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.parent_Teacher_Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            _context.parent_Teacher_Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion


    }












}
