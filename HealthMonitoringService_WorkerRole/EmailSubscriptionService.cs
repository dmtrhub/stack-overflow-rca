using Data.Contracts;
using Data.Repositories;
using System.Collections.Generic;
using System.ServiceModel;

namespace HealthMonitoringService_WorkerRole
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EmailSubscriptionService : IEmailSubscriptionService
    {
        private readonly AlertEmailRepository _alertRepo;

        public EmailSubscriptionService(AlertEmailRepository alertRepo)
        {
            _alertRepo = alertRepo;
        }

        public List<string> GetAllEmails()
        {
            return _alertRepo.GetAllEmailsAsync().Result;
        }

        public void AddEmail(string email)
        {
            _alertRepo.AddEmailAsync(email).Wait();
        }

        public void RemoveEmail(string email)
        {
            _alertRepo.RemoveEmailAsync(email).Wait();
        }
    }
}