using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoringService_WorkerRole;

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
        private ServiceHost _wcfServiceHost;

        public override bool OnStart()
        {
            Trace.WriteLine("HealthMonitoringService starting...");

            _connectionString = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");

            _healthRepo = new HealthCheckRepository(_connectionString);
            _alertRepo = new AlertEmailRepository(_connectionString);

            StartWcfService();

            _httpClient = new HttpClient();

            string host = RoleEnvironment.GetConfigurationSettingValue("SmtpHost");
            int port = int.Parse(RoleEnvironment.GetConfigurationSettingValue("SmtpPort"));
            string user = RoleEnvironment.GetConfigurationSettingValue("SmtpUser");
            string pass = RoleEnvironment.GetConfigurationSettingValue("SmtpPass");
            string from = RoleEnvironment.GetConfigurationSettingValue("FromAddress");

            _emailService = new Data.Services.EmailService(host, port, user, pass, from);

            // Timer na 4 sekunde
            _timer = new Timer(async _ => await CheckServicesAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(4));

            Trace.WriteLine("HealthMonitoringService started successfully");
            return base.OnStart();
        }

        private void StartWcfService()
        {
            try
            {
                var wcfService = new EmailSubscriptionService(_alertRepo);
                _wcfServiceHost = new ServiceHost(wcfService);

                _wcfServiceHost.Open();

                Trace.WriteLine("WCF Service started ");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error starting WCF service: {ex.Message}");
                throw;
            }
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

                    Trace.WriteLine($"{url} -> {(int)response.StatusCode} {response.ReasonPhrase}");

                    if (!response.IsSuccessStatusCode)
                        status = "NOT_OK";
                }
                catch (Exception ex)
                {
                    status = "NOT_OK";
                    Trace.WriteLine($"Error checking service {serviceName}: {ex.Message}");
                }

                // Log u tabelu HealthCheck
                var health = new HealthCheck(serviceName, DateTime.UtcNow)
                {
                    Status = status,
                    ServiceName = serviceName
                };

                try
                {
                    await _healthRepo.InsertHealthCheckAsync(health);
                    Trace.WriteLine($"Health check recorded for {serviceName}: {status}");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error recording health check for {serviceName}: {ex.Message}");
                }

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
            try
            {
                var emails = await _alertRepo.GetAllEmailsAsync();
                Trace.WriteLine($"Sending alerts for {service} to {emails.Count} emails");

                foreach (var email in emails)
                {
                    string subject = $"[ALERT] {service} DOWN";
                    string body = $"Servis {service} nije dostupan. Proverite status.";

                    await _emailService.SendEmailAsync(email, subject, body);
                    Trace.WriteLine($"Alert email sent to: {email}, Service: {service}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error sending alerts for {service}: {ex.Message}");
            }
        }

        public override void OnStop()
        {
            Trace.WriteLine("HealthMonitoringService stopping...");

            _timer?.Dispose();
            _wcfServiceHost?.Close();
            _httpClient?.Dispose();

            Trace.WriteLine("HealthMonitoringService stopped");
            base.OnStop();
        }
    }
}