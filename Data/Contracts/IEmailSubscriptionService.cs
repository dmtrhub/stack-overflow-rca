using System.Collections.Generic;
using System.ServiceModel;

namespace Data.Contracts
{
    [ServiceContract]
    public interface IEmailSubscriptionService
    {
        [OperationContract]
        List<string> GetAllEmails();

        [OperationContract]
        void AddEmail(string email);

        [OperationContract]
        void RemoveEmail(string email);
    }
}