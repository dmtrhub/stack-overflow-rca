using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflowApp_Data.Helpers
{
    public static class EmailHelper
    {
        public static void SendEmail(string to, string subject, string body)
        {
            using (var smtp = new SmtpClient("smtp.yourserver.com", 25)) // ili testni SMTP
            {
                smtp.Credentials = new NetworkCredential("username", "password");
                smtp.EnableSsl = true;

                var mail = new MailMessage("from@domain.com", to, subject, body);
                smtp.Send(mail);
            }
        }
    }
}
