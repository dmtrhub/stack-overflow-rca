using Data.Entities;
using Data.Helpers;
using Data.Repositories;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
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
        private string _connectionString;

        public override bool OnStart()
        {
            _connectionString = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");

            _healthRepo = new HealthCheckRepository(_connectionString);
            _alertRepo = new AlertEmailRepository(_connectionString);

            var apiServer = new AdminApiServer("http://localhost:5003/", _alertRepo);
            apiServer.Start();

            _httpClient = new HttpClient();

            // Timer na 4 sekunde
            _timer = new Timer(async _ => await CheckServicesAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(4));

            return base.OnStart();
        }

        private async Task CheckServicesAsync()
        {
            string[] serviceUrls = new string[]
            {
                "http://localhost:51400", // StackOverflowService Web Role
                // NotificationService Worker Role
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
            //if (url.Contains()) return "NotificationService";
            return "UnknownService";
        }

        private async Task SendAlertsAsync(string service)
        {
            var emails = await _alertRepo.GetAllEmailsAsync();
            foreach (var email in emails)
            {
                string subject = $"[ALERT] {service} DOWN";
                string body = $"Servis {service} nije dostupan. Proverite status.";
                await EmailHelper.SendEmailAsync(email, subject, body);
            }
        }

        public override void OnStop()
        {
            _timer?.Dispose();
            base.OnStop();
        }
    }
}