using Data.Interfaces;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Data.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromAddress;

        public EmailService(string smtpHost, int smtpPort, string smtpUser, string smtpPass, string fromAddress)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPass = smtpPass;
            _fromAddress = fromAddress;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage(_fromAddress, to, subject, body);
                try
                {
                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine("Email poslat!");
                }
                catch (SmtpException ex)
                {
                    Console.WriteLine($"SMTP Exception: {ex.StatusCode} - {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Exception: {ex.Message}");
                }
            }

            Console.WriteLine($"[EMAIL SENT] To: {to}, Subject: {subject}, Body: {body}");
        }
    }
}