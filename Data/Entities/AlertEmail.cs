using Microsoft.WindowsAzure.Storage.Table;

namespace Data.Entities
{
    // Za čuvanje mejlova u AlertEmails tabeli
    public class AlertEmail : TableEntity
    {
        public AlertEmail()
        { }

        public AlertEmail(string email)
        {
            PartitionKey = "AlertEmails";
            RowKey = email;
        }

        public string Email { get; set; }
    }
}