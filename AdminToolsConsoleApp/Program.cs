using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AdminToolsConsoleApp
{
    public class Program
    {
        private static readonly HttpClient _client = new HttpClient();
        private const string apiBase = "http://localhost:5003/emails";

        private static async Task Main(string[] args)
        {
            var serializer = new JavaScriptSerializer();

            while (true)
            {
                Console.WriteLine("1. Prikaz svih mejlova");
                Console.WriteLine("2. Dodaj mejl");
                Console.WriteLine("3. Obrisi mejl");
                Console.WriteLine("0. Izlaz");
                Console.Write("Izaberite opciju: ");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        var resp = await _client.GetStringAsync(apiBase);
                        var emails = serializer.Deserialize<List<string>>(resp);
                        emails.ForEach(e => Console.WriteLine(e));
                        break;

                    case "2":
                        Console.Write("Unesite mejl: ");
                        var newEmail = Console.ReadLine();
                        var postData = new { email = newEmail };
                        var content = new StringContent(serializer.Serialize(postData), Encoding.UTF8, "application/json");
                        await _client.PostAsync(apiBase, content);
                        Console.WriteLine("Mejl dodat.");
                        break;

                    case "3":
                        Console.Write("Unesite mejl za brisanje: ");
                        var delEmail = Console.ReadLine();
                        var deleteData = new { email = delEmail };
                        var deleteContent = new StringContent(serializer.Serialize(deleteData), Encoding.UTF8, "application/json");
                        var request = new HttpRequestMessage(HttpMethod.Delete, apiBase) { Content = deleteContent };
                        await _client.SendAsync(request);
                        Console.WriteLine("Mejl obrisan.");
                        break;

                    case "0":
                        return;
                }

                Console.WriteLine();
            }
        }
    }
}