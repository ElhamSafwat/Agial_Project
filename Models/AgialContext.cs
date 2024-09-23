using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace final_project_Api.Models
{
    public class AgialContext:IdentityDbContext<ApplicationUser>
    {
        public AgialContext() { }
        public AgialContext(DbContextOptions<AgialContext> options) : base(options) { }
        public DbSet<Admin> admins { get; set; }
        public DbSet<Class> classes { get; set; }
        public DbSet<Exam> exam { get; set; }
        public DbSet<Parent> parent { get; set; }
        public DbSet<Parent_Teacher_Feedback> parent_Teacher_Feedbacks { get; set; }
        public DbSet<Payment> payments { get; set; }
        public DbSet<Session> sessions { get; set; }
        public DbSet<Session_Student> Session_Students { get; set; }
        public DbSet<Student> students { get; set; }
        public DbSet<Student_Class> student_classes { get; set; }
        public DbSet<Student_Exam> student_Exams { get; set; }
        public DbSet<Student_Teacher_Feedback> student_Teacher_Feedbacks { get;set; }
        public DbSet<Subject> subjects { get; set; }
        public DbSet<Teacher> teachers { get; set; }
        public DbSet<Teacher_Class> teacher_Classes { get; set; }
        public DbSet<Teacher_Stage> teacher_Stages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Parent_Teacher_Feedback>()
            //    .HasKey(e => new { e.Teacher_ID, e.Parent_ID });
            //modelBuilder.Entity<Parent_Teacher_Feedback>()
            //    .HasOne<Teacher>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Teacher_ID).OnDelete(DeleteBehavior.NoAction);
                
            //modelBuilder.Entity<Parent_Teacher_Feedback>()
            //    .HasOne<Parent>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Parent_ID).OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Student_Teacher_Feedback>()
            //   .HasKey(e => new { e.Student_ID, e.Teacher_ID });
            //modelBuilder.Entity<Student_Teacher_Feedback>()
            //    .HasOne<Teacher>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Teacher_ID).OnDelete(DeleteBehavior.NoAction);
            //modelBuilder.Entity<Student_Teacher_Feedback>()
            //    .HasOne<Student>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Student_ID).OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Session_Student>()
            //  .HasKey(e => new { e.Student_ID, e.Session_ID });
            //modelBuilder.Entity<Session_Student>()
            //    .HasOne<Session>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Session_ID).OnDelete(DeleteBehavior.NoAction);
            //modelBuilder.Entity<Session_Student>()
            //    .HasOne<Student>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Student_ID).OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Student_Class>()
            //  .HasKey(e => new { e.Student_ID, e.Class_ID });
            //modelBuilder.Entity<Student_Class>()
            //    .HasOne<Class>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Class_ID).OnDelete(DeleteBehavior.NoAction);
            //modelBuilder.Entity<Student_Class>()
            //    .HasOne<Student>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Student_ID).OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Student_Exam>()
            //  .HasKey(e => new { e.Student_ID, e.Exam_ID });
            //modelBuilder.Entity<Student_Exam>()
            //    .HasOne<Exam>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Exam_ID).OnDelete(DeleteBehavior.NoAction);
            //modelBuilder.Entity<Student_Exam>()
            //    .HasOne<Student>()
            //    .WithMany()
            //    .HasForeignKey(e => e.Student_ID).OnDelete(DeleteBehavior.NoAction);

        }
    }
}
