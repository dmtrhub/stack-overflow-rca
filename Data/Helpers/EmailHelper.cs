using System.Net;
using System.Net.Mail;

namespace Data.Helpers
{
    public static class EmailHelper
    {
        public static void SendEmail(string to, string subject, string body)
        {
            using (var smtp = new SmtpClient("localhost", 25))
            {
                smtp.EnableSsl = false; // lokalno ne treba SSL
                var mail = new MailMessage("test@local.dev", to, subject, body);
                smtp.Send(mail);
            }
        }
    }
}