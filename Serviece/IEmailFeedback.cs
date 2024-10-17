namespace final_project_Api.Serviece
{
    public interface IEmailFeedback
    {
        Task SendDeleteEmail(string toEmail, string username, string becouse);
    }
}
