using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace final_project_Api.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Full_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "classes",
                columns: table => new
                {
                    Class_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Class_Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classes", x => x.Class_ID);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    Subject_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Subject_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.Subject_ID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    User_Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.User_Id);
                    table.ForeignKey(
                        name: "FK_admins_AspNetUsers_User_Id",
                        column: x => x.User_Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parent",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parent", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_parent_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teachers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HireDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    Subject_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teachers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_teachers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teachers_subjects_Subject_ID",
                        column: x => x.Subject_ID,
                        principalTable: "subjects",
                        principalColumn: "Subject_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
     name: "students",
     columns: table => new
     {
         UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
         enrollmentDate = table.Column<DateTime>(type: "date", nullable: false),
         Stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
         Level = table.Column<int>(type: "int", nullable: false),
         Parent_ID = table.Column<string>(type: "nvarchar(450)", nullable: false)
     },
     constraints: table =>
     {
         table.PrimaryKey("PK_students", x => x.UserId);
         table.ForeignKey(
             name: "FK_students_AspNetUsers_UserId",
             column: x => x.UserId,
             principalTable: "AspNetUsers",
             principalColumn: "Id",
             onDelete: ReferentialAction.Cascade); 
         table.ForeignKey(
             name: "FK_students_parent_Parent_ID",
             column: x => x.Parent_ID,
             principalTable: "parent",
             principalColumn: "UserId",
             onDelete: ReferentialAction.NoAction);
     });


            migrationBuilder.CreateTable(
                name: "exam",
                columns: table => new
                {
                    Exam_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Exam_Date = table.Column<DateTime>(type: "date", nullable: false),
                    Start_Time = table.Column<float>(type: "real", nullable: false),
                    End_Time = table.Column<float>(type: "real", nullable: false),
                    Min_Degree = table.Column<int>(type: "int", nullable: false),
                    Max_Degree = table.Column<int>(type: "int", nullable: false),
                    class_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    subject_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Teacher_ID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam", x => x.Exam_ID);
                    table.ForeignKey(
                        name: "FK_exam_teachers_Teacher_ID",
                        column: x => x.Teacher_ID,
                        principalTable: "teachers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "teacher_Classes",
                columns: table => new
                {
                    TC_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Teacher_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Class_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_Classes", x => x.TC_ID);
                    table.ForeignKey(
                        name: "FK_teacher_Classes_classes_Class_ID",
                        column: x => x.Class_ID,
                        principalTable: "classes",
                        principalColumn: "Class_ID");
                    table.ForeignKey(
                        name: "FK_teacher_Classes_teachers_Teacher_ID",
                        column: x => x.Teacher_ID,
                        principalTable: "teachers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "teacher_Stages",
                columns: table => new
                {
                    Teacher_Stage_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Teacher_Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_Stages", x => x.Teacher_Stage_Id);
                    table.ForeignKey(
                        name: "FK_teacher_Stages_teachers_Teacher_Id",
                        column: x => x.Teacher_Id,
                        principalTable: "teachers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parent_Teacher_Feedbacks",
                columns: table => new
                {
                    Parent_Teacher_Feedback_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Teacher_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Parent_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Student_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    FeedBack = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parent_Teacher_Feedbacks", x => x.Parent_Teacher_Feedback_Id);
                    table.ForeignKey(
                        name: "FK_parent_Teacher_Feedbacks_parent_Parent_ID",
                        column: x => x.Parent_ID,
                        principalTable: "parent",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_parent_Teacher_Feedbacks_students_Student_ID",
                        column: x => x.Student_ID,
                        principalTable: "students",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_parent_Teacher_Feedbacks_teachers_Teacher_ID",
                        column: x => x.Teacher_ID,
                        principalTable: "teachers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Payment_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    Payment_Method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Student_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Admin_ID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Payment_ID);
                    table.ForeignKey(
                        name: "FK_payments_admins_Admin_ID",
                        column: x => x.Admin_ID,
                        principalTable: "admins",
                        principalColumn: "User_Id");
                    table.ForeignKey(
                        name: "FK_payments_students_Student_ID",
                        column: x => x.Student_ID,
                        principalTable: "students",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "student_classes",
                columns: table => new
                {
                    Student_Class_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Student_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Class_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_classes", x => x.Student_Class_Id);
                    table.ForeignKey(
                        name: "FK_student_classes_classes_Class_ID",
                        column: x => x.Class_ID,
                        principalTable: "classes",
                        principalColumn: "Class_ID");
                    table.ForeignKey(
                        name: "FK_student_classes_students_Student_ID",
                        column: x => x.Student_ID,
                        principalTable: "students",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "student_Teacher_Feedbacks",
                columns: table => new
                {
                    Student_Teacher_Feedback_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Teacher_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Student_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_Teacher_Feedbacks", x => x.Student_Teacher_Feedback_Id);
                    table.ForeignKey(
                        name: "FK_student_Teacher_Feedbacks_students_Student_ID",
                        column: x => x.Student_ID,
                        principalTable: "students",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_student_Teacher_Feedbacks_teachers_Teacher_ID",
                        column: x => x.Teacher_ID,
                        principalTable: "teachers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "student_Exams",
                columns: table => new
                {
                    Student_Exam_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Student_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Exam_ID = table.Column<int>(type: "int", nullable: true),
                    Degree = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_Exams", x => x.Student_Exam_Id);
                    table.ForeignKey(
                        name: "FK_student_Exams_exam_Exam_ID",
                        column: x => x.Exam_ID,
                        principalTable: "exam",
                        principalColumn: "Exam_ID");
                    table.ForeignKey(
                        name: "FK_student_Exams_students_Student_ID",
                        column: x => x.Student_ID,
                        principalTable: "students",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Session_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Session_Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Room = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    End_Time = table.Column<float>(type: "real", nullable: false),
                    Start_Time = table.Column<float>(type: "real", nullable: false),
                    period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TC_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Session_ID);
                    table.ForeignKey(
                        name: "FK_sessions_teacher_Classes_TC_ID",
                        column: x => x.TC_ID,
                        principalTable: "teacher_Classes",
                        principalColumn: "TC_ID");
                });

            migrationBuilder.CreateTable(
                name: "Session_Students",
                columns: table => new
                {
                    Session_Student_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Session_ID = table.Column<int>(type: "int", nullable: true),
                    Student_ID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Assignment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attendance = table.Column<bool>(type: "bit", nullable: false),
                    Degree = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session_Students", x => x.Session_Student_Id);
                    table.ForeignKey(
                        name: "FK_Session_Students_sessions_Session_ID",
                        column: x => x.Session_ID,
                        principalTable: "sessions",
                        principalColumn: "Session_ID");
                    table.ForeignKey(
                        name: "FK_Session_Students_students_Student_ID",
                        column: x => x.Student_ID,
                        principalTable: "students",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_exam_Teacher_ID",
                table: "exam",
                column: "Teacher_ID");

            migrationBuilder.CreateIndex(
                name: "IX_parent_Teacher_Feedbacks_Parent_ID",
                table: "parent_Teacher_Feedbacks",
                column: "Parent_ID");

            migrationBuilder.CreateIndex(
                name: "IX_parent_Teacher_Feedbacks_Student_ID",
                table: "parent_Teacher_Feedbacks",
                column: "Student_ID");

            migrationBuilder.CreateIndex(
                name: "IX_parent_Teacher_Feedbacks_Teacher_ID",
                table: "parent_Teacher_Feedbacks",
                column: "Teacher_ID");

            migrationBuilder.CreateIndex(
                name: "IX_payments_Admin_ID",
                table: "payments",
                column: "Admin_ID");

            migrationBuilder.CreateIndex(
                name: "IX_payments_Student_ID",
                table: "payments",
                column: "Student_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Session_Students_Session_ID",
                table: "Session_Students",
                column: "Session_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Session_Students_Student_ID",
                table: "Session_Students",
                column: "Student_ID");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_TC_ID",
                table: "sessions",
                column: "TC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_student_classes_Class_ID",
                table: "student_classes",
                column: "Class_ID");

            migrationBuilder.CreateIndex(
                name: "IX_student_classes_Student_ID",
                table: "student_classes",
                column: "Student_ID");

            migrationBuilder.CreateIndex(
                name: "IX_student_Exams_Exam_ID",
                table: "student_Exams",
                column: "Exam_ID");

            migrationBuilder.CreateIndex(
                name: "IX_student_Exams_Student_ID",
                table: "student_Exams",
                column: "Student_ID");

            migrationBuilder.CreateIndex(
                name: "IX_student_Teacher_Feedbacks_Student_ID",
                table: "student_Teacher_Feedbacks",
                column: "Student_ID");

            migrationBuilder.CreateIndex(
                name: "IX_student_Teacher_Feedbacks_Teacher_ID",
                table: "student_Teacher_Feedbacks",
                column: "Teacher_ID");

            migrationBuilder.CreateIndex(
                name: "IX_students_Parent_ID",
                table: "students",
                column: "Parent_ID");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_Classes_Class_ID",
                table: "teacher_Classes",
                column: "Class_ID");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_Classes_Teacher_ID",
                table: "teacher_Classes",
                column: "Teacher_ID");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_Stages_Teacher_Id",
                table: "teacher_Stages",
                column: "Teacher_Id");

            migrationBuilder.CreateIndex(
                name: "IX_teachers_Subject_ID",
                table: "teachers",
                column: "Subject_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "parent_Teacher_Feedbacks");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "Session_Students");

            migrationBuilder.DropTable(
                name: "student_classes");

            migrationBuilder.DropTable(
                name: "student_Exams");

            migrationBuilder.DropTable(
                name: "student_Teacher_Feedbacks");

            migrationBuilder.DropTable(
                name: "teacher_Stages");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "exam");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "teacher_Classes");

            migrationBuilder.DropTable(
                name: "parent");

            migrationBuilder.DropTable(
                name: "classes");

            migrationBuilder.DropTable(
                name: "teachers");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "subjects");
        }
    }
}
