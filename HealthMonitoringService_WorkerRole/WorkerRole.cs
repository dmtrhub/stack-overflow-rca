using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoringService
{
    public class WorkerRole : RoleEntryPoint
    {
        private Timer _timer;
        private HealthCheckRepository _healthRepo;
        private AlertEmailRepository _alertRepo;
        private HttpClient _httpClient;
        private IEmailService _emailService;
        private string _connectionString;
        private readonly string rootPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\.."));

        public override bool OnStart()
        {
            string logFilePath = Path.Combine(rootPath, "emails.log");
            Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
            Trace.Listeners.Add(new ConsoleTraceListener());
            Trace.AutoFlush = true;

            _connectionString = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");

            _healthRepo = new HealthCheckRepository(_connectionString);
            _alertRepo = new AlertEmailRepository(_connectionString);

            var apiServer = new AdminApiServer("http://localhost:5003/", _alertRepo);
            apiServer.Start();

            _httpClient = new HttpClient();

            string host = RoleEnvironment.GetConfigurationSettingValue("SmtpHost");
            int port = int.Parse(RoleEnvironment.GetConfigurationSettingValue("SmtpPort"));
            string user = RoleEnvironment.GetConfigurationSettingValue("SmtpUser");
            string pass = RoleEnvironment.GetConfigurationSettingValue("SmtpPass");
            string from = RoleEnvironment.GetConfigurationSettingValue("FromAddress");

            _emailService = new Data.Services.EmailService(host, port, user, pass, from);

            // Timer na 4 sekunde
            _timer = new Timer(async _ => await CheckServicesAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(4));

            return base.OnStart();
        }

        private async Task CheckServicesAsync()
        {
            string[] serviceUrls = new string[]
            {
                "http://localhost:51400", // StackOverflowService Web Role
                "http://localhost:8080/health-monitoring"  // NotificationService Worker Role
            };

            foreach (var url in serviceUrls)
            {
                string serviceName = GetServiceNameFromUrl(url);
                string status = "OK";

                try
                {
                    // Timeout 3 sekunde da ne visi
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    var response = await _httpClient.GetAsync(url, cts.Token);
                    Console.WriteLine($"{url} -> {(int)response.StatusCode} {response.ReasonPhrase}");
                    if (!response.IsSuccessStatusCode)
                        status = "NOT_OK";
                }
                catch
                {
                    status = "NOT_OK";
                }

                // Log u tabelu HealthCheck
                var health = new HealthCheck(serviceName, DateTime.UtcNow)
                {
                    Status = status,
                    ServiceName = serviceName
                };
                await _healthRepo.InsertHealthCheckAsync(health);

                if (status == "NOT_OK")
                {
                    await SendAlertsAsync(serviceName);
                }
            }
        }

        private string GetServiceNameFromUrl(string url)
        {
            if (url.Contains("51400")) return "StackOverflowService";
            if (url.Contains("8080")) return "NotificationService";
            return "UnknownService";
        }

        private async Task SendAlertsAsync(string service)
        {
            var emails = await _alertRepo.GetAllEmailsAsync();
            foreach (var email in emails)
            {
                string subject = $"[ALERT] {service} DOWN";
                string body = $"Servis {service} nije dostupan. Proverite status.";

                await _emailService.SendEmailAsync(email, subject, body);
                Trace.TraceInformation($"Email poslat: {email}, Subject: {subject}, Body: {body}");
            }
        }

        public override void OnStop()
        {
            _timer?.Dispose();
            base.OnStop();
        }
    }
}