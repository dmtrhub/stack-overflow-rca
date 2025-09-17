using System;
using System.Threading.Tasks;

namespace Data.Helpers
{
    public static class EmailHelper
    {
        public static async Task SendEmailAsync(string to, string subject, string body)
        {
            // Mock slanje mejla (za testiranje)
            Console.WriteLine($"[MOCK SEND] To: {to}, Subject: {subject}, Body: {body}");

            await Task.CompletedTask;

            // Pravi slanje mejla preko SendGrid-a (Sandbox mod)
            //var apiKey = "API_KEY_FOR_SANDBOX";
            //var client = new SendGridClient(apiKey);

            //var from = new EmailAddress("test@local.com", "HealthMonitoring");
            //var toEmail = new EmailAddress(to);
            //var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);

            //msg.MailSettings = new MailSettings
            //{
            //    SandboxMode = new SandboxMode { Enable = true }
            //};

            //var response = await client.SendEmailAsync(msg);

            //Console.WriteLine($"[SANDBOX] Email to {to} queued. Status code: {response.StatusCode}");
        }
    }
}