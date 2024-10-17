
using System.Net.Mail;
using System.Net;

namespace final_project_Api.Serviece
{
    public class EmailDeleteFeedback : IEmailFeedback
    {

        private readonly IConfiguration _configuration;

        public EmailDeleteFeedback(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        Task IEmailFeedback.SendDeleteEmail(string toEmail, string username, string becouse)
        {
            var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
            {
                Port = int.Parse(_configuration["Smtp:Port"]),
                Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:From"]),
                Subject = "تم  حذف تعليقك  ",
                Body = $"مرحبًا،\n\n  {username}\n لقد تم حذف تعليقك والسبب: {becouse}",
                IsBodyHtml = false,
            };
            mailMessage.To.Add(toEmail);

             smtpClient.Send(mailMessage);
            return Task.CompletedTask;
        }
    }
}
