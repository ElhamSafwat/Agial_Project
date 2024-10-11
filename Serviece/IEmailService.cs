namespace final_project_Api.Serviece
{
    public interface IEmailService
    {
        Task SendRegistrationEmail(string toEmail, string username, string password);
    }
}
