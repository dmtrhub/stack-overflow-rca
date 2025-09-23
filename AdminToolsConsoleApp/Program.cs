using Data.Contracts;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AdminToolsConsoleApp
{
    public class Program
    {
        private static IEmailSubscriptionService _emailClient;

        private static async Task Main(string[] args)
        {
            // Kreiranje WCF klijenta
            var factory = new ChannelFactory<IEmailSubscriptionService>("NetTcpBinding_IEmailSubscriptionService");
            _emailClient = factory.CreateChannel();

            while (true)
            {
                Console.WriteLine("1. Prikaz svih mejlova");
                Console.WriteLine("2. Dodaj mejl");
                Console.WriteLine("3. Obrisi mejl");
                Console.WriteLine("0. Izlaz");

                switch (Console.ReadLine())
                {
                    case "1":
                        var emails = _emailClient.GetAllEmails();
                        emails.ForEach(e => Console.WriteLine(e));
                        break;

                    case "2":
                        Console.Write("Unesite mejl: ");
                        _emailClient.AddEmail(Console.ReadLine());
                        Console.WriteLine("Mejl uspesno dodat.");
                        break;

                    case "3":
                        Console.Write("Unesite mejl za brisanje: ");
                        _emailClient.RemoveEmail(Console.ReadLine());
                        Console.WriteLine("Mejl uspesno obrisan.");
                        break;

                    case "0":
                        return;
                }
            }
        }
    }
}