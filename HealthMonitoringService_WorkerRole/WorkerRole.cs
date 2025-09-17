using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Data.Repositories;
using Data.Entities;
using Data.Helpers;

namespace HealthMonitoringService_WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private Timer _timer;
        private HealthCheckRepository _healthRepo;
        private AlertEmailRepository _alertRepo;
        private HttpClient _httpClient;
        private string _connectionString;

        public override bool OnStart()
        {
            _connectionString = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");

            _healthRepo = new HealthCheckRepository(_connectionString);
            _alertRepo = new AlertEmailRepository(_connectionString);

            _httpClient = new HttpClient();

            // Timer na 4 sekunde
            _timer = new Timer(async _ => await CheckServicesAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(4));

            return base.OnStart();
        }

        private async Task CheckServicesAsync()
        {
            string[] serviceUrls = new string[]
            {
                "http://localhost:80/health-monitoring", // StackOverflowService Web Role
                "http://localhost:81/health-monitoring"  // NotificationService Worker Role
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

        // Pomoćna funkcija za izdvajanje imena servisa iz URL-a
        private string GetServiceNameFromUrl(string url)
        {
            if (url.Contains("80")) return "StackOverflowService";
            if (url.Contains("81")) return "NotificationService";
            return "UnknownService";
        }

        private async Task SendAlertsAsync(string service)
        {
            var emails = await _alertRepo.GetAllEmailsAsync();
            foreach (var email in emails)
            {
                string subject = $"[ALERT] {service} DOWN";
                string body = $"Servis {service} nije dostupan. Proverite status.";
                EmailHelper.SendEmail(email, subject, body);
            }
        }

        public override void OnStop()
        {
            _timer?.Dispose();
            base.OnStop();
        }
    }
}