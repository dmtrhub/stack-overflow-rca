using Data.Repositories;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.ServiceModel;

namespace HealthMonitoringService_WorkerRole
{
    public class WcfServiceHost
    {
        private ServiceHost _emailServiceHost;

        public void Start()
        {
            // Kreiranje repozitorijuma
            var connectionString = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");
            var alertRepo = new AlertEmailRepository(connectionString);

            // Email service host
            _emailServiceHost = new ServiceHost(new EmailSubscriptionService(alertRepo));

            _emailServiceHost.Open();
        }

        public void Stop()
        {
            _emailServiceHost?.Close();
        }
    }
}